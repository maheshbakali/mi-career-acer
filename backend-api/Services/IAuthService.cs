using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Models;

namespace MiCareerAcer.Api.Services;

public interface IAuthService
{
    Task<(bool ok, string? error, AuthResponse? data)> RegisterAsync(RegisterRequest req, CancellationToken ct);
    Task<(bool ok, string? error, AuthResponse? data)> LoginAsync(LoginRequest req, CancellationToken ct);
}
