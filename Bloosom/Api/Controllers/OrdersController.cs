using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "it_admin,admin,staff")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Orders.Where(o => !o.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o => o.OrderNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.OrderStatus == status);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _db.Orders.Include(o => o.Items).Include(o => o.AddOns).FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        if (order == null) return NotFound();
        order.OrderStatus = dto.OrderStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPatch("{id:guid}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusDto dto)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        if (order == null) return NotFound();
        order.PaymentStatus = dto.PaymentStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(order);
    }
}

public record UpdateOrderStatusDto(string OrderStatus);
public record UpdatePaymentStatusDto(string PaymentStatus);

