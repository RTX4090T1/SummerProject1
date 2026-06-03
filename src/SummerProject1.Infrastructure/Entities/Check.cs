namespace SummerProject1.Infrastructure.Entities;

public sealed class Check
{
    public Guid Id { get; set; }
    public string Vin { get; set; } = "";

    /// <summary>"single" or "all"</summary>
    public string Mode { get; set; } = "";
    public string? SingleSourceId { get; set; }

    public long AmountCents { get; set; }
    public string Currency { get; set; } = "EUR";

    /// <summary>created | paid | running | done | failed</summary>
    public string Status { get; set; } = "created";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public string? StripeCheckoutSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public List<CheckSource> Sources { get; set; } = new();
}
