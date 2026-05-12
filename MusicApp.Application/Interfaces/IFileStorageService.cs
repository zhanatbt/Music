namespace MusicApp.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(string sourceFilePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}
