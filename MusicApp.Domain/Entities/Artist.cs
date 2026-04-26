namespace MusicApp.Domain.Entities;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
