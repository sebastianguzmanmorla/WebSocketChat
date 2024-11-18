using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Payload.Base;

public class ChatHubMessage : MessageBase
{
    public required Guid PeerId { get; init; }
}