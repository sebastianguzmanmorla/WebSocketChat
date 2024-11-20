using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(GetPeersResponse), typeDiscriminator: nameof(GetPeersResponse))]
public class GetPeersResponse : ChatHubResponse
{
    public required IEnumerable<Peer> Peers { get; init; }
}
