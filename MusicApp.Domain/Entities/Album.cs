namespace MusicApp.Domain.Entities;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? ReleaseDateUtc { get; set; }
    public int ArtistId { get; set; }

    public Artist? Artist { get; set; }
    public ICollection<TrackAlbum> TrackAlbums { get; set; } = new List<TrackAlbum>();
    public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
}
