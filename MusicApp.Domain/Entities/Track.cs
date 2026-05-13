namespace MusicApp.Domain.Entities;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int DurationSeconds { get; set; }
    public string? AudioFilePath { get; set; }

    public Category? Category { get; set; }
    public ICollection<TrackAlbum> TrackAlbums { get; set; } = new List<TrackAlbum>();
    public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
    public ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
    public ICollection<Artist> Artists { get; set; } = new List<Artist>();
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
}