using System.Net.WebSockets;
using System.Text.Json;
using WebSocketChat.Shared.Endpoint.Base.Helper;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base;

public abstract class WebSocketBase<TWebSocketHelper, TWebSocket> : IAsyncDisposable
    where TWebSocketHelper : WebSocketHelperBase<TWebSocket>
    where TWebSocket : WebSocket
{
    internal TWebSocketHelper Helper { get; }


    public delegate void MessageHandler(IMessage message);
    public delegate void RequestHandler(IRequest request);
    public delegate void ResponseHandler(IResponse response);

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
                case IMessage message:
                    OnMessage?.Invoke(message);
                    break;
                case IRequest request:
                    OnRequest?.Invoke(request);
                    break;
                case IResponse response:
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

    public async Task<TResponse?> SendRequest<TRequest, TResponse>(TRequest request) where TRequest : RequestBase<TResponse> where TResponse : ResponseBase
    {
        TaskCompletionSource<TResponse?> responseSource = new();

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

        void OnResponseHandler(IResponse? serverResponse)
        {
            responseSource.SetResult((TResponse?)serverResponse);
        }
    }

    public async Task<bool> SendResponse<TResponse>(TResponse response) where TResponse : ResponseBase => await Helper.Send(response);

    public async Task<bool> SendMessage<TMessage>(TMessage message) where TMessage : MessageBase => await Helper.Send(message);

    public async Task<bool> SendBytes(byte[] bytes) => await Helper.Send(bytes);

    public async Task<bool> SendText(string text) => await Helper.Send(text);
    
    public async ValueTask DisposeAsync()
    {
        await Helper.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}