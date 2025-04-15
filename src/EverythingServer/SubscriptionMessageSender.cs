using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace EverythingServer;

internal class SubscriptionMessageSender(IMcpServer server, HashSet<string> subscription) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var uri in subscription)
            {
                await server.SendNotificationAsync("notifications/resource/updated", new
                {
                    Uri = uri,
                }, cancellationToken: stoppingToken);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
