using System.Net.Http;
using MusicApp.Application.Interfaces;

namespace MusicApp.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _storageDirectory;

    public LocalFileStorageService(HttpClient httpClient, string baseDirectory)
    {
        _httpClient = httpClient;
        _storageDirectory = Path.Combine(baseDirectory, "music_storage");
        Directory.CreateDirectory(_storageDirectory);
    }

    public Task<string> SaveAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new ArgumentException("Source file path is required.", nameof(sourceFilePath));
        }

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source audio file not found.", sourceFilePath);
        }

        var extension = Path.GetExtension(sourceFilePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mp3";
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_storageDirectory, fileName);
        File.Copy(sourceFilePath, destinationPath, overwrite: true);
        return Task.FromResult(GetRelativePath(destinationPath));
    }

    public async Task<string> SaveFromUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Source URL is required.", nameof(url));
        }

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var extension = GetExtensionFromResponse(response) ?? ".mp3";
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_storageDirectory, fileName);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = File.OpenWrite(destinationPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return GetRelativePath(destinationPath);
    }

    public string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path is required.", nameof(relativePath));
        }

        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        return Path.GetFullPath(Path.Combine(_storageDirectory, relativePath));
    }

    private string GetRelativePath(string absolutePath)
    {
        return Path.GetRelativePath(_storageDirectory, absolutePath);
    }

    private static string? GetExtensionFromResponse(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrWhiteSpace(mediaType))
        {
            return null;
        }

        if (mediaType.Equals("audio/mpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ".mp3";
        }

        if (mediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            return ".mp3";
        }

        return null;
    }
}
