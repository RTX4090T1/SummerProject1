namespace SummerProject1.Api.Contracts;

public sealed class CreateCheckResponse
{
    public Guid CheckId { get; set; }
    public long AmountCents { get; set; }
    public string Currency { get; set; } = "EUR";

    public string StripeCheckoutUrl { get; set; } = "";
    public string StripeSessionId { get; set; } = "";
}
