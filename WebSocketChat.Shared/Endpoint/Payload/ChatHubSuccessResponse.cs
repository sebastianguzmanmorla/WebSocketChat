using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubSuccessResponse), typeDiscriminator: nameof(ChatHubSuccessResponse))]
public class ChatHubSuccessResponse : ResponseBase
{
    public bool Success { get; init; }
}
