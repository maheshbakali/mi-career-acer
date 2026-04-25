namespace MiCareerAcer.Api.Services;

public interface IAgentClient
{
    Task<string?> ExtractResumeTextAsync(Stream fileStream, string fileName, string bearerToken, CancellationToken ct);
    Task<string> AssessJsonAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct);
    Task<string> InterviewJsonAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct);
    Task<string> ResumeStreamTextAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct);
}
