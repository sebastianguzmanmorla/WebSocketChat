namespace WebSocketChat.Shared.Endpoint.Payload.Base;

using WebSocketChat.Shared.Endpoint.Base.Payload;

public class ChatHubRequest<TResponse> : RequestBase<TResponse> where TResponse : ResponseBase
{
    public required Guid PeerId { get; init; }
}