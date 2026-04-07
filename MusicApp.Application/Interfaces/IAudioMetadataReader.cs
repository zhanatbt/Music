using MusicApp.Application.DTOs;

namespace MusicApp.Application.Interfaces;

public interface IAudioMetadataReader
{
    Task<AudioMetadataDto> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
