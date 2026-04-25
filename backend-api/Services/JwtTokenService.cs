using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiCareerAcer.Api.Models;
using MiCareerAcer.Api.Options;

namespace MiCareerAcer.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;

    public JwtTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.ExpiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
