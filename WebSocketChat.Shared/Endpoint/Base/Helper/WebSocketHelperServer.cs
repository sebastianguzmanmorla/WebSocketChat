using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace WebSocketChat.Shared.Endpoint.Base.Helper;

public sealed class WebSocketHelperServer : WebSocketHelperBase<WebSocket>
{
    private readonly TaskCompletionSource _closeSource;

    public WebSocketHelperServer
    (
        WebSocket connection,
        CancellationToken? cancelToken = null,
        ILogger? logger = null
    ) : base(connection, cancelToken, logger)
    {
        _closeSource = new();

        OnReceiveClose += (_, _) =>
        {
            _closeSource.SetResult();
        };
    }

    public Task Wait()
    {
        return _closeSource.Task;
    }
}