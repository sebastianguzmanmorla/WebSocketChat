using Microsoft.Extensions.Logging;
using WebSocketChat.Shared.Endpoint.Base;

namespace WebSocketChat.Shared.Endpoint;

public class ChatHubEndpoint
(
    ILogger? logger = null
) : WebSocketClient(logger)
{
    public new const string Path = "/";

    public override string GetPath() => Path;
}