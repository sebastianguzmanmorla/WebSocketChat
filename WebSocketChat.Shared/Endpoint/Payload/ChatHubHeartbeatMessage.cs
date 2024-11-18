using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubHeartbeatMessage), nameof(ChatHubHeartbeatMessage))]
public class ChatHubHeartbeatMessage : ChatHubMessage
{
}
