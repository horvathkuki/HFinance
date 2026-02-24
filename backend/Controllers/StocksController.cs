using backend.Contracts;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/market")]
public class StocksController : ControllerBase
{
    private readonly IMarketDataService _marketDataService;

    public StocksController(IMarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
    }

    [HttpGet("quote/{symbol}")]
    public async Task<IActionResult> GetQuote(string symbol, CancellationToken cancellationToken)
    {
        var quote = await _marketDataService.GetQuoteAsync(symbol, cancellationToken);
        if (quote is null)
        {
            return NotFound(new ApiErrorResponse("quote_not_found", "Quote could not be fetched for this symbol.", HttpContext.TraceIdentifier));
        }

        return Ok(quote);
    }

    [HttpGet("historical/{symbol}")]
    public async Task<IActionResult> GetHistorical(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        if (endDate < startDate)
        {
            return BadRequest(new ApiErrorResponse("invalid_date_range", "endDate must be after startDate.", HttpContext.TraceIdentifier));
        }

        if ((endDate - startDate).TotalDays > 3650)
        {
            return BadRequest(new ApiErrorResponse("date_range_too_large", "Requested range is too large.", HttpContext.TraceIdentifier));
        }

        var data = await _marketDataService.GetHistoricalAsync(symbol, startDate, endDate, cancellationToken);
        return Ok(data);
    }
}
