using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace WebSocketChat.Shared.Endpoint.Base.Helper;

public sealed class WebSocketHelperClient : WebSocketHelperBase<ClientWebSocket>
{
    private CancellationTokenSource CancelTokenSource { get; set; }

    public new event ErrorHandler? OnError;

    internal WebSocketHelperClient
    (
        ILogger? logger = null
    ) : base(new ClientWebSocket(), null, logger)
    {
        CancelTokenSource = new CancellationTokenSource();

        CancelToken = CancelTokenSource.Token;

        base.OnError += (e) => OnError?.Invoke(e);
    }

    public Uri? Endpoint { get; internal set; }

    public async Task<bool> Connect()
    {
        try
        {
            if (Endpoint == null)
            {
                throw new NullReferenceException($"{nameof(Endpoint)} is null");
            }

            switch (State)
            {
                case WebSocketState.Open:
                    break;
                case WebSocketState.Connecting:

                    while (State == WebSocketState.Connecting)
                    {
                        await Task.Delay(100);
                    }

                    break;
                default:
                    await CancelTokenSource.CancelAsync();

                    CancelTokenSource = new CancellationTokenSource();

                    CancelToken = CancelTokenSource.Token;

                    Connection = new ClientWebSocket();

                    await Connection.ConnectAsync(Endpoint, CancellationToken.None);

                    _ = Task.Run(ReceiveTask, CancellationToken.None);

                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);

            return false;
        }
    }
}