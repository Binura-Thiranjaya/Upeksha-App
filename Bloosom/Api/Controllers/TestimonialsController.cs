using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TestimonialsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TestimonialsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "it_admin,admin,manager")]
    public async Task<IActionResult> GetAll([FromQuery] bool? isApproved, [FromQuery] bool? isFeatured, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Testimonials.Where(t => !t.IsDeleted).AsQueryable();
        if (isApproved.HasValue) query = query.Where(t => t.IsApproved == isApproved.Value);
        if (isFeatured.HasValue) query = query.Where(t => t.IsFeatured == isFeatured.Value);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total });
    }

    [HttpPost]
    [Authorize(Roles = "it_admin,admin,manager")]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialDto dto)
    {
        var t = new Testimonial
        {
            Id = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            Rating = dto.Rating,
            ReviewText = dto.ReviewText,
            CustomerImage = dto.CustomerImage,
            IsApproved = dto.IsApproved,
            IsFeatured = dto.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Testimonials.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = t.Id }, t);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "it_admin,admin,manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialDto dto)
    {
        var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (t == null) return NotFound();
        t.CustomerName = dto.CustomerName;
        t.Rating = dto.Rating;
        t.ReviewText = dto.ReviewText;
        t.IsApproved = dto.IsApproved;
        t.IsFeatured = dto.IsFeatured;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle-approval")]
    [Authorize(Roles = "it_admin,admin,manager")]
    public async Task<IActionResult> ToggleApproval(Guid id)
    {
        var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (t == null) return NotFound();
        t.IsApproved = !t.IsApproved;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = t.Id, isApproved = t.IsApproved });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "it_admin,admin,manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();
        t.IsDeleted = !t.IsDeleted;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = t.Id, isDeleted = t.IsDeleted });
    }

    [HttpPost("submit")]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] SubmitReviewDto dto)
    {
        var t = new Testimonial
        {
            Id = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            Rating = dto.Rating,
            ReviewText = dto.ReviewText,
            CustomerImage = dto.CustomerImage,
            IsApproved = false,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Testimonials.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Submit), new { id = t.Id }, new { id = t.Id });
    }
}

public record CreateTestimonialDto(string CustomerName, int Rating, string ReviewText, string? CustomerImage, bool IsApproved, bool IsFeatured);
public record UpdateTestimonialDto(string CustomerName, int Rating, string ReviewText, bool IsApproved, bool IsFeatured);
public record SubmitReviewDto(string CustomerName, int Rating, string ReviewText, string? CustomerImage);

