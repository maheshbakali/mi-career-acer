namespace MiCareerAcer.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Plan { get; set; } = "free";
    public DateTime CreatedAt { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<AgentSession> AgentSessions { get; set; } = new List<AgentSession>();
    public ICollection<ResumeFile> ResumeFiles { get; set; } = new List<ResumeFile>();
}
