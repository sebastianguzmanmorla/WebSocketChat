using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Base.Helper;

public abstract class WebSocketHelperBase<TWebSocket> : IAsyncDisposable
    where TWebSocket : WebSocket
{
    private const int BufferSize = 1024 * 4;

    public static readonly JsonSerializerOptions? SerializerOptions = new()
    {
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public delegate void ErrorHandler(Exception exception);
    public delegate void MessageBinaryHandler(byte[] bytes);
    public delegate void MessageTextHandler(string message);
    public delegate void CloseHandler(WebSocketCloseStatus status, string? description);

    public event ErrorHandler? OnError;
    public event MessageBinaryHandler? OnReceiveBinary;
    public event MessageTextHandler? OnReceiveText;
    public event CloseHandler? OnClose;

    public WebSocketState State => Connection?.State ?? WebSocketState.None;

    internal TWebSocket? Connection;
    internal CancellationToken CancelToken;
    private readonly ILogger? _logger;

    protected WebSocketHelperBase
    (
        TWebSocket connection,
        CancellationToken? cancelToken = null,
        ILogger? logger = null
    )
    {
        Connection = connection;
        CancelToken = cancelToken ?? CancellationToken.None;
        _logger = logger;

        if (State == WebSocketState.Open)
        {
            _ = Task.Run(ReceiveTask, CancellationToken.None);
        }
    }

    internal async Task ReceiveTask()
    {
        List<byte> completeBuffer = new(BufferSize);
        byte[] buffer = new byte[BufferSize];

        try
        {
            WebSocketReceiveResult result;

            do
            {
                if (Connection is null)
                {
                    throw new NullReferenceException($"{nameof(Connection)} is null");
                }

                result = await Connection.ReceiveAsync(buffer, CancelToken);

                completeBuffer.AddRange(buffer.Take(result.Count));

                if (!result.EndOfMessage || result.MessageType == WebSocketMessageType.Close) continue;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString([.. completeBuffer]);

                    _logger?.LogDebug("Received string: {string}", message);

                    OnReceiveText?.Invoke(message);
                }
                else
                {
                    _logger?.LogDebug("Received bytes: {int}", completeBuffer.Count);

                    OnReceiveBinary?.Invoke([.. completeBuffer]);
                }

                completeBuffer.Clear();
            } while (!result.CloseStatus.HasValue && !CancelToken.IsCancellationRequested);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                OnClose?.Invoke(result.CloseStatus ?? WebSocketCloseStatus.Empty, result.CloseStatusDescription);
            }

            if (Connection.State != WebSocketState.Closed)
            {
                await Close();
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);

            await Close(WebSocketCloseStatus.InternalServerError, ex.Message);
        }
    }

    public async Task Close
    (
        WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
        string? description = null
    )
    {
        if (Connection is not null && State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                await Connection.CloseAsync(status, description, CancelToken);

                OnClose?.Invoke(status, description);

                _logger?.LogDebug("Closed WebSocket connection with status {status}", status);

                return;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        if (status != WebSocketCloseStatus.NormalClosure)
        {
            OnError?.Invoke(new InvalidOperationException("WebSocket connection is not open"));
        }
    }
    
    public async Task<bool> Send<TPayload>(TPayload payload) where TPayload : PayloadBase
    {
        string json = JsonSerializer.Serialize(payload, typeof(TPayload), SerializerOptions);

        _logger?.LogDebug("Sending JSON: {json}", json);

        return await Send(json);
    }

    public async Task<bool> Send(byte[] bytes) => await Send(WebSocketMessageType.Binary, bytes);

    public async Task<bool> Send(string message) => await Send(WebSocketMessageType.Text, Encoding.UTF8.GetBytes(message));

    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);

    private async Task<bool> Send(WebSocketMessageType type, byte[] bytes)
    {
        if (Connection is not null && State == WebSocketState.Open)
        {
            bool success;

            try
            {
                await _sendSemaphore.WaitAsync(CancelToken).ConfigureAwait(false);

                for (int i = 0; i < bytes.Length; i += BufferSize)
                {
                    int length = Math.Min(BufferSize, bytes.Length - i);

                    await Connection.SendAsync(bytes[i..(i + length)], type, i + length == bytes.Length, CancelToken).ConfigureAwait(false);
                }

                _logger?.LogDebug("Sent bytes: {int}", bytes.Length);

                success = true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);

                await Close(WebSocketCloseStatus.InternalServerError, ex.Message);

                success = false;
            }
            finally
            {
                _sendSemaphore.Release();
            }

            return success;
        }
        else
        {
            OnError?.Invoke(new InvalidOperationException("WebSocket connection is not open"));

            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Close(WebSocketCloseStatus.NormalClosure, "Disposed");

        Connection?.Dispose();

        _logger?.LogDebug("Disposed WebSocket connection");

        GC.SuppressFinalize(this);
    }
}