using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PortfoliosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolios()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        return await _context.Portfolios
            .Include(p => p.Holdings)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Portfolio>> GetPortfolio(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolio = await _context.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (portfolio == null)
        {
            return NotFound();
        }
        return portfolio;
    }

    [HttpPost]
    public async Task<ActionResult<Portfolio>> CreatePortfolio(Portfolio portfolio)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        portfolio.UserId = userId;
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePortfolio(int id, Portfolio portfolio)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (id != portfolio.Id)
        {
            return BadRequest();
        }

        if (!await PortfolioExists(id, userId))
        {
            return NotFound();
        }

        portfolio.UserId = userId;
        _context.Entry(portfolio).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PortfolioExists(id, userId))
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
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (portfolio == null)
        {
            return NotFound();
        }
        _context.Portfolios.Remove(portfolio);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> PortfolioExists(int id, string userId)
    {
        return _context.Portfolios.AnyAsync(e => e.Id == id && e.UserId == userId);
    }
}
