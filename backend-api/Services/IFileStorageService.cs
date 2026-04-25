namespace MiCareerAcer.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string originalFileName, CancellationToken ct);
    string GetFullPath(string relativePath);
}
