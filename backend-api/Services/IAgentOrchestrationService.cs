using MiCareerAcer.Api.DTOs;

namespace MiCareerAcer.Api.Services;

public interface IAgentOrchestrationService
{
    Task<(bool ok, string? error, ProcessResultDto? result)> ProcessJobAsync(Guid jobId, Guid userId, string bearerToken, CancellationToken ct);
}
