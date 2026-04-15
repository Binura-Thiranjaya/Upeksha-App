using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Bloosom.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin,admin,manager")]
public class EventCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _storage;

    public EventCategoriesController(AppDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.EventCategories.Include(e => e.Images).Where(e => !e.IsDeleted).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCategoryDto dto)
    {
        var ec = new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Name.ToLower().Replace(' ', '-'),
            Description = dto.Description,
            Icon = dto.Icon,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.EventCategories.Add(ec);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = ec.Id }, ec);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventCategoryDto dto)
    {
        var ec = await _db.EventCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (ec == null) return NotFound();
        ec.Name = dto.Name;
        ec.Slug = dto.Name.ToLower().Replace(' ', '-');
        ec.Description = dto.Description;
        ec.Icon = dto.Icon;
        ec.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ec = await _db.EventCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (ec == null) return NotFound();
        ec.IsDeleted = !ec.IsDeleted;
        ec.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = ec.Id, isDeleted = ec.IsDeleted });
    }

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, [FromForm] int sortOrder = 0, [FromForm] string? caption = null)
    {
        var ec = await _db.EventCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (ec == null) return NotFound();
        if (file == null || file.Length == 0) return BadRequest(new { error = "File required" });
        var url = await _storage.SaveFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
        var img = new EventImage
        {
            Id = Guid.NewGuid(),
            EventCategoryId = ec.Id,
            ImageUrl = url,
            Caption = caption,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.EventImages.Add(img);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = img.Id }, img);
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var img = await _db.EventImages.FirstOrDefaultAsync(x => x.Id == imageId && x.EventCategoryId == id);
        if (img == null) return NotFound();
        img.IsDeleted = !img.IsDeleted;
        img.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = img.Id, isDeleted = img.IsDeleted });
    }
}

public record CreateEventCategoryDto(string Name, string? Description, string? Icon);
public record UpdateEventCategoryDto(string Name, string? Description, string? Icon);

