using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Payload;

Guid peerId = Guid.CreateVersion7();

ChatHubEndpoint chatHubEndpoint = new()
{
    Host = "ws://localhost:5230/"
};

chatHubEndpoint.OnError += (e) => {
    Console.WriteLine(e);
    
};

await chatHubEndpoint.Connect();

ChatHubResponse? loginResponse = await chatHubEndpoint.Send(new ChatHubRequest()
{
    PeerId = peerId,
    RequestType = ChatHubRequestType.Login,
    Nickname = "John Doe"
});

ChatHubResponse? peerListResponse = await chatHubEndpoint.Send(new ChatHubRequest()
{
    PeerId = peerId,
    RequestType = ChatHubRequestType.GetPeersConnected
});

ChatHubResponse? logoutResponse = await chatHubEndpoint.Send(new ChatHubRequest()
{
    PeerId = peerId,
    RequestType = ChatHubRequestType.Logout
});

peerListResponse = await chatHubEndpoint.Send(new ChatHubRequest()
{
    PeerId = peerId,
    RequestType = ChatHubRequestType.GetPeersConnected
});

await chatHubEndpoint.Disconnect("bye");

Console.WriteLine(loginResponse);