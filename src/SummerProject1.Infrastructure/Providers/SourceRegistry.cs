namespace SummerProject1.Infrastructure.Providers;

public sealed record SourceInfo(
    string Id,
    string DisplayName,
    long PriceCents,
    string Currency,
    string Description);

public interface ISourceRegistry
{
    IReadOnlyList<SourceInfo> GetSources();
    long GetAllBundlePriceCents();
    string GetCurrency();
    bool IsValidSource(string sourceId);
}

public sealed class SourceRegistry : ISourceRegistry
{
    // MVP: hardcoded pricing; later move to DB/config.
    private const string Currency = "EUR";

    private static readonly List<SourceInfo> SourcesList =
    [
        new(Sources.Auctions, "Auctions", 199, Currency, "Auction listings & sale signals (dummy)."),
        new(Sources.Inspections, "Inspections", 299, Currency, "Mileage timeline from inspections (dummy)."),
        new(Sources.Listings, "Listings", 99, Currency, "Marketplace listings (dummy)."),
    ];

    public IReadOnlyList<SourceInfo> GetSources() => SourcesList;

    public long GetAllBundlePriceCents() => 449; // discounted bundle

    public string GetCurrency() => Currency;

    public bool IsValidSource(string sourceId) => SourcesList.Any(s => s.Id == sourceId);
}
