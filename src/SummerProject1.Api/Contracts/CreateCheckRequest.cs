using System.ComponentModel.DataAnnotations;

namespace SummerProject1.Api.Contracts;

public sealed class CreateCheckRequest
{
    [Required]
    public string Vin { get; set; } = "";

    [Required]
    [RegularExpression("^(single|all)$")]
    public string Mode { get; set; } = "single";

    public string? SourceId { get; set; }
}
