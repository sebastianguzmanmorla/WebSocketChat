using System.Text.Json.Serialization;

namespace WebSocketChat.Shared.Endpoint.Payload;

using WebSocketChat.Shared.Endpoint.Base.Payload;

[JsonPolymorphic(TypeDiscriminatorPropertyName = TypeProperty)]
[JsonDerivedType(typeof(ChatHubRequest), nameof(ChatHubRequest))]
public class ChatHubRequest : RequestBase
{
    public new const string Route = "/";
    public override string RequestRoute() => Route;

    public required Guid PeerId { get; set; }
    
    public string? Nickname { get; set; }
    
    public required ChatHubRequestType RequestType { get; set; }
}