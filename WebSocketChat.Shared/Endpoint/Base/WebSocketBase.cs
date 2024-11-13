using System.Net.WebSockets;
using System.Text.Json;
using WebSocketChat.Shared.Endpoint.Base.Helper;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base;

public abstract class WebSocketBase<TMessage, TRequest, TResponse, TWebSocketHelper, TWebSocket> : IAsyncDisposable
    where TMessage : MessageBase
    where TRequest : RequestBase
    where TResponse : ResponseBase
    where TWebSocketHelper : WebSocketHelperBase<TWebSocket>
    where TWebSocket : WebSocket
{
    internal TWebSocketHelper Helper { get; }


    public delegate void MessageHandler(TMessage message);
    public delegate void RequestHandler(TRequest request);
    public delegate void ResponseHandler(TResponse response);

    public event MessageHandler? OnMessage;
    public event RequestHandler? OnRequest;
    public event ResponseHandler? OnResponse;
    public event WebSocketHelperBase<TWebSocket>.MessageTextHandler? OnText;

    public event WebSocketHelperBase<TWebSocket>.MessageBinaryHandler? OnBinary;
    public event WebSocketHelperBase<TWebSocket>.ErrorHandler? OnError;
    public event WebSocketHelperBase<TWebSocket>.CloseHandler? OnClose;

    public WebSocketState State => Helper.State;

    public int RequestTimeout { get; init; } = 2000;

    internal WebSocketBase(TWebSocketHelper webSocketHelper)
    {
        Helper = webSocketHelper;

        webSocketHelper.OnReceiveText += MessageTextHandler;

        webSocketHelper.OnReceiveBinary += (bytes) => OnBinary?.Invoke(bytes);
        webSocketHelper.OnError += (e) => OnError?.Invoke(e);
        webSocketHelper.OnClose += (status, description) => OnClose?.Invoke(status, description);
    }

    private void MessageTextHandler(string text)
    {
        PayloadBase? payload = null;

        try
        {
            payload = JsonSerializer.Deserialize<PayloadBase>(text, WebSocketHelperBase<TWebSocket>.SerializerOptions);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }

        if (payload is not null)
        {
            switch (payload)
            {
                case TMessage message:
                    OnMessage?.Invoke(message);
                    break;
                case TRequest request:
                    OnRequest?.Invoke(request);
                    break;
                case TResponse response:
                    OnResponse?.Invoke(response);
                    break;
                default:
                    OnError?.Invoke(new NotSupportedException());
                    break;
            }
        }
        else
        {
            OnText?.Invoke(text);
        }
    }

    public async virtual Task<TResponse?> Send(TRequest request)
    {
        TaskCompletionSource<TResponse?> responseSource = new();

        void OnResponseHandler(TResponse? response)
        {
            responseSource.SetResult(response);
        }

        OnResponse += OnResponseHandler;

        TResponse? response;

        if (await Helper.Send(request))
        {
            response = Task.WaitAny([responseSource.Task], RequestTimeout) == 0 ? responseSource.Task.Result : null;
        }
        else
        {
            response = null;
        }

        OnResponse -= OnResponseHandler;

        return response;
    }

    public async virtual Task<bool> Send(TResponse response) => await Helper.Send(response);

    public async virtual Task<bool> Send(TMessage message) => await Helper.Send(message);

    public async virtual Task<bool> Send(byte[] bytes) => await Helper.Send(bytes);

    public async virtual Task<bool> Send(string text) => await Helper.Send(text);
    
    public async ValueTask DisposeAsync()
    {
        await Helper.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}