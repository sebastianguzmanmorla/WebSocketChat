using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Base.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubMessage), nameof(ChatHubMessage))]
[JsonDerivedType(typeof(ChatHubRequest<ChatHubResponse>), nameof(ChatHubRequest<ChatHubResponse>))]
[JsonDerivedType(typeof(ChatHubResponse), nameof(ChatHubResponse))]
[JsonDerivedType(typeof(ChatHubGetPeersConnectedRequest), nameof(ChatHubGetPeersConnectedRequest))]
[JsonDerivedType(typeof(ChatHubGetPeersConnectedResponse), nameof(ChatHubGetPeersConnectedResponse))]
[JsonDerivedType(typeof(ChatHubHeartbeatMessage), nameof(ChatHubHeartbeatMessage))]
[JsonDerivedType(typeof(ChatHubLoginRequest), typeDiscriminator: nameof(ChatHubLoginRequest))]
[JsonDerivedType(typeof(ChatHubLogoutRequest), typeDiscriminator: nameof(ChatHubLogoutRequest))]
[JsonDerivedType(typeof(ChatHubSetNicknameRequest), typeDiscriminator: nameof(ChatHubSetNicknameRequest))]
[JsonDerivedType(typeof(ChatHubSuccessResponse), typeDiscriminator: nameof(ChatHubSuccessResponse))]
public class PayloadBase : IPayload
{
    protected const string TypeProperty = "$type";
    
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}