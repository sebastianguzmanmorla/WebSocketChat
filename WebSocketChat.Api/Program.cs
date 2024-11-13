using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Base;
using WebSocketChat.Shared.Endpoint.Payload;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddLogging();

WebApplication app = builder.Build();

app.MapGet(ChatHubRequest.Route, async (context) => {
    if (context.WebSockets.IsWebSocketRequest)
    {
        await using WebSocketServer<ChatHubMessage, ChatHubRequest, ChatHubResponse> handler = ChatHubEndpoint.HandleServer
        (
            connection: await context.WebSockets.AcceptWebSocketAsync(),
            logger: app.Logger
        );
        
        handler.OnError += (e) =>
        {
            app.Logger.LogError(e.ToString());  
        };

        handler.OnMessage += ((message) => {
            switch (message.MessageType)
            {
                case ChatHubMessageType.Heartbeat:
                    break;
            }
        });

        handler.OnRequest += async (request) => {
            switch (request.RequestType)
            {
                case ChatHubRequestType.Login:
                    await handler.Send(new ChatHubResponse()
                    {
                        ResponseType = ChatHubResponseType.Success
                    });
                    break;
                case ChatHubRequestType.Logout:
                    break;
                case ChatHubRequestType.PeerList:
                    break;
            }
        };

        handler.OnClose += (status, description) => {
            Console.WriteLine("{0} {1}", status, description);
        };
        
        await handler.Wait();
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

WebSocketOptions webSocketOptions = new()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

await app.RunAsync();