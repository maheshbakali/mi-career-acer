using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCareerAcer.Api.Common;
using MiCareerAcer.Api.Data;
using MiCareerAcer.Api.DTOs;
using MiCareerAcer.Api.Extensions;

namespace MiCareerAcer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public SessionsController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private Guid UserId => User.GetUserId() ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<List<AgentSessionResponse>>>> List(CancellationToken ct)
    {
        var list = await _db.AgentSessions
            .Where(s => s.UserId == UserId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(200)
            .ToListAsync(ct);
        return Ok(ApiEnvelope<List<AgentSessionResponse>>.Ok(_mapper.Map<List<AgentSessionResponse>>(list)));
    }

    [HttpGet("runs")]
    public async Task<ActionResult<ApiEnvelope<List<SessionGroupDto>>>> Runs(CancellationToken ct)
    {
        var sessions = await _db.AgentSessions
            .Include(s => s.Job)
            .Where(s => s.UserId == UserId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(300)
            .ToListAsync(ct);

        var groups = sessions
            .GroupBy(s => new { s.JobId, Minute = new DateTime(s.CreatedAt.Year, s.CreatedAt.Month, s.CreatedAt.Day, s.CreatedAt.Hour, s.CreatedAt.Minute, 0, DateTimeKind.Utc) })
            .OrderByDescending(g => g.Key.Minute)
            .ThenByDescending(g => g.Max(x => x.CreatedAt))
            .Take(40)
            .Select(g => new SessionGroupDto
            {
                JobId = g.Key.JobId,
                JobTitle = g.First().Job?.Title,
                RunAt = g.Max(x => x.CreatedAt),
                Sessions = _mapper.Map<List<AgentSessionResponse>>(g.OrderBy(x => x.CreatedAt).ToList())
            })
            .ToList();

        return Ok(ApiEnvelope<List<SessionGroupDto>>.Ok(groups));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiEnvelope<AgentSessionResponse>>> Get(Guid id, CancellationToken ct)
    {
        var s = await _db.AgentSessions.FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId, ct);
        if (s == null) return NotFound(ApiEnvelope<AgentSessionResponse>.Fail("Session not found"));
        return Ok(ApiEnvelope<AgentSessionResponse>.Ok(_mapper.Map<AgentSessionResponse>(s)));
    }
}
