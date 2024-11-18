using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using WebSocketChat.Shared.Endpoint.Base.Helper;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base;

public abstract class WebSocketClient : WebSocketBase<WebSocketHelperClient, ClientWebSocket>
{
    private string _host = "";
    public string Host
    {
        get => _host;
        set
        {
            if (value == _host) return;
            
            _host = value;

            UpdateUri();
        }
    }

    private async void UpdateUri()
    {
        UriBuilder uriBuilder = new(_host)
        {
            Path = GetPath()
        };

        if (Helper.State == WebSocketState.Open)
        {
            await Helper.Close();
        }

        Helper.Endpoint = uriBuilder.Uri;
    }

    public const string Path = "/";

    public virtual string GetPath() => Path;

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
        if (Helper.State == WebSocketState.Open || await Helper.Connect()) return true;
        
        OnError?.Invoke(new Exception("WebSocketClientHelper is not connected."));

        return false;

    }

    public async Task Disconnect(string? description = null) => await Helper.Close(description : description);

    public async new Task<TResponse?> SendRequest<TRequest, TResponse>(TRequest request) where TRequest : RequestBase<TResponse> where TResponse : ResponseBase
    {
        if (Helper.State != WebSocketState.Open)
        {
            return null;
        }

        return await base.SendRequest<TRequest, TResponse>(request);
    }

    public async new Task<bool> SendResponse<TResponse>(TResponse response) where TResponse: ResponseBase => Helper.State == WebSocketState.Open && await base.SendResponse(response);

    public async new Task<bool> SendMessage<TMessage>(TMessage message) where TMessage : MessageBase => Helper.State == WebSocketState.Open && await base.SendMessage(message);

    public async new Task<bool> SendBytes(byte[] message) => Helper.State == WebSocketState.Open && await base.SendBytes(message);

    public async new Task<bool> SendText(string message) => Helper.State == WebSocketState.Open && await base.SendText(message);
}