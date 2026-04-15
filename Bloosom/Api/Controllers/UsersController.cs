using Bloosom.Domain.Entities;
using Bloosom.Infrastructure.Persistence;
using Bloosom.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UsersController(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Users.Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt, u.LastLogin }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "it_admin,admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return Conflict(new { error = "Email already exists" });
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PasswordHash = _hasher.Hash(dto.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, new { id = user.Id });
    }
}

public record CreateUserDto(string FullName, string Email, string Password, string Role);
