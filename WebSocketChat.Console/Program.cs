using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Payload;

Guid userId = Guid.CreateVersion7();

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
    UserId = userId,
    RequestType = ChatHubRequestType.Login
});

await chatHubEndpoint.Disconnect("bye");

Console.WriteLine(loginResponse);