namespace SummerProject1.Infrastructure.Entities;

public sealed class CheckSource
{
    public Guid Id { get; set; }
    public Guid CheckId { get; set; }
    public string SourceId { get; set; } = "";

    /// <summary>pending | running | success | no_data | error | skipped</summary>
    public string Status { get; set; } = "pending";

    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
}
