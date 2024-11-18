using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubSetNicknameRequest), nameof(ChatHubSetNicknameRequest))]
public class ChatHubSetNicknameRequest : ChatHubRequest<ChatHubSuccessResponse>
{
    public string? Nickname { get; init; }
}
