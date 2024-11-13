using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using WebSocketChat.Shared.Endpoint.Base.Helper;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base;

public sealed class WebSocketServer<TMessage, TRequest, TResponse>
(
    WebSocket connection,
    CancellationToken? cancelToken = null,
    ILogger? logger = null
) : WebSocketBase<TMessage, TRequest, TResponse, WebSocketHelperServer, WebSocket>(new WebSocketHelperServer(connection, cancelToken, logger))
    where TMessage : MessageBase
    where TRequest : RequestBase
    where TResponse : ResponseBase
{
    public Task Wait()
    {
        return Helper.Wait();
    }
}