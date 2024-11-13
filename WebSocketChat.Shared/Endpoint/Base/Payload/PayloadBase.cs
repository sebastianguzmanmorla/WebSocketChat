using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload;

namespace WebSocketChat.Shared.Endpoint.Base.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubMessage), typeDiscriminator: nameof(ChatHubMessage))]
[JsonDerivedType(typeof(ChatHubRequest), typeDiscriminator: nameof(ChatHubRequest))]
[JsonDerivedType(typeof(ChatHubResponse), typeDiscriminator: nameof(ChatHubResponse))]
public class PayloadBase
{
    protected const string TypeProperty = "$type";
    
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}