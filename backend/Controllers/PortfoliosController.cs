using backend.Data;
using backend.Contracts.Portfolio;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios")]
public class PortfoliosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PortfoliosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetPortfolios()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolios = await _context.Portfolios
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .Select(portfolio => new PortfolioDto
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Description = portfolio.Description,
                CreatedDate = portfolio.CreatedDate
            })
            .ToListAsync();

        return Ok(portfolios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDto>> GetPortfolio(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolio = await _context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (portfolio == null)
        {
            return NotFound();
        }

        return Ok(new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            CreatedDate = portfolio.CreatedDate
        });
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioDto>> CreatePortfolio(CreatePortfolioRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolio = new Portfolio
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedDate = DateTime.UtcNow
        };
        portfolio.UserId = userId;
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var dto = new PortfolioDto
        {
            Id = portfolio.Id,
            Name = portfolio.Name,
            Description = portfolio.Description,
            CreatedDate = portfolio.CreatedDate
        };
        return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePortfolio(int id, UpdatePortfolioRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var portfolio = await _context.Portfolios.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
        if (portfolio is null)
        {
            return NotFound();
        }

        portfolio.Name = request.Name.Trim();
        portfolio.Description = request.Description?.Trim();
        await _context.SaveChangesAsync();
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

}
