using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiCareerAcer.Api.Common;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Services;

namespace MiCareerAcer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiEnvelope<AuthResponse>>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (ok, err, data) = await _auth.RegisterAsync(req, ct);
        if (!ok) return BadRequest(ApiEnvelope<AuthResponse>.Fail(err ?? "register failed"));
        return Ok(ApiEnvelope<AuthResponse>.Ok(data!));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiEnvelope<AuthResponse>>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (ok, err, data) = await _auth.LoginAsync(req, ct);
        if (!ok) return Unauthorized(ApiEnvelope<AuthResponse>.Fail(err ?? "login failed"));
        return Ok(ApiEnvelope<AuthResponse>.Ok(data!));
    }
}
