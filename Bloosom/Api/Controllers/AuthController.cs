using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bloosom.Infrastructure.Persistence;
using Bloosom.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Bloosom.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext db, IPasswordHasher hasher, IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
        if (user == null || !user.IsActive || !_hasher.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { error = "Invalid email or password" });
        }

        var jwtKey = _configuration["Jwt:Key"] ?? "VerySecretKeyChangeThis_AtLeast32Chars!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "floara.local";
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        if (keyBytes.Length < 32)
        {
            return StatusCode(500, new { error = "JWT configuration is invalid" });
        }
        var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Name, user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new LoginResponseDto(
            tokenString,
            new LoginUserDto(user.Id, user.FullName, user.Email, user.Role)));
    }
}

public record LoginRequestDto(string Email, string Password);
public record LoginUserDto(Guid Id, string FullName, string Email, string Role);
public record LoginResponseDto(string Token, LoginUserDto User);

