using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Payload;

Guid peerId = Guid.CreateVersion7();

ChatHubEndpoint chatHubEndpoint = new()
{
    Host = "ws://localhost:5230/"
};

chatHubEndpoint.OnError += Console.WriteLine;

await chatHubEndpoint.Connect();

ChatHubLoginRequest loginRequest = new()
{
    PeerId = peerId,
    Nickname = "John Doe"
};

ChatHubSuccessResponse? loginResponse = await chatHubEndpoint.SendRequest<ChatHubLoginRequest, ChatHubSuccessResponse>(loginRequest);

ChatHubGetPeersConnectedResponse? peerListResponse = await chatHubEndpoint.SendRequest<ChatHubGetPeersConnectedRequest, ChatHubGetPeersConnectedResponse>(new ChatHubGetPeersConnectedRequest
{
    PeerId = peerId
});

ChatHubSuccessResponse? logoutResponse = await chatHubEndpoint.SendRequest<ChatHubLogoutRequest, ChatHubSuccessResponse>(new ChatHubLogoutRequest
{
    PeerId = peerId
});

peerListResponse = await chatHubEndpoint.SendRequest<ChatHubGetPeersConnectedRequest, ChatHubGetPeersConnectedResponse>(new ChatHubGetPeersConnectedRequest
{
    PeerId = peerId
});

await chatHubEndpoint.Disconnect("bye");

Console.WriteLine(loginResponse);