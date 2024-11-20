using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;
using WebSocketChat.Shared.Model;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(PeerDisconnectedMessage), nameof(PeerDisconnectedMessage))]
public class PeerDisconnectedMessage : ChatHubMessage
{
    public required Peer Peer { get; init; }
}
