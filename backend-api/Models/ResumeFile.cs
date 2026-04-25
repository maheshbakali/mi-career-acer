namespace MiCareerAcer.Api.Models;

public class ResumeFile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string FileName { get; set; } = "";
    public string StoragePath { get; set; } = "";
    public string? ExtractedText { get; set; }
    public DateTime UploadedAt { get; set; }
}
