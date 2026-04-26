namespace MusicApp.Application.DTOs;

public class TrackCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public IReadOnlyList<string> ArtistNames { get; set; } = Array.Empty<string>();
    public string? AlbumTitle { get; set; }
    public int GenreId { get; set; }
    public string? GenreName { get; set; }
    public IReadOnlyList<string> GenreNames { get; set; } = Array.Empty<string>();
    public string? CategoryName { get; set; }
    public int? CategoryId { get; set; }
    public int DurationSeconds { get; set; }
    public string? DeezerId { get; set; }
    public string? PreviewUrl { get; set; }
    public string? AudioFilePath { get; set; }
    public string SourceType { get; set; } = "Manual";
}
