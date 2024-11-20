using System.Text.Json.Serialization;
using WebSocketChat.Shared.Endpoint.Base.Interfaces;
using WebSocketChat.Shared.Endpoint.Payload;
using WebSocketChat.Shared.Endpoint.Payload.Base;

namespace WebSocketChat.Shared.Endpoint.Base.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubMessage), nameof(ChatHubMessage))]
[JsonDerivedType(typeof(ChatHubRequest<ChatHubResponse>), nameof(ChatHubRequest<ChatHubResponse>))]
[JsonDerivedType(typeof(ChatHubResponse), nameof(ChatHubResponse))]
[JsonDerivedType(typeof(GetPeersRequest), nameof(GetPeersRequest))]
[JsonDerivedType(typeof(GetPeersResponse), nameof(GetPeersResponse))]
[JsonDerivedType(typeof(HeartbeatMessage), nameof(HeartbeatMessage))]
[JsonDerivedType(typeof(LoginRequest), nameof(LoginRequest))]
[JsonDerivedType(typeof(LogoutRequest), nameof(LogoutRequest))]
[JsonDerivedType(typeof(PeerConnectedMessage), nameof(PeerConnectedMessage))]
[JsonDerivedType(typeof(PeerDisconnectedMessage), nameof(PeerDisconnectedMessage))]
[JsonDerivedType(typeof(SendTextMessage), nameof(SendTextMessage))]
[JsonDerivedType(typeof(SetNicknameRequest), nameof(SetNicknameRequest))]
[JsonDerivedType(typeof(SuccessResponse), nameof(SuccessResponse))]
public class PayloadBase : IPayload
{
    protected const string TypeProperty = "$type";

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}