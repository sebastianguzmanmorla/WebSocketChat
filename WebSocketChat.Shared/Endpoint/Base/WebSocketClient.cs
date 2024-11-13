using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using WebSocketChat.Shared.Endpoint.Base.Helper;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base;

public abstract class WebSocketClient<TMessage, TRequest, TResponse> : WebSocketBase<TMessage, TRequest, TResponse, WebSocketHelperClient, ClientWebSocket>
    where TMessage : MessageBase
    where TRequest : RequestBase
    where TResponse : ResponseBase
{
    private string _host = "";
    public string Host
    {
        get => _host;
        set
        {
            if (value != _host)
            {
                _host = value;

                UpdateUri();
            }
        }
    }

    private async void UpdateUri()
    {
        UriBuilder uriBuilder = new(_host)
        {
            Path = Path
        };

        if (Helper.State == WebSocketState.Open)
        {
            await Helper.Close();
        }

        Helper.Endpoint = uriBuilder.Uri;
    }

    protected abstract string Path { get; }

    public new event WebSocketHelperBase<ClientWebSocket>.ErrorHandler? OnError;

    protected WebSocketClient
    (
        ILogger? logger = null
    ) : base(new WebSocketHelperClient(logger))
    {
        Helper.OnError += (e) => OnError?.Invoke(e);
    }

    public async Task<bool> Connect()
    {
        if (Helper.State != WebSocketState.Open && !await Helper.Connect())
        {
            OnError?.Invoke(new("WebSocketClientHelper is not connected."));

            return false;
        }

        return true;
    }

    public async Task Disconnect(string? description = null) => await Helper.Close(description : description);

    public override async Task<TResponse?> Send(TRequest request)
    {
        if (Helper.State != WebSocketState.Open)
        {
            return null;
        }

        return await base.Send(request);
    }

    public override async Task<bool> Send(TResponse response) => Helper.State == WebSocketState.Open && await base.Send(response);

    public override async Task<bool> Send(TMessage message) => Helper.State == WebSocketState.Open && await base.Send(message);

    public override async Task<bool> Send(byte[] message) => Helper.State == WebSocketState.Open && await base.Send(message);

    public override async Task<bool> Send(string message) => Helper.State == WebSocketState.Open && await base.Send(message);

    public static WebSocketServer<TMessage, TRequest, TResponse> HandleServer
    (
        WebSocket connection,
        CancellationToken? cancelToken = null,
        ILogger? logger = null
    ) => new(connection, cancelToken, logger);
}