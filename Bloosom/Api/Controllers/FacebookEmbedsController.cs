using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin,admin,manager")]
public class FacebookEmbedsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FacebookEmbedsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.FacebookEmbeds.Where(f => !f.IsDeleted).OrderBy(f => f.SortOrder).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFacebookEmbedDto dto)
    {
        var f = new FacebookEmbed
        {
            Id = Guid.NewGuid(),
            Url = dto.Url,
            Type = dto.Type,
            Label = dto.Label,
            EventCategory = dto.EventCategory,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.FacebookEmbeds.Add(f);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = f.Id }, f);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateFacebookEmbedDto dto)
    {
        var f = await _db.FacebookEmbeds.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (f == null) return NotFound();
        f.Url = dto.Url;
        f.Type = dto.Type;
        f.Label = dto.Label;
        f.EventCategory = dto.EventCategory;
        f.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var f = await _db.FacebookEmbeds.FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        f.IsDeleted = !f.IsDeleted;
        f.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = f.Id, isDeleted = f.IsDeleted });
    }

    [HttpGet("/facebook-embeds/page-url")]
    [Authorize(Roles = "it_admin")]
    public async Task<IActionResult> GetPageUrl()
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "facebook_page_url");
        return Ok(new { pageUrl = setting?.Value });
    }

    [HttpPut("/facebook-embeds/page-url")]
    [Authorize(Roles = "it_admin")]
    public async Task<IActionResult> SetPageUrl([FromBody] PageUrlDto dto)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "facebook_page_url");
        if (setting == null)
        {
            setting = new SiteSetting { Key = "facebook_page_url", Value = dto.PageUrl, Type = "string" };
            _db.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = dto.PageUrl;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateFacebookEmbedDto(string Url, string Type, string Label, string? EventCategory);
public record PageUrlDto(string PageUrl);

