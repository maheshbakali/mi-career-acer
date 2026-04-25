namespace MiCareerAcer.Api.Models;

public class AgentSession
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public AgentType AgentType { get; set; }
    public string InputPayload { get; set; } = "{}";
    public string OutputPayload { get; set; } = "";
    public decimal? CompatibilityScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
