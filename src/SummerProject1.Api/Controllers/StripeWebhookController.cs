using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using SummerProject1.Infrastructure;
using SummerProject1.Infrastructure.Background;

namespace SummerProject1.Api.Controllers;

[ApiController]
[Route("api/stripe")]
public sealed class StripeWebhookController(
    AppDbContext db,
    IOptions<StripeOptions> stripeOptions,
    IBackgroundTaskQueue queue,
    ICheckExecutionService executor) : ControllerBase
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

                    // Enqueue execution (scope-safe)
                    await queue.QueueBackgroundWorkItemAsync(async token =>
                    {
                        await executor.ExecuteAsync(checkId, token);
                    });
                }
            }
        }

        return Ok();
    }
}
