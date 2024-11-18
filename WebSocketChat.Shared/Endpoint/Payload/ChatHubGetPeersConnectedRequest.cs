using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubGetPeersConnectedRequest), nameof(ChatHubGetPeersConnectedRequest))]
public class ChatHubGetPeersConnectedRequest : ChatHubRequest<ChatHubGetPeersConnectedResponse>
{
}
