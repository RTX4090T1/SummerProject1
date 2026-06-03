using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using SummerProject1.Api.Contracts;
using SummerProject1.Infrastructure;
using SummerProject1.Infrastructure.Entities;
using SummerProject1.Infrastructure.Providers;

namespace SummerProject1.Api.Controllers;

[ApiController]
[Route("api/checks")]
public sealed class ChecksController(
    AppDbContext db,
    ISourceRegistry sourceRegistry,
    IOptions<StripeOptions> stripeOptions) : ControllerBase
{
    private static readonly Regex VinRegex = new("^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.Compiled);

    [HttpPost]
    public async Task<ActionResult<CreateCheckResponse>> Create([FromBody] CreateCheckRequest req, CancellationToken ct)
    {
        var vin = (req.Vin ?? "").Trim().ToUpperInvariant();
        if (!VinRegex.IsMatch(vin))
            return BadRequest(new { error = "VIN must be 17 chars, uppercase letters/digits (I,O,Q not allowed)." });

        var mode = req.Mode;
        if (mode != "single" && mode != "all")
            return BadRequest(new { error = "mode must be 'single' or 'all'" });

        string? singleSourceId = null;
        long amountCents;

        if (mode == "single")
        {
            if (string.IsNullOrWhiteSpace(req.SourceId))
                return BadRequest(new { error = "sourceId is required for mode=single" });

            singleSourceId = req.SourceId.Trim().ToLowerInvariant();
            if (!sourceRegistry.IsValidSource(singleSourceId))
                return BadRequest(new { error = "Invalid sourceId" });

            amountCents = sourceRegistry.GetSources().First(s => s.Id == singleSourceId).PriceCents;
        }
        else
        {
            amountCents = sourceRegistry.GetAllBundlePriceCents();
        }

        var currency = sourceRegistry.GetCurrency();

        var check = new Check
        {
            Id = Guid.NewGuid(),
            Vin = vin,
            Mode = mode,
            SingleSourceId = singleSourceId,
            AmountCents = amountCents,
            Currency = currency,
            Status = "created",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // create check sources
        var sourcesToRun = mode == "all" ? Sources.All : new[] { singleSourceId! };
        foreach (var s in sourcesToRun)
        {
            check.Sources.Add(new CheckSource
            {
                Id = Guid.NewGuid(),
                CheckId = check.Id,
                SourceId = s,
                Status = "pending"
            });
        }

        db.Checks.Add(check);
        await db.SaveChangesAsync(ct);

        // Stripe Checkout
        var stripe = stripeOptions.Value;
        if (string.IsNullOrWhiteSpace(stripe.SecretKey))
            return StatusCode(500, new { error = "Stripe secret key is not configured" });

        StripeConfiguration.ApiKey = stripe.SecretKey;

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = stripe.SuccessUrl + $"?checkId={check.Id}",
            CancelUrl = stripe.CancelUrl + $"?checkId={check.Id}",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency.ToLowerInvariant(),
                        UnitAmount = amountCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = mode == "all" ? "VIN Check (All Sources)" : $"VIN Check ({singleSourceId})",
                            Description = $"VIN: {vin}"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["checkId"] = check.Id.ToString()
            }
        }, ct);

        check.StripeCheckoutSessionId = session.Id;
        await db.SaveChangesAsync(ct);

        return Ok(new CreateCheckResponse
        {
            CheckId = check.Id,
            AmountCents = amountCents,
            Currency = currency,
            StripeCheckoutUrl = session.Url,
            StripeSessionId = session.Id
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var check = await db.Checks.Include(c => c.Sources).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (check is null) return NotFound();

        return Ok(new
        {
            id = check.Id,
            vin = check.Vin,
            mode = check.Mode,
            singleSourceId = check.SingleSourceId,
            amountCents = check.AmountCents,
            currency = check.Currency,
            status = check.Status,
            createdAt = check.CreatedAt,
            paidAt = check.PaidAt,
            finishedAt = check.FinishedAt,
            sources = check.Sources.Select(s => new
            {
                id = s.SourceId,
                status = s.Status,
                startedAt = s.StartedAt,
                finishedAt = s.FinishedAt
            })
        });
    }

    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> Report(Guid id, CancellationToken ct)
    {
        var check = await db.Checks.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (check is null) return NotFound();

        var results = await db.SourceResults.Where(r => r.CheckId == id).ToListAsync(ct);
        var events = await db.MileageEvents.Where(e => e.CheckId == id).OrderBy(e => e.EventDate).ToListAsync(ct);

        return Ok(new
        {
            checkId = id,
            vin = check.Vin,
            status = check.Status,
            results = results.Select(r => new
            {
                sourceId = r.SourceId,
                provider = r.Provider,
                status = r.Status,
                createdAt = r.CreatedAt
            }),
            timeline = events.Select(e => new
            {
                date = e.EventDate,
                mileageKm = e.MileageKm,
                country = e.Country,
                sourceId = e.SourceId,
                provider = e.Provider,
                confidence = e.Confidence,
                evidenceUrl = e.EvidenceUrl,
                notes = e.Notes
            })
        });
    }
}
