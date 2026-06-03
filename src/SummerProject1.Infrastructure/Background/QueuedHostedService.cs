using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SummerProject1.Infrastructure.Background;

public sealed class QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("QueuedHostedService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing background work item.");
            }
        }

        logger.LogInformation("QueuedHostedService is stopping.");
    }
}
