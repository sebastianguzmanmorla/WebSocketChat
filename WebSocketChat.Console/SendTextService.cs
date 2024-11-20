using Microsoft.Extensions.Hosting;
using WebSocketChat.Shared.Endpoint;
using WebSocketChat.Shared.Endpoint.Payload;

namespace WebSocketChat.Console;

public class SendTextService
(
    Settings settings,
    ChatHubEndpoint chatHubEndpoint
) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken CancelToken)
    {
        do
        {
            if (chatHubEndpoint.State == System.Net.WebSockets.WebSocketState.Open)
            {
                string? text = System.Console.ReadLine();
                if (!string.IsNullOrEmpty(text))
                {
                    await chatHubEndpoint.SendMessage(new SendTextMessage
                    {
                        PeerId = settings.PeerId.GetValueOrDefault(),
                        Text = text
                    });
                }
            }

            await Task.Delay(1000, CancelToken).ContinueWith(x => x.Exception == default);
        } while (!CancelToken.IsCancellationRequested);
    }
}
