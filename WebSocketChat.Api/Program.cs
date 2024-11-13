using StackExchange.Redis;
using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Base;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Model;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddLogging();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis"));

WebApplication app = builder.Build();

app.MapGet(ChatHubRequest.Route, async (IConnectionMultiplexer redis, HttpContext context) => {
    if (context.WebSockets.IsWebSocketRequest)
    {
        await using WebSocketServer<ChatHubMessage, ChatHubRequest, ChatHubResponse> handler = ChatHubEndpoint.HandleServer
        (
            connection: await context.WebSockets.AcceptWebSocketAsync(),
            logger: app.Logger
        );
        
        IDatabase redisDatabase = redis.GetDatabase();
        
        Guid? peerId = null;
        
        handler.OnError += (e) =>
        {
            app.Logger.LogError(e.ToString());  
        };

        handler.OnMessage += async (message) => {
            switch (message.MessageType)
            {
                case ChatHubMessageType.Heartbeat:
                    await redisDatabase.HashSetAsync($"Peer:{peerId}", [
                        new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString())
                    ]);
                    break;
            }
        };

        handler.OnRequest += async (request) => {
            switch (request.RequestType)
            {
                case ChatHubRequestType.Login:
                    if (peerId == null)
                    {
                        peerId = request.PeerId;
                        
                        await redisDatabase.SetAddAsync("PeersConnected", peerId.ToString());
                        await redisDatabase.SetAddAsync("Peers", peerId.ToString());
                        
                        await redisDatabase.HashSetAsync($"Peer:{peerId}", [
                            new HashEntry("Nickname", request.Nickname),
                            new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString())
                        ]);
                    
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Success
                        });
                    }
                    else
                    {
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Failure
                        });
                    }
                    
                    break;
                case ChatHubRequestType.SetNickname:
                    if (peerId != null)
                    {
                        await redisDatabase.HashSetAsync($"Peer:{peerId}", [
                            new HashEntry("Nickname", request.Nickname),
                            new HashEntry("LastSeen", DateTimeOffset.UtcNow.ToString())
                        ]);
                    
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Success
                        });
                    }
                    else
                    {
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Failure
                        });
                    }

                    break;
                case ChatHubRequestType.Logout:
                    if (peerId != null)
                    {
                        await redisDatabase.SetRemoveAsync("PeersConnected", peerId.ToString());
                    
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Success
                        });
                    }
                    else
                    {
                        await handler.Send(new ChatHubResponse()
                        {
                            ResponseType = ChatHubResponseType.Failure
                        });
                    }
                    
                    break;
                case ChatHubRequestType.GetPeersConnected:
                    List<Peer> peers = [];
                    
                    foreach (RedisValue peerId in await redisDatabase.SetMembersAsync("PeersConnected"))
                    {
                        RedisValue[] peerData = await redisDatabase.HashGetAsync($"Peer:{peerId}", ["Nickname", "LastSeen"]);
                        
                        peers.Add(new Peer()
                        {
                            Id = Guid.Parse(peerId),
                            Nickname = peerData[0],
                            LastSeen = DateTimeOffset.Parse(peerData[1])
                        });
                    }
                    
                    await handler.Send(new ChatHubResponse()
                    {
                        ResponseType = ChatHubResponseType.PeersConnected,
                        Peers = peers
                    });
                    
                    break;
            }
        };

        handler.OnClose += async (status, description) => {
            await redisDatabase.SetRemoveAsync("PeersConnected", peerId.ToString());
            Console.WriteLine("{0} {1}", status, description);
        };
        
        await handler.Wait();
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.UseWebSockets();

await app.RunAsync();