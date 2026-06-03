using Microsoft.AspNetCore.Mvc;
using SummerProject1.Infrastructure.Providers;

namespace SummerProject1.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public sealed class PricingController(ISourceRegistry sources) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            currency = sources.GetCurrency(),
            sources = sources.GetSources().Select(s => new
            {
                id = s.Id,
                name = s.DisplayName,
                priceCents = s.PriceCents,
                currency = s.Currency,
                description = s.Description
            }),
            all = new
            {
                priceCents = sources.GetAllBundlePriceCents(),
                currency = sources.GetCurrency(),
                sourceIds = Sources.All
            }
        });
    }
}
