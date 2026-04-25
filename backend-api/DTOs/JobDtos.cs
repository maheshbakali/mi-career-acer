using System.ComponentModel.DataAnnotations;

namespace MiCareerAcer.Api.DTOs;

public class CreateJobRequest
{
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Url { get; set; } = "";

    [Required]
    public string Description { get; set; } = ""; // HTML
}

public class UpdateJobRequest
{
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Description { get; set; }
}

public class JobResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Url { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ProcessResultDto
{
    public object Compatibility { get; set; } = null!;
    public string TailoredResume { get; set; } = "";
    public object InterviewPrep { get; set; } = null!;
    public bool JobDescriptionTruncated { get; set; }
}
