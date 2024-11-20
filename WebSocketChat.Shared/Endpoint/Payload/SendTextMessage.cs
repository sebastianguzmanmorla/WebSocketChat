using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(SendTextMessage), nameof(SendTextMessage))]
public class SendTextMessage : ChatHubMessage
{
    public required string Text { get; init; }
}
