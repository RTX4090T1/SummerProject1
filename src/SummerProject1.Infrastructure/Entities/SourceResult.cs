namespace SummerProject1.Infrastructure.Entities;

public sealed class SourceResult
{
    public Guid Id { get; set; }
    public Guid CheckId { get; set; }
    public string SourceId { get; set; } = "";

    public string Provider { get; set; } = "dummy";

    /// <summary>success | no_data | error</summary>
    public string Status { get; set; } = "success";

    public string? RawJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
