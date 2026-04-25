namespace MiCareerAcer.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = "";
    public string Issuer { get; set; } = "mi-career-acer";
    public string Audience { get; set; } = "mi-career-acer-clients";
    public int ExpiryMinutes { get; set; } = 10080;
}
