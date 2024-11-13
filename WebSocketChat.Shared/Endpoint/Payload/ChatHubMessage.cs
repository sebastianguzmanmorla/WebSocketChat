using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubMessage), nameof(ChatHubMessage))]
public class ChatHubMessage : MessageBase
{
    public required Guid PeerId { get; set; }
    
    public required ChatHubMessageType MessageType { get; set; }
}