using Microsoft.Extensions.Logging;
using WebSocketChat.Shared.Endpoint.Base;
using WebSocketChat.Shared.Endpoint.Payload;

namespace WebSocketChat.Shared.Endpoint;

public class ChatHubEndpoint
(
    ILogger? logger = null
) : WebSocketClient<ChatHubMessage, ChatHubRequest, ChatHubResponse>(logger)
{
    protected override string Path => ChatHubRequest.Route;
}