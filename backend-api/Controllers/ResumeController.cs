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
public class ResumeController : ControllerBase
{
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx" };

    private readonly AppDbContext _db;
    private readonly IFileStorageService _files;
    private readonly IAgentClient _agent;

    public ResumeController(AppDbContext db, IFileStorageService files, IAgentClient agent)
    {
        _db = db;
        _files = files;
        _agent = agent;
    }

    private Guid UserId => User.GetUserId() ?? throw new UnauthorizedAccessException();

    [HttpGet("current")]
    public async Task<ActionResult<ApiEnvelope<ResumeCurrentResponse?>>> Current(CancellationToken ct)
    {
        var r = await _db.ResumeFiles
            .Where(x => x.UserId == UserId)
            .OrderByDescending(x => x.UploadedAt)
            .FirstOrDefaultAsync(ct);
        if (r == null) return Ok(ApiEnvelope<ResumeCurrentResponse?>.Ok(null));

        var dto = new ResumeCurrentResponse
        {
            Id = r.Id,
            FileName = r.FileName,
            UploadedAt = r.UploadedAt,
            HasExtractedText = !string.IsNullOrWhiteSpace(r.ExtractedText)
        };
        return Ok(ApiEnvelope<ResumeCurrentResponse?>.Ok(dto));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(16_000_000)]
    public async Task<ActionResult<ApiEnvelope<ResumeCurrentResponse>>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiEnvelope<ResumeCurrentResponse>.Fail("No file uploaded"));

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
            return BadRequest(ApiEnvelope<ResumeCurrentResponse>.Fail("Only PDF and DOCX files are allowed"));

        await using var readStream = file.OpenReadStream();
        var rel = await _files.SaveFileAsync(readStream, file.FileName, ct);

        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth))
            return Unauthorized(ApiEnvelope<ResumeCurrentResponse>.Fail("Missing Authorization header"));

        string? extracted;
        await using (var fs = System.IO.File.OpenRead(_files.GetFullPath(rel)))
        {
            extracted = await _agent.ExtractResumeTextAsync(fs, file.FileName, auth, ct);
        }

        if (string.IsNullOrWhiteSpace(extracted))
            return BadRequest(ApiEnvelope<ResumeCurrentResponse>.Fail("Could not extract text from resume"));

        var entity = new ResumeFile
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            FileName = file.FileName,
            StoragePath = rel,
            ExtractedText = extracted,
            UploadedAt = DateTime.UtcNow
        };
        _db.ResumeFiles.Add(entity);
        await _db.SaveChangesAsync(ct);

        var dto = new ResumeCurrentResponse
        {
            Id = entity.Id,
            FileName = entity.FileName,
            UploadedAt = entity.UploadedAt,
            HasExtractedText = true
        };
        return Ok(ApiEnvelope<ResumeCurrentResponse>.Ok(dto));
    }
}
