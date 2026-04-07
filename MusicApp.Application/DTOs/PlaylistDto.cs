namespace MusicApp.Application.DTOs;

public class PlaylistDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int TrackCount { get; init; }
    public string DisplayText => $"{Name} ({TrackCount})";

    public override string ToString()
    {
        return DisplayText;
    }
}
