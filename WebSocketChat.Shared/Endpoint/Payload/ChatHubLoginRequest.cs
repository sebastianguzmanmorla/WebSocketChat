using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubLoginRequest), nameof(ChatHubLoginRequest))]
public class ChatHubLoginRequest : ChatHubRequest<ChatHubSuccessResponse>
{
    public string? Nickname { get; init; }
}
