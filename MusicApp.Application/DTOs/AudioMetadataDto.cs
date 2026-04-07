namespace MusicApp.Application.DTOs;

public class AudioMetadataDto
{
    public string FilePath { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ArtistName { get; init; } = string.Empty;
    public string AlbumTitle { get; init; } = string.Empty;
    public string? GenreName { get; init; }
    public int DurationSeconds { get; init; }
}
