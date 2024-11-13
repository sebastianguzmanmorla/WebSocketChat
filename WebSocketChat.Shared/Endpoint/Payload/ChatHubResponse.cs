using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubResponse), nameof(ChatHubResponse))]
public class ChatHubResponse : ResponseBase
{
    public ChatHubResponseType ResponseType { get; set; }
}