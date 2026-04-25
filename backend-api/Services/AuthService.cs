using Microsoft.EntityFrameworkCore;
using MiCareerAcer.Api.Data;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Models;

namespace MiCareerAcer.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthService(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<(bool ok, string? error, AuthResponse? data)> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return (false, "Email already registered", null);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Plan = "free",
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var token = _jwt.CreateToken(user);
        return (true, null, new AuthResponse { Token = token, UserId = user.Id, Email = user.Email });
    }

    public async Task<(bool ok, string? error, AuthResponse? data)> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return (false, "Invalid email or password", null);

        var token = _jwt.CreateToken(user);
        return (true, null, new AuthResponse { Token = token, UserId = user.Id, Email = user.Email });
    }
}
