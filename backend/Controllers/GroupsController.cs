using backend.Contracts;
using backend.Contracts.Groups;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/groups")]
public class GroupsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHoldingGroupService _holdingGroupService;

    public GroupsController(ApplicationDbContext dbContext, IHoldingGroupService holdingGroupService)
    {
        _dbContext = dbContext;
        _holdingGroupService = holdingGroupService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _holdingGroupService.EnsureUncategorizedGroupAsync(userId);

        var groups = await _dbContext.HoldingGroups
            .AsNoTracking()
            .Where(group => group.UserId == userId)
            .OrderBy(group => group.Name)
            .Select(group => new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAtUtc = group.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var normalizedName = request.Name.Trim();
        var exists = await _dbContext.HoldingGroups.AnyAsync(group =>
            group.UserId == userId &&
            group.Name.ToLower() == normalizedName.ToLower());

        if (exists)
        {
            return Conflict(new ApiErrorResponse("group_exists", "A group with this name already exists.", HttpContext.TraceIdentifier));
        }

        var group = new Models.HoldingGroup
        {
            UserId = userId,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.HoldingGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGroups), new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CreatedAtUtc = group.CreatedAtUtc
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(int id, UpdateGroupRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var group = await _dbContext.HoldingGroups.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
        if (group is null)
        {
            return NotFound();
        }

        if (group.Name == HoldingGroupService.UncategorizedName)
        {
            return BadRequest(new ApiErrorResponse("group_locked", "The default group cannot be renamed.", HttpContext.TraceIdentifier));
        }

        group.Name = request.Name.Trim();
        group.Description = request.Description?.Trim();
        group.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var group = await _dbContext.HoldingGroups.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
        if (group is null)
        {
            return NotFound();
        }

        var uncategorized = await _holdingGroupService.EnsureUncategorizedGroupAsync(userId);
        if (group.Id == uncategorized.Id)
        {
            return BadRequest(new ApiErrorResponse("group_locked", "The default group cannot be deleted.", HttpContext.TraceIdentifier));
        }

        var holdingsToMove = await _dbContext.Holdings.Where(holding => holding.GroupId == id).ToListAsync();
        foreach (var holding in holdingsToMove)
        {
            holding.GroupId = uncategorized.Id;
        }

        _dbContext.HoldingGroups.Remove(group);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
