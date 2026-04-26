using System.ComponentModel;

namespace MusicApp.Application.DTOs;

public class DeezerTrackDto
{
    public string DeezerId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    [Browsable(false)]
    public IReadOnlyList<string> ArtistNames { get; set; } = Array.Empty<string>();
    public string AlbumTitle { get; init; } = string.Empty;
    public string? GenreName { get; set; }
    [Browsable(false)]
    public IReadOnlyList<string> GenreNames { get; set; } = Array.Empty<string>();
    public string? PreviewUrl { get; init; }
    public int DurationSeconds { get; init; }
}
