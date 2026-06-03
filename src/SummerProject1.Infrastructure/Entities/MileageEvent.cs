namespace SummerProject1.Infrastructure.Entities;

public sealed class MileageEvent
{
    public Guid Id { get; set; }
    public Guid CheckId { get; set; }

    public string SourceId { get; set; } = "";
    public string Provider { get; set; } = "";

    public DateTimeOffset EventDate { get; set; }
    public int? MileageKm { get; set; }
    public string? Country { get; set; }

    /// <summary>high | medium | low</summary>
    public string Confidence { get; set; } = "medium";

    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
}
