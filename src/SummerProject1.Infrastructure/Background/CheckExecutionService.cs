using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SummerProject1.Infrastructure.Background;

public interface ICheckExecutionService
{
    Task ExecuteAsync(Guid checkId, CancellationToken ct);
}

public sealed class CheckExecutionService(
    IServiceScopeFactory scopeFactory,
    ILogger<CheckExecutionService> logger) : ICheckExecutionService
{
    public async Task ExecuteAsync(Guid checkId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var runner = scope.ServiceProvider.GetRequiredService<Providers.IDummySourceRunner>();

        var check = await db.Checks.Include(c => c.Sources).FirstOrDefaultAsync(c => c.Id == checkId, ct);
        if (check is null) return;

        check.Status = "running";
        await db.SaveChangesAsync(ct);

        foreach (var s in check.Sources)
        {
            s.Status = "running";
            s.StartedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);

        foreach (var s in check.Sources)
        {
            try
            {
                await runner.RunAsync(check.Id, check.Vin, s.SourceId, ct);
                s.Status = "success";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Provider execution failed for checkId={CheckId} source={SourceId}", checkId, s.SourceId);
                s.Status = "error";
            }
            finally
            {
                s.FinishedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }
        }

        check.Status = "done";
        check.FinishedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
