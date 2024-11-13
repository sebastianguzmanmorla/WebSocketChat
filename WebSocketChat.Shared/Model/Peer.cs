namespace WebSocketChat.Shared.Model;

public class Peer
{
    public required Guid Id { get; set; }
    public required string Nickname { get; set; }
    public DateTimeOffset LastSeen { get; set; }
}