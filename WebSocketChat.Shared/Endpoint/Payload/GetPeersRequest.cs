using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(GetPeersRequest), nameof(GetPeersRequest))]
public class GetPeersRequest : ChatHubRequest<GetPeersResponse>
{
}
