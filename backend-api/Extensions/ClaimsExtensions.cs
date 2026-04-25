using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiCareerAcer.Api.Extensions;

public static class ClaimsExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? user)
    {
        var s = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(s, out var id) ? id : null;
    }
}
