namespace MiCareerAcer.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;

    public FileStorageService(IWebHostEnvironment env)
    {
        _uploadRoot = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public string GetFullPath(string relativePath) => Path.Combine(_uploadRoot, relativePath);

    public async Task<string> SaveFileAsync(Stream stream, string originalFileName, CancellationToken ct)
    {
        var ext = Path.GetExtension(originalFileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var full = Path.Combine(_uploadRoot, name);
        await using var fs = File.Create(full);
        await stream.CopyToAsync(fs, ct);
        return name;
    }
}
