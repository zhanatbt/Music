namespace MusicApp.Application.DTOs;

public class AlbumDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Artists { get; init; } = string.Empty;
    public int TrackCount { get; init; }
}