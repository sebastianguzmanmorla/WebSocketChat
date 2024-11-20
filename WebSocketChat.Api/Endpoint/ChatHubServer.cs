using StackExchange.Redis;
using System.Globalization;
using System.Net.WebSockets;
using System.Text.Json;
using WebSocketChat.Shared.Endpoint.Base;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Api.Endpoint;

public class ChatHubServer : IAsyncDisposable
{
    private WebSocketServer Handler { get; }
    private IDatabase RedisDatabase { get; }
    private ISubscriber RedisSubscriber { get; }
    private ILogger Logger { get; }
    private Guid? PeerId { get; set; }

    private static readonly RedisChannel TextMessage = RedisChannel.Literal($"TextMessage");
    private ChannelMessageQueue? TextMessageSended { get; set; }

    private static readonly RedisChannel PeerConnect = RedisChannel.Literal($"PeerConnect");
    private ChannelMessageQueue? PeerConnected { get; set; }

    private static RedisChannel PeerDisconnect = RedisChannel.Literal($"PeerDisconnect");
    private ChannelMessageQueue? PeerDisconnected { get; set; }

    public ChatHubServer(WebSocket connection, IConnectionMultiplexer redis, ILogger logger)
    {
        Handler = new WebSocketServer(connection, CancellationToken.None, logger);

        Handler.OnError += OnHandlerOnError;

        Handler.OnMessage += OnHandlerOnMessage;

        Handler.OnRequest += OnHandlerOnRequest;

        Handler.OnClose += OnHandlerOnClose;

        RedisDatabase = redis.GetDatabase();

        RedisSubscriber = redis.GetSubscriber();

        Logger = logger;
    }

    public Task Wait() => Handler.Wait();

    private void OnHandlerOnError(Exception ex)
    {
        Logger.LogError($"Peer: {PeerId} Exception: {ex}");
    }

    private async void OnHandlerOnMessage(IMessage message)
    {
        switch (message)
        {
            case HeartbeatMessage:
                if (PeerId != null)
                {
                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);

                    Logger.LogInformation($"{nameof(HeartbeatMessage)} Peer: {PeerId}");
                }
                break;
            case SendTextMessage sendTextMessage:
                await RedisSubscriber.PublishAsync(TextMessage, JsonSerializer.Serialize(sendTextMessage));
                Logger.LogInformation($"{nameof(SendTextMessage)} Peer: {PeerId}");
                break;
        }
    }

    private async void OnHandlerOnRequest(IRequest request)
    {
        switch (request)
        {
            case LoginRequest loginRequest:
                if (PeerId == null)
                {
                    PeerId = loginRequest.PeerId;

                    await RedisDatabase.SetAddAsync("PeersConnected", PeerId.ToString());
                    await RedisDatabase.SetAddAsync("Peers", PeerId.ToString());

                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("Nickname", loginRequest.Nickname),
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);

                    await RedisSubscriber.PublishAsync(PeerConnect, PeerId.ToString());

                    TextMessageSended = await RedisSubscriber.SubscribeAsync(TextMessage);

                    TextMessageSended.OnMessage(async (message) =>
                    {
                        if (Handler.State == WebSocketState.Open && message.Message.HasValue)
                        {
                            _ = await Handler.SendText(message.Message!);
                        }
                    });

                    PeerConnected = await RedisSubscriber.SubscribeAsync(PeerConnect);

                    PeerConnected.OnMessage(async (message) =>
                    {
                        if (Handler.State == WebSocketState.Open && message.Message.HasValue)
                        {
                            Guid peerId = Guid.Parse(message.Message!);

                            if (PeerId == peerId)
                            {
                                return;
                            }

                            RedisValue[] peerData = await RedisDatabase.HashGetAsync($"Peer:{peerId}", ["Nickname", "LastSeen"]);

                            if (peerData.Length == 2 && peerData[0] != RedisValue.Null && peerData[1] != RedisValue.Null)
                            {
                                _ = await Handler.SendMessage(new PeerConnectedMessage
                                {
                                    PeerId = PeerId.GetValueOrDefault(),
                                    Peer = new Peer
                                    {
                                        Id = peerId,
                                        Nickname = peerData[0]!,
                                        LastSeen = DateTimeOffset.Parse(peerData[1]!, CultureInfo.InvariantCulture)
                                    }
                                });
                            }
                        }
                    });

                    PeerDisconnected = await RedisSubscriber.SubscribeAsync(PeerDisconnect);

                    PeerDisconnected.OnMessage(async (message) =>
                    {
                        if (Handler.State == WebSocketState.Open && message.Message.HasValue)
                        {
                            Guid peerId = Guid.Parse(message.Message!);

                            if (PeerId == peerId)
                            {
                                return;
                            }

                            RedisValue[] peerData = await RedisDatabase.HashGetAsync($"Peer:{peerId}", ["Nickname", "LastSeen"]);

                            if (peerData.Length == 2 && peerData[0] != RedisValue.Null && peerData[1] != RedisValue.Null)
                            {
                                _ = await Handler.SendMessage(new PeerDisconnectedMessage
                                {
                                    PeerId = PeerId.GetValueOrDefault(),
                                    Peer = new Peer
                                    {
                                        Id = peerId,
                                        Nickname = peerData[0]!,
                                        LastSeen = DateTimeOffset.Parse(peerData[1]!, CultureInfo.InvariantCulture)
                                    }
                                });
                            }
                        }
                    });

                    Logger.LogInformation($"{nameof(LoginRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case SetNicknameRequest setNicknameRequest:
                if (PeerId != null)
                {
                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("Nickname", setNicknameRequest.Nickname),
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);

                    Logger.LogInformation($"{nameof(SetNicknameRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case LogoutRequest:
                if (PeerId != null)
                {
                    await RedisDatabase.SetRemoveAsync("PeersConnected", PeerId.ToString());

                    await RedisSubscriber.PublishAsync(PeerDisconnect, PeerId.ToString());

                    Logger.LogInformation($"{nameof(LogoutRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new SuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case GetPeersRequest:
                List<Peer> peers = [];

                foreach (RedisValue peerId in await RedisDatabase.SetMembersAsync("PeersConnected"))
                {
                    RedisValue[] peerData = await RedisDatabase.HashGetAsync($"Peer:{peerId}", ["Nickname", "LastSeen"]);

                    if (peerData.Length == 2 && peerData[0] != RedisValue.Null && peerData[1] != RedisValue.Null)
                    {
                        peers.Add(new Peer
                        {
                            Id = Guid.Parse(peerId!),
                            Nickname = peerData[0]!,
                            LastSeen = DateTimeOffset.Parse(peerData[1]!, CultureInfo.InvariantCulture)
                        });
                    }
                }

                Logger.LogInformation($"{nameof(GetPeersRequest)} Peer: {PeerId}");

                await Handler.SendResponse(new GetPeersResponse
                {
                    Peers = peers
                });

                break;
        }
    }

    private async void OnHandlerOnClose(WebSocketCloseStatus status, string? description)
    {
        if (PeerId == null) return;

        await RedisDatabase.SetRemoveAsync("PeersConnected", PeerId.ToString());

        Logger.LogInformation($"Peer: {PeerId} CloseStatus: {status} Description: {description}");
    }

    public async ValueTask DisposeAsync()
    {
        if (TextMessageSended != null)
        {
            await TextMessageSended.UnsubscribeAsync();

            TextMessageSended = null;
        }

        if (PeerConnected != null)
        {
            await PeerConnected.UnsubscribeAsync();

            PeerConnected = null;
        }

        if (PeerDisconnected != null)
        {
            await PeerDisconnected.UnsubscribeAsync();

            PeerDisconnected = null;
        }

        Handler.OnError -= OnHandlerOnError;

        Handler.OnMessage -= OnHandlerOnMessage;

        Handler.OnRequest -= OnHandlerOnRequest;

        Handler.OnClose -= OnHandlerOnClose;

        await Handler.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}