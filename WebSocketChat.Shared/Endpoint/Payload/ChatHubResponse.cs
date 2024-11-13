using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Payload;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubResponse), nameof(ChatHubResponse))]
public class ChatHubResponse : ResponseBase
{
    public ChatHubResponseType ResponseType { get; set; }
    
    public IEnumerable<Peer>? Peers { get; set; }
}