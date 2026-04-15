using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin,admin,manager")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.SortOrder).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var cat = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Name.ToLower().Replace(' ', '-'),
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cat.Id }, cat);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (cat == null) return NotFound();
        cat.Name = dto.Name;
        cat.Slug = dto.Name.ToLower().Replace(' ', '-');
        cat.SortOrder = dto.SortOrder;
        cat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return NotFound();
        cat.IsDeleted = !cat.IsDeleted;
        cat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = cat.Id, isDeleted = cat.IsDeleted });
    }
}

public record CreateCategoryDto(string Name, int SortOrder);
public record UpdateCategoryDto(string Name, int SortOrder);

