using StackExchange.Redis;
using System.Globalization;
using System.Net.WebSockets;
using WebSocketChat.Shared.Endpoint.Base;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Api.Endpoint;

public class ChatHubServer : IAsyncDisposable
{
    private WebSocketServer Handler { get; }
    private IDatabase RedisDatabase { get; }
    private ILogger Logger { get; }
    private Guid? PeerId { get; set; }

    public ChatHubServer(WebSocket connection, IDatabase redisDatabase, ILogger logger)
    {
        Handler = new WebSocketServer(connection, CancellationToken.None, logger);

        Handler.OnError += OnHandlerOnError;

        Handler.OnMessage += OnHandlerOnMessage;

        Handler.OnRequest += OnHandlerOnRequest;

        Handler.OnClose += OnHandlerOnClose;

        RedisDatabase = redisDatabase;

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
            case ChatHubHeartbeatMessage:
                if (PeerId != null)
                {
                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);
                    
                    Logger.LogInformation($"{nameof(ChatHubHeartbeatMessage)} Peer: {PeerId}");
                }
                break;
        }
    }

    private async void OnHandlerOnRequest(IRequest request)
    {
        switch (request)
        {
            case ChatHubLoginRequest loginRequest:
                if (PeerId == null)
                {
                    PeerId = loginRequest.PeerId;

                    await RedisDatabase.SetAddAsync("PeersConnected", PeerId.ToString());
                    await RedisDatabase.SetAddAsync("Peers", PeerId.ToString());

                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("Nickname", loginRequest.Nickname),
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);
                    
                    Logger.LogInformation($"{nameof(ChatHubLoginRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case ChatHubSetNicknameRequest setNicknameRequest:
                if (PeerId != null)
                {
                    await RedisDatabase.HashSetAsync($"Peer:{PeerId}", [
                        new HashEntry("Nickname", setNicknameRequest.Nickname),
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture))
                    ]);
                    
                    Logger.LogInformation($"{nameof(ChatHubSetNicknameRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case ChatHubLogoutRequest:
                if (PeerId != null)
                {
                    await RedisDatabase.SetRemoveAsync("PeersConnected", PeerId.ToString());
                    
                    Logger.LogInformation($"{nameof(ChatHubLogoutRequest)} Peer: {PeerId}");

                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    await Handler.SendResponse(new ChatHubSuccessResponse
                    {
                        Success = false
                    });
                }

                break;
            case ChatHubGetPeersConnectedRequest:
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
                            LastSeen =  DateTimeOffset.Parse(peerData[1]!, CultureInfo.InvariantCulture)
                        });
                    }
                }
                
                Logger.LogInformation($"{nameof(ChatHubGetPeersConnectedRequest)} Peer: {PeerId}");

                await Handler.SendResponse(new ChatHubGetPeersConnectedResponse
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
        Handler.OnError -= OnHandlerOnError;

        Handler.OnMessage -= OnHandlerOnMessage;

        Handler.OnRequest -= OnHandlerOnRequest;

        Handler.OnClose -= OnHandlerOnClose;
        
        await Handler.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}