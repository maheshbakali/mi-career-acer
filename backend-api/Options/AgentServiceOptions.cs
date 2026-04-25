namespace MiCareerAcer.Api.Options;

public class AgentServiceOptions
{
    public const string SectionName = "AgentService";

    public string BaseUrl { get; set; } = "http://localhost:8000";
    public int MaxContextChars { get; set; } = 12000;
}
