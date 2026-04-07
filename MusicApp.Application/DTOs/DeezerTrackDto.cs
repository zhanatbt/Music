namespace MusicApp.Application.DTOs;

public class DeezerTrackDto
{
    public string DeezerId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ArtistName { get; init; } = string.Empty;
    public string AlbumTitle { get; init; } = string.Empty;
    public string? PreviewUrl { get; init; }
    public int DurationSeconds { get; init; }
}
