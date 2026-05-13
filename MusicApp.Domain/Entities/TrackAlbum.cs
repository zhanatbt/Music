namespace MusicApp.Domain.Entities;

public class TrackAlbum
{
    public int TrackId { get; set; }
    public int AlbumId { get; set; }
    public Track? Track { get; set; }
    public Album? Album { get; set; }
}