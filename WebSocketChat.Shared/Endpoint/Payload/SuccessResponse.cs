using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(SuccessResponse), typeDiscriminator: nameof(SuccessResponse))]
public class SuccessResponse : ResponseBase
{
    public bool Success { get; init; }
}
