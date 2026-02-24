using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/portfolios/{portfolioId}/[controller]")]
public class HoldingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HoldingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Holding>>> GetHoldings(int portfolioId)
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
        return await _context.Holdings.Where(h => h.PortfolioId == portfolioId).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Holding>> GetHolding(int portfolioId, int id)
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
        return holding;
    }

    [HttpPost]
    public async Task<ActionResult<Holding>> CreateHolding(int portfolioId, Holding holding)
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
        holding.PortfolioId = portfolioId;
        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetHolding), new { portfolioId, id = holding.Id }, holding);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHolding(int portfolioId, int id, Holding holding)
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

        if (id != holding.Id || portfolioId != holding.PortfolioId)
        {
            return BadRequest();
        }
        _context.Entry(holding).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HoldingExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
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

    private bool HoldingExists(int id)
    {
        return _context.Holdings.Any(e => e.Id == id);
    }
}
