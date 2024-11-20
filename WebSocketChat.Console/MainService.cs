using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Console;

public class MainService
(
    Settings settings,
    ChatHubEndpoint chatHubEndpoint,
    IHostApplicationLifetime App,
    ILogger<MainService> Logger
) : BackgroundService
{
    public Dictionary<Guid, Peer> Peers { get; set; } = [];

    protected override async Task ExecuteAsync(CancellationToken CancelToken)
    {
        chatHubEndpoint.OnError += (ex) => Logger.LogError($"Exception: {ex.Message}");
        chatHubEndpoint.OnClose += (status, description) => Logger.LogInformation($"CloseStatus: {status} Description: {description}");
        chatHubEndpoint.OnMessage += ChatHubEndpoint_OnMessage;
        chatHubEndpoint.OnRequest += ChatHubEndpoint_OnRequest;

        string? Nickname = null;

        do
        {
            System.Console.WriteLine("Enter Nickname:");
            Nickname = System.Console.ReadLine();
        }
        while (string.IsNullOrEmpty(Nickname));

        if (!await chatHubEndpoint.Connect())
        {
            Logger.LogError("Not Connected");

            App.StopApplication();

            return;
        }

        SuccessResponse? loginResponse = null;

        do
        {
            LoginRequest loginRequest = new()
            {
                PeerId = settings.PeerId.GetValueOrDefault(),
                Nickname = Nickname
            };

            loginResponse = await chatHubEndpoint.SendRequest<LoginRequest, SuccessResponse>(loginRequest);
        } while (!loginResponse?.Success ?? true);

        GetPeersResponse? peerListResponse = await chatHubEndpoint.SendRequest<GetPeersRequest, GetPeersResponse>(new GetPeersRequest
        {
            PeerId = settings.PeerId.GetValueOrDefault()
        });

        if (peerListResponse != null)
        {
            Peers = peerListResponse.Peers.ToDictionary(x => x.Id, x => x);
        }

        do
        {
            await chatHubEndpoint.SendMessage(new HeartbeatMessage()
            {
                PeerId = settings.PeerId.GetValueOrDefault()
            });

            await Task.Delay(5000, CancelToken).ContinueWith(x => x.Exception == default);
        } while (!CancelToken.IsCancellationRequested);

        SuccessResponse? logoutResponse = await chatHubEndpoint.SendRequest<LogoutRequest, SuccessResponse>(new LogoutRequest
        {
            PeerId = settings.PeerId.GetValueOrDefault()
        });

        await chatHubEndpoint.Disconnect("bye");
    }

    private void ChatHubEndpoint_OnRequest(IRequest request)
    {
        throw new NotImplementedException();
    }

    private void ChatHubEndpoint_OnMessage(IMessage message)
    {
        switch (message)
        {
            case SendTextMessage sendTextMessage:
                if (Peers.TryGetValue(sendTextMessage.PeerId, out Peer? peer))
                {
                    Logger.LogInformation($"{peer.Nickname} : {sendTextMessage.Text}");
                }
                break;
            case PeerConnectedMessage peerConnectedMessage:
                if (Peers.TryGetValue(peerConnectedMessage.Peer.Id, out Peer? peerConnected))
                {
                    peerConnected.Nickname = peerConnectedMessage.Peer.Nickname;
                    peerConnected.LastSeen = peerConnectedMessage.Peer.LastSeen;
                }
                else
                {
                    Peers.Add(peerConnectedMessage.Peer.Id, peerConnectedMessage.Peer);
                }

                Logger.LogInformation($"{peerConnectedMessage.Peer.Nickname} Connected!");
                break;
            case PeerDisconnectedMessage peerDisconnectedMessage:
                Peers.Remove(peerDisconnectedMessage.Peer.Id);

                Logger.LogInformation($"{peerDisconnectedMessage.Peer.Nickname} Disconnected :(");
                break;
        }
    }
}
