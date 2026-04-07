namespace MusicApp.Domain.Entities;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public int? AlbumId { get; set; }
    public int GenreId { get; set; }
    public int? CategoryId { get; set; }
    public int DurationSeconds { get; set; }
    public string? DeezerId { get; set; }
    public string? PreviewUrl { get; set; }
    public string SourceType { get; set; } = "Manual";

    public Artist? Artist { get; set; }
    public Album? Album { get; set; }
    public Genre? Genre { get; set; }
    public Category? Category { get; set; }
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}
