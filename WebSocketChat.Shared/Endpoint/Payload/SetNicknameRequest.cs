using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(SetNicknameRequest), nameof(SetNicknameRequest))]
public class SetNicknameRequest : ChatHubRequest<SuccessResponse>
{
    public string? Nickname { get; init; }
}
