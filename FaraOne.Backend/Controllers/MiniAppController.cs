using FaraOne.Application.Context;
using FaraOne.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaraOneBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MiniAppController : ControllerBase
{
    private readonly IDatabaseContext _dbContext;

    public MiniAppController(IDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetMiniApps([FromQuery] int? tenantId = null)
    {
        var query = _dbContext.MiniApps.AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(m => m.TenantId == tenantId || m.TenantId == null);
        }
        else
        {
            query = query.Where(m => m.TenantId == null);
        }

        var apps = await query.Where(m => m.IsActive).ToListAsync();
        return Ok(apps);
    }

    [HttpGet("{appId}")]
    public async Task<IActionResult> GetMiniApp(string appId)
    {
        var app = await _dbContext.MiniApps.FirstOrDefaultAsync(m => m.AppId == appId);

        if (app == null)
            return NotFound();

        return Ok(app);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterMiniApp([FromBody] MiniAppRegistrationRequest request)
    {
        var miniApp = new MiniApp
        {
            AppId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            Url = request.Url,
            Version = request.Version,
            Permissions = request.Permissions,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.MiniApps.AddAsync(miniApp);
        await _dbContext.SaveChangesAsync();

        return Ok(miniApp);
    }
}

public class MiniAppRegistrationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Permissions { get; set; } 
    public int? TenantId { get; set; }
}