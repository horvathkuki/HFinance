using backend.Data;
using backend.Contracts;
using backend.Contracts.Holdings;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios/{portfolioId}/holdings")]
public class HoldingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HoldingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HoldingDto>>> GetHoldings(int portfolioId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!await PortfolioExists(portfolioId, userId))
        {
            return NotFound();
        }

        var holdings = await _context.Holdings
            .AsNoTracking()
            .Include(h => h.Group)
            .Where(h => h.PortfolioId == portfolioId)
            .Select(h => MapHolding(h))
            .ToListAsync();

        return Ok(holdings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HoldingDto>> GetHolding(int portfolioId, int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!await PortfolioExists(portfolioId, userId))
        {
            return NotFound();
        }

        var holding = await _context.Holdings
            .AsNoTracking()
            .Include(h => h.Group)
            .FirstOrDefaultAsync(h => h.Id == id && h.PortfolioId == portfolioId);
        if (holding == null)
        {
            return NotFound();
        }
        return Ok(MapHolding(holding));
    }

    [HttpPost]
    public async Task<ActionResult<HoldingDto>> CreateHolding(int portfolioId, CreateHoldingRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!await PortfolioExists(portfolioId, userId))
        {
            return NotFound();
        }

        var normalizedCurrency = CurrencyHelper.NormalizeCurrency(request.Currency);
        if (!CurrencyHelper.AllowedCurrencies.Contains(normalizedCurrency))
        {
            return BadRequest(new ApiErrorResponse("invalid_currency", "Currency must be EUR, USD, or RON.", HttpContext.TraceIdentifier));
        }

        var groupExists = await _context.HoldingGroups.AnyAsync(group => group.Id == request.GroupId && group.UserId == userId);
        if (!groupExists)
        {
            return BadRequest(new ApiErrorResponse("invalid_group", "Group does not exist.", HttpContext.TraceIdentifier));
        }

        var holding = new Holding
        {
            PortfolioId = portfolioId,
            GroupId = request.GroupId,
            Symbol = request.Symbol.Trim().ToUpperInvariant(),
            Quantity = request.Quantity,
            AveragePurchasePrice = request.AveragePurchasePrice,
            Currency = normalizedCurrency,
            PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow
        };

        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();

        var createdHolding = await _context.Holdings
            .AsNoTracking()
            .Include(h => h.Group)
            .FirstAsync(h => h.Id == holding.Id);

        return CreatedAtAction(nameof(GetHolding), new { portfolioId, id = holding.Id }, MapHolding(createdHolding));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHolding(int portfolioId, int id, UpdateHoldingRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!await PortfolioExists(portfolioId, userId))
        {
            return NotFound();
        }

        var holding = await _context.Holdings.FirstOrDefaultAsync(h => h.Id == id && h.PortfolioId == portfolioId);
        if (holding is null)
        {
            return NotFound();
        }

        var normalizedCurrency = CurrencyHelper.NormalizeCurrency(request.Currency);
        if (!CurrencyHelper.AllowedCurrencies.Contains(normalizedCurrency))
        {
            return BadRequest(new ApiErrorResponse("invalid_currency", "Currency must be EUR, USD, or RON.", HttpContext.TraceIdentifier));
        }

        var groupExists = await _context.HoldingGroups.AnyAsync(group => group.Id == request.GroupId && group.UserId == userId);
        if (!groupExists)
        {
            return BadRequest(new ApiErrorResponse("invalid_group", "Group does not exist.", HttpContext.TraceIdentifier));
        }

        holding.Symbol = request.Symbol.Trim().ToUpperInvariant();
        holding.Quantity = request.Quantity;
        holding.AveragePurchasePrice = request.AveragePurchasePrice;
        holding.Currency = normalizedCurrency;
        holding.GroupId = request.GroupId;
        holding.PurchaseDate = request.PurchaseDate ?? holding.PurchaseDate;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHolding(int portfolioId, int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!await PortfolioExists(portfolioId, userId))
        {
            return NotFound();
        }

        var holding = await _context.Holdings.FirstOrDefaultAsync(h => h.Id == id && h.PortfolioId == portfolioId);
        if (holding == null)
        {
            return NotFound();
        }
        _context.Holdings.Remove(holding);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> PortfolioExists(int id, string userId)
    {
        return _context.Portfolios.AnyAsync(e => e.Id == id && e.UserId == userId);
    }

    private static HoldingDto MapHolding(Holding holding)
    {
        return new HoldingDto
        {
            Id = holding.Id,
            PortfolioId = holding.PortfolioId,
            Symbol = holding.Symbol,
            Quantity = holding.Quantity,
            AveragePurchasePrice = holding.AveragePurchasePrice,
            Currency = holding.Currency,
            PurchaseDate = holding.PurchaseDate,
            GroupId = holding.GroupId,
            GroupName = holding.Group?.Name ?? HoldingGroupService.UncategorizedName
        };
    }
}
