using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCareerAcer.Api.Common;
using MiCareerAcer.Api.Data;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Extensions;
using MiCareerAcer.Api.Models;
using MiCareerAcer.Api.Services;

namespace MiCareerAcer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAgentOrchestrationService _orchestration;

    public JobsController(AppDbContext db, IMapper mapper, IAgentOrchestrationService orchestration)
    {
        _db = db;
        _mapper = mapper;
        _orchestration = orchestration;
    }

    private Guid UserId => User.GetUserId() ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<List<JobResponse>>>> List(CancellationToken ct)
    {
        var jobs = await _db.Jobs
            .Where(j => j.UserId == UserId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(ct);
        return Ok(ApiEnvelope<List<JobResponse>>.Ok(_mapper.Map<List<JobResponse>>(jobs)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiEnvelope<JobResponse>>> Get(Guid id, CancellationToken ct)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId, ct);
        if (job == null) return NotFound(ApiEnvelope<JobResponse>.Fail("Job not found"));
        return Ok(ApiEnvelope<JobResponse>.Ok(_mapper.Map<JobResponse>(job)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiEnvelope<JobResponse>>> Create([FromBody] CreateJobRequest req, CancellationToken ct)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Title = req.Title?.Trim() ?? "",
            Company = req.Company?.Trim() ?? "",
            Location = req.Location?.Trim() ?? "",
            Url = req.Url?.Trim() ?? "",
            Description = req.Description ?? "",
            CreatedAt = DateTime.UtcNow
        };
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiEnvelope<JobResponse>.Ok(_mapper.Map<JobResponse>(job)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiEnvelope<JobResponse>>> Update(Guid id, [FromBody] UpdateJobRequest req, CancellationToken ct)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId, ct);
        if (job == null) return NotFound(ApiEnvelope<JobResponse>.Fail("Job not found"));

        if (req.Title != null) job.Title = req.Title.Trim();
        if (req.Company != null) job.Company = req.Company.Trim();
        if (req.Location != null) job.Location = req.Location.Trim();
        if (req.Url != null) job.Url = req.Url.Trim();
        if (req.Description != null) job.Description = req.Description;

        await _db.SaveChangesAsync(ct);
        return Ok(ApiEnvelope<JobResponse>.Ok(_mapper.Map<JobResponse>(job)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiEnvelope<object?>>> Delete(Guid id, CancellationToken ct)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.UserId == UserId, ct);
        if (job == null) return NotFound(ApiEnvelope<object?>.Fail("Job not found"));
        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiEnvelope<object?>.Ok(null));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<ApiEnvelope<ProcessResultDto>>> Process(Guid id, CancellationToken ct)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(ApiEnvelope<ProcessResultDto>.Fail("Missing Authorization header"));

        var (ok, err, result) = await _orchestration.ProcessJobAsync(id, UserId, auth, ct);
        if (!ok) return BadRequest(ApiEnvelope<ProcessResultDto>.Fail(err ?? "Process failed"));
        return Ok(ApiEnvelope<ProcessResultDto>.Ok(result!));
    }
}
