namespace MiCareerAcer.Api.Models;

public class Job
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Url { get; set; } = "";
    /// <summary>Rich-text job description (HTML).</summary>
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public ICollection<AgentSession> AgentSessions { get; set; } = new List<AgentSession>();
}
