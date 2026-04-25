namespace MiCareerAcer.Api.DTOs;

public class ResumeCurrentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public bool HasExtractedText { get; set; }
}
