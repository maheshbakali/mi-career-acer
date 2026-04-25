namespace MiCareerAcer.Api.DTOs;

public class AgentSessionResponse
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string AgentType { get; set; } = "";
    public string InputPayload { get; set; } = "";
    public string OutputPayload { get; set; } = "";
    public decimal? CompatibilityScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SessionGroupDto
{
    public Guid JobId { get; set; }
    public string? JobTitle { get; set; }
    public DateTime RunAt { get; set; }
    public IReadOnlyList<AgentSessionResponse> Sessions { get; set; } = Array.Empty<AgentSessionResponse>();
}
