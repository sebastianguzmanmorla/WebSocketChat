using WebSocketChat.Shared.Endpoint.Base.Interfaces;

namespace WebSocketChat.Shared.Endpoint.Base.Payload;

public abstract class RequestBase<TResponse> : PayloadBase, IRequest where TResponse : ResponseBase
{
}