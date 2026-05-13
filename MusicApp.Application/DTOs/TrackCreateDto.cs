namespace MusicApp.Application.DTOs;

public class TrackCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public IReadOnlyList<string> ArtistNames { get; set; } = [];
    public IReadOnlyList<int> AlbumIds { get; set; } = [];
    public int GenreId { get; set; }
    public string? GenreName { get; set; }
    public IReadOnlyList<string> GenreNames { get; set; } = [];
    public string? CategoryName { get; set; }
    public int? CategoryId { get; set; }
    public int DurationSeconds { get; set; }
    public string? AudioFilePath { get; set; }
}
