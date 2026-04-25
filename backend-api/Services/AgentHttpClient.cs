using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiCareerAcer.Api.Services;

public class AgentHttpClient : IAgentClient
{
    private readonly HttpClient _http;

    public AgentHttpClient(HttpClient http) => _http = http;

    private static string NormalizeToken(string bearerToken)
    {
        var t = bearerToken.Trim();
        if (t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            t = t[7..].Trim();
        return t;
    }

    public async Task<string?> ExtractResumeTextAsync(Stream fileStream, string fileName, string bearerToken, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMime(fileName));
        content.Add(fileContent, "file", fileName);

        using var req = new HttpRequestMessage(HttpMethod.Post, "resume/extract-text") { Content = content };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NormalizeToken(bearerToken));

        var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (!root.GetProperty("success").GetBoolean())
            throw new InvalidOperationException(root.TryGetProperty("error", out var e) ? e.GetString() ?? "extract failed" : "extract failed");

        if (!root.TryGetProperty("data", out var data) || !data.TryGetProperty("text", out var textEl))
            return null;
        return textEl.GetString();
    }

    public async Task<string> AssessJsonAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { resume_text = resumeText, job_description = jobDescriptionPlain });
        using var req = new HttpRequestMessage(HttpMethod.Post, "agents/assess")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NormalizeToken(bearerToken));
        var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (!root.GetProperty("success").GetBoolean())
            throw new InvalidOperationException(root.TryGetProperty("error", out var e) ? e.GetString() ?? "assess failed" : "assess failed");
        return root.GetProperty("data").GetRawText();
    }

    public async Task<string> InterviewJsonAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { resume_text = resumeText, job_description = jobDescriptionPlain });
        using var req = new HttpRequestMessage(HttpMethod.Post, "agents/interview/questions")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NormalizeToken(bearerToken));
        var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (!root.GetProperty("success").GetBoolean())
            throw new InvalidOperationException(root.TryGetProperty("error", out var e) ? e.GetString() ?? "interview failed" : "interview failed");
        return root.GetProperty("data").GetRawText();
    }

    public async Task<string> ResumeStreamTextAsync(string resumeText, string jobDescriptionPlain, string bearerToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { resume_text = resumeText, job_description = jobDescriptionPlain });
        using var req = new HttpRequestMessage(HttpMethod.Post, "agents/resume/stream")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", NormalizeToken(bearerToken));
        using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        res.EnsureSuccessStatusCode();
        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        return await ReadSseContentAsync(stream, ct);
    }

    private static async Task<string> ReadSseContentAsync(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream);
        var sb = new StringBuilder();
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) break;
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
            var payload = line[5..].Trim();
            if (payload.Length == 0 || payload == "[DONE]") continue;
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("content", out var c))
                    sb.Append(c.GetString());
            }
            catch
            {
                // ignore malformed sse line
            }
        }
        return sb.ToString();
    }

    private static string GetMime(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
