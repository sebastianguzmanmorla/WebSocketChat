using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using WebSocketChat.Shared.Endpoint.Base.Helper;

namespace WebSocketChat.Shared.Endpoint.Base;

public sealed class WebSocketServer
(
    WebSocket connection,
    CancellationToken? cancelToken = null,
    ILogger? logger = null
) : WebSocketBase<WebSocketHelperServer, WebSocket>(new WebSocketHelperServer(connection, cancelToken, logger))
{
    public Task Wait()
    {
        return Helper.Wait();
    }
}