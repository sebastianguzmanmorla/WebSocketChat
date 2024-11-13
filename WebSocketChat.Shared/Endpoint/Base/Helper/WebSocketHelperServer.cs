using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace WebSocketChat.Shared.Endpoint.Base.Helper;

public sealed class WebSocketHelperServer
(
    WebSocket connection,
    CancellationToken? cancelToken = null,
    ILogger? logger = null
) : WebSocketHelperBase<WebSocket>(connection, cancelToken, logger)
{
    private readonly TaskCompletionSource _closeSource = new();

    public Task Wait()
    {
        OnClose += (_, _) =>
        {
            _closeSource.TrySetResult();
        };
        
        return _closeSource.Task;
    }
}