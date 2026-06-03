using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SummerProject1.Infrastructure.Entities;

namespace SummerProject1.Infrastructure.Providers;

public interface IDummySourceRunner
{
    Task RunAsync(Guid checkId, string vin, string sourceId, CancellationToken ct);
}

public sealed class DummySourceRunner(AppDbContext db, ILogger<DummySourceRunner> logger) : IDummySourceRunner
{
    public async Task RunAsync(Guid checkId, string vin, string sourceId, CancellationToken ct)
    {
        // TTL cache: if we have results for same vin+source in last 30 days, reuse.
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30);
        var cachedCheck = await db.Checks
            .Where(c => c.Vin == vin && c.CreatedAt >= cutoff)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                Results = db.SourceResults.Where(r => r.CheckId == c.Id && r.SourceId == sourceId).ToList(),
                Events = db.MileageEvents.Where(e => e.CheckId == c.Id && e.SourceId == sourceId).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (cachedCheck is not null && cachedCheck.Results.Count > 0)
        {
            logger.LogInformation("Using cached results for VIN={Vin} source={Source}", vin, sourceId);

            // clone cached into this check
            foreach (var r in cachedCheck.Results)
            {
                db.SourceResults.Add(new SourceResult
                {
                    Id = Guid.NewGuid(),
                    CheckId = checkId,
                    SourceId = sourceId,
                    Provider = r.Provider,
                    Status = r.Status,
                    RawJson = r.RawJson,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            }

            foreach (var e in cachedCheck.Events)
            {
                db.MileageEvents.Add(new MileageEvent
                {
                    Id = Guid.NewGuid(),
                    CheckId = checkId,
                    SourceId = sourceId,
                    Provider = e.Provider,
                    EventDate = e.EventDate,
                    MileageKm = e.MileageKm,
                    Country = e.Country,
                    Confidence = e.Confidence,
                    EvidenceUrl = e.EvidenceUrl,
                    Notes = e.Notes,
                });
            }

            await db.SaveChangesAsync(ct);
            return;
        }

        // Deterministic pseudo data from VIN + source
        var seedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(vin + ":" + sourceId));
        var seed = BitConverter.ToInt32(seedBytes, 0);
        var rng = new Random(seed);

        var provider = "dummy-" + sourceId;

        // Some VINs return no data in a deterministic way
        var noData = (seedBytes[1] % 7) == 0;
        if (noData)
        {
            db.SourceResults.Add(new SourceResult
            {
                Id = Guid.NewGuid(),
                CheckId = checkId,
                SourceId = sourceId,
                Provider = provider,
                Status = "no_data",
                RawJson = JsonSerializer.Serialize(new { message = "No data found (dummy)" })
            });
            await db.SaveChangesAsync(ct);
            return;
        }

        // Create 1-4 mileage events
        var count = 1 + (rng.Next() % 4);
        var baseMileage = 10_000 + (rng.Next() % 190_000);
        var baseDate = DateTimeOffset.UtcNow.AddYears(-(1 + (rng.Next() % 6)));

        var confidence = sourceId == Sources.Inspections ? "high" :
            sourceId == Sources.Auctions ? "medium" : "low";

        string? country = sourceId switch
        {
            Sources.Auctions => "DE",
            Sources.Inspections => "FR",
            Sources.Listings => "PL",
            _ => null
        };

        for (var i = 0; i < count; i++)
        {
            var mileage = baseMileage + (i * (2000 + (rng.Next() % 15000)));
            var date = baseDate.AddMonths(i * (6 + (rng.Next() % 8)));

            db.MileageEvents.Add(new MileageEvent
            {
                Id = Guid.NewGuid(),
                CheckId = checkId,
                SourceId = sourceId,
                Provider = provider,
                EventDate = date,
                MileageKm = mileage,
                Country = country,
                Confidence = confidence,
                EvidenceUrl = null,
                Notes = $"Dummy {sourceId} event {i + 1}"
            });
        }

        db.SourceResults.Add(new SourceResult
        {
            Id = Guid.NewGuid(),
            CheckId = checkId,
            SourceId = sourceId,
            Provider = provider,
            Status = "success",
            RawJson = JsonSerializer.Serialize(new { count })
        });

        await db.SaveChangesAsync(ct);
    }
}
