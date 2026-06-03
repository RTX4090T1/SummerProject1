using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using SummerProject1.Infrastructure;
using SummerProject1.Infrastructure.Background;
using SummerProject1.Infrastructure.Providers;

namespace SummerProject1.Api.Controllers;

[ApiController]
[Route("api/stripe")]
public sealed class StripeWebhookController(
    AppDbContext db,
    IOptions<StripeOptions> stripeOptions,
    IBackgroundTaskQueue queue,
    IDummySourceRunner runner) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var stripe = stripeOptions.Value;
        if (string.IsNullOrWhiteSpace(stripe.WebhookSecret))
            return StatusCode(500, new { error = "Stripe webhook secret not configured" });

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);

        Event stripeEvent;
        try
        {
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, stripe.WebhookSecret);
        }
        catch
        {
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            var checkIdStr = session?.Metadata?.GetValueOrDefault("checkId");

            if (Guid.TryParse(checkIdStr, out var checkId))
            {
                var check = await db.Checks.Include(c => c.Sources).FirstOrDefaultAsync(c => c.Id == checkId, ct);
                if (check is not null && check.Status == "created")
                {
                    check.Status = "paid";
                    check.PaidAt = DateTimeOffset.UtcNow;
                    check.StripePaymentIntentId = session?.PaymentIntentId;
                    await db.SaveChangesAsync(ct);

                    // Enqueue execution
                    await queue.QueueBackgroundWorkItemAsync(async token =>
                    {
                        await ExecuteCheckAsync(checkId, token);
                    });
                }
            }
        }

        return Ok();
    }

    private async Task ExecuteCheckAsync(Guid checkId, CancellationToken ct)
    {
        // create a scope-like DbContext since this controller's db is scoped; simplest: new context is better,
        // but for MVP, reusing is ok in background as long as it stays alive. We'll re-query each time.
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
            catch
            {
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
