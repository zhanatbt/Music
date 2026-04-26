namespace MusicApp.Domain.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
