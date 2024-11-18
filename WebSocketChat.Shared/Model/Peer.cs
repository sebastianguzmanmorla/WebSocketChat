namespace WebSocketChat.Shared.Model;

public class Peer
{
    public required Guid Id { get; init; }
    public required string Nickname { get; init; }
    public DateTimeOffset LastSeen { get; init; }
}