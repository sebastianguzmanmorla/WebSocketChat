using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(LoginRequest), nameof(LoginRequest))]
public class LoginRequest : ChatHubRequest<SuccessResponse>
{
    public string? Nickname { get; init; }
}
