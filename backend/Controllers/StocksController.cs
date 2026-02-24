using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YahooFinanceApi;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    [HttpGet("quote/{symbol}")]
    public async Task<IActionResult> GetQuote(string symbol)
    {
        try
        {
            var securities = await Yahoo.Symbols(symbol).Fields(
                Field.Symbol, Field.RegularMarketPrice, Field.FiftyTwoWeekHigh,
                Field.FiftyTwoWeekLow, Field.LongName, Field.ShortName,
                Field.MarketCap, Field.Currency).QueryAsync();

            var quote = securities[symbol];
            return Ok(new
            {
                Symbol = quote[Field.Symbol],
                Price = quote[Field.RegularMarketPrice],
                High52Week = quote[Field.FiftyTwoWeekHigh],
                Low52Week = quote[Field.FiftyTwoWeekLow],
                CompanyName = quote[Field.LongName] ?? quote[Field.ShortName],
                MarketCap = quote[Field.MarketCap],
                Currency = quote[Field.Currency]
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error fetching quote: {ex.Message}");
        }
    }

    [HttpGet("historical/{symbol}")]
    public async Task<IActionResult> GetHistorical(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var history = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate, Period.Daily);
            var data = history.Select(c => new
            {
                Date = c.DateTime,
                Open = c.Open,
                High = c.High,
                Low = c.Low,
                Close = c.Close,
                Volume = c.Volume,
                AdjustedClose = c.AdjustedClose
            });
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error fetching historical data: {ex.Message}");
        }
    }
}
