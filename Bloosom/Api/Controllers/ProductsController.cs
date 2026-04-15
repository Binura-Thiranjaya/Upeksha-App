using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin,admin,manager")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.Include(p => p.Category).Where(p => !p.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.Description,
                p.LongDescription,
                p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                p.IsFeatured,
                p.IsAvailable,
                Images = new object[0],
                p.CreatedAt,
                p.UpdatedAt
            }).ToListAsync();

        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p == null) return NotFound();
        return Ok(new
        {
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.Description,
            p.LongDescription,
            p.CategoryId,
            CategoryName = p.Category?.Name,
            p.IsFeatured,
            p.IsAvailable,
            Images = new object[0],
            p.CreatedAt,
            p.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Name.ToLower().Replace(' ', '-'),
            Price = dto.Price,
            Description = dto.Description,
            LongDescription = dto.LongDescription,
            CategoryId = dto.CategoryId,
            IsFeatured = dto.IsFeatured,
            IsAvailable = dto.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { id = product.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p == null) return NotFound();
        p.Name = dto.Name;
        p.Slug = dto.Name.ToLower().Replace(' ', '-');
        p.Price = dto.Price;
        p.Description = dto.Description;
        p.LongDescription = dto.LongDescription;
        p.CategoryId = dto.CategoryId;
        p.IsFeatured = dto.IsFeatured;
        p.IsAvailable = dto.IsAvailable;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        p.IsDeleted = !p.IsDeleted;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = p.Id, isDeleted = p.IsDeleted });
    }

    [HttpPost("{id:guid}/images")]
    public IActionResult UploadImage(Guid id)
    {
        // Placeholder - implement IFormFile handling and cloud/local storage
        return StatusCode(501, new { message = "Image upload not implemented in scaffold" });
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public IActionResult DeleteImage(Guid id, Guid imageId)
    {
        // Placeholder
        return StatusCode(501, new { message = "Image delete not implemented in scaffold" });
    }
}

public record CreateProductDto(
    string Name,
    decimal Price,
    string Description,
    string? LongDescription,
    Guid CategoryId,
    bool IsFeatured,
    bool IsAvailable);

public record UpdateProductDto(
    string Name,
    decimal Price,
    string Description,
    string? LongDescription,
    Guid CategoryId,
    bool IsFeatured,
    bool IsAvailable);

