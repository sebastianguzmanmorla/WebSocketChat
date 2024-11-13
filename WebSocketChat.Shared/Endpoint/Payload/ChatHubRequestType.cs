namespace WebSocketChat.Shared.Endpoint.Payload;

public enum ChatHubRequestType
{
    Login,
    Logout,
    SetNickname,
    GetPeersConnected
}