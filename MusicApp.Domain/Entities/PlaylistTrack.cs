namespace MusicApp.Domain.Entities;

public class PlaylistTrack
{
    public int PlaylistId { get; set; }
    public int TrackId { get; set; }
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    public Playlist? Playlist { get; set; }
    public Track? Track { get; set; }
}
