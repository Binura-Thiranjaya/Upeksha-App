using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _db.SiteSettings.ToListAsync();
        return Ok(all);
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> Bulk([FromBody] BulkUpdateSettingsDto dto)
    {
        foreach (var s in dto.Settings)
        {
            var existing = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == s.Key);
            if (existing == null)
            {
                _db.SiteSettings.Add(new SiteSetting { Key = s.Key, Value = s.Value, Type = "string" });
            }
            else
            {
                existing.Value = s.Value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> PutKey(string key, [FromBody] UpdateSettingDto dto)
    {
        var existing = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
        if (existing == null) return NotFound();
        existing.Value = dto.Value;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record BulkUpdateSettingsDto(IEnumerable<SettingPair> Settings);
public record SettingPair(string Key, string Value);
public record UpdateSettingDto(string Value);

