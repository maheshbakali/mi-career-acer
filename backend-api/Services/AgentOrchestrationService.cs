using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiCareerAcer.Api.Data;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Models;
using MiCareerAcer.Api.Options;

namespace MiCareerAcer.Api.Services;

public class AgentOrchestrationService : IAgentOrchestrationService
{
    private readonly AppDbContext _db;
    private readonly IAgentClient _agent;
    private readonly IHtmlToPlainTextService _html;
    private readonly AgentServiceOptions _agentOpt;

    public AgentOrchestrationService(
        AppDbContext db,
        IAgentClient agent,
        IHtmlToPlainTextService html,
        IOptions<AgentServiceOptions> agentOpt)
    {
        _db = db;
        _agent = agent;
        _html = html;
        _agentOpt = agentOpt.Value;
    }

    public async Task<(bool ok, string? error, ProcessResultDto? result)> ProcessJobAsync(
        Guid jobId, Guid userId, string bearerToken, CancellationToken ct)
    {
        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId && j.UserId == userId, ct);
        if (job == null) return (false, "Job not found", null);

        var resume = await _db.ResumeFiles
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefaultAsync(ct);

        if (resume == null || string.IsNullOrWhiteSpace(resume.ExtractedText))
            return (false, "Upload a resume (PDF or DOCX) and wait for text extraction before processing.", null);

        var jdPlain = _html.ToPlainText(job.Description);
        var truncated = false;
        if (jdPlain.Length > _agentOpt.MaxContextChars)
        {
            jdPlain = jdPlain[.._agentOpt.MaxContextChars];
            truncated = true;
        }

        var resumeText = resume.ExtractedText!;
        if (resumeText.Length > _agentOpt.MaxContextChars)
            resumeText = resumeText[.._agentOpt.MaxContextChars];

        var inputBase = JsonSerializer.Serialize(new { jobId, resumeFileId = resume.Id });

        // 1) Assessment
        string assessJson;
        try
        {
            assessJson = await _agent.AssessJsonAsync(resumeText, jdPlain, bearerToken, ct);
        }
        catch (Exception ex)
        {
            return (false, $"Assessment failed: {ex.Message}", null);
        }

        decimal? score = null;
        try
        {
            using var assessDoc = JsonDocument.Parse(assessJson);
            if (assessDoc.RootElement.TryGetProperty("overall_score", out var os) && os.ValueKind == JsonValueKind.Number)
            {
                if (os.TryGetDecimal(out var d)) score = d;
                else if (os.TryGetDouble(out var x)) score = (decimal)x;
            }
        }
        catch { /* ignore parse */ }

        await SaveSessionAsync(jobId, userId, AgentType.Assessment, inputBase, assessJson, score, ct);

        // 2) Resume stream
        string tailored;
        try
        {
            tailored = await _agent.ResumeStreamTextAsync(resumeText, jdPlain, bearerToken, ct);
        }
        catch (Exception ex)
        {
            return (false, $"Resume agent failed: {ex.Message}", null);
        }

        await SaveSessionAsync(jobId, userId, AgentType.Resume, inputBase, tailored, null, ct);

        // 3) Interview
        string interviewJson;
        try
        {
            interviewJson = await _agent.InterviewJsonAsync(resumeText, jdPlain, bearerToken, ct);
        }
        catch (Exception ex)
        {
            return (false, $"Interview prep failed: {ex.Message}", null);
        }

        await SaveSessionAsync(jobId, userId, AgentType.Interview, inputBase, interviewJson, null, ct);

        object compatObj = JsonSerializer.Deserialize<object>(assessJson) ?? assessJson;
        object interviewObj = JsonSerializer.Deserialize<object>(interviewJson) ?? interviewJson;

        return (true, null, new ProcessResultDto
        {
            Compatibility = compatObj,
            TailoredResume = tailored,
            InterviewPrep = interviewObj,
            JobDescriptionTruncated = truncated
        });
    }

    private async Task SaveSessionAsync(
        Guid jobId, Guid userId, AgentType type, string inputPayload, string output, decimal? score, CancellationToken ct)
    {
        _db.AgentSessions.Add(new AgentSession
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            UserId = userId,
            AgentType = type,
            InputPayload = inputPayload,
            OutputPayload = output,
            CompatibilityScore = type == AgentType.Assessment ? score : null,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
