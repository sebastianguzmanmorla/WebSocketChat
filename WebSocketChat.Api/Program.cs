using StackExchange.Redis;
using WebSocketChat.Api.Endpoint;
using WebSocketChat.Shared.Endpoint;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLogging();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis"));

WebApplication app = builder.Build();

app.MapGet(ChatHubEndpoint.Path, async (IConnectionMultiplexer redis, HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        await using ChatHubServer handler = new(
            connection: await context.WebSockets.AcceptWebSocketAsync(),
            redis: redis,
            logger: app.Logger
        );

        await handler.Wait();
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.UseWebSockets();

await app.RunAsync();