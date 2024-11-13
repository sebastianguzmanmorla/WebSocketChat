namespace WebSocketChat.Shared.Endpoint.Base.Payload;

public abstract class RequestBase : PayloadBase
{
    public const string Route = "/";
    public abstract string RequestRoute();
}