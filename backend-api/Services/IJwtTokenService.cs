using MiCareerAcer.Api.Models;

namespace MiCareerAcer.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
