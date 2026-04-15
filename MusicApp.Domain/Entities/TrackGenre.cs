namespace MusicApp.Domain.Entities;

public class TrackGenre
{
    public int TrackId { get; set; }
    public int GenreId { get; set; }

    public Track? Track { get; set; }
    public Genre? Genre { get; set; }
}
