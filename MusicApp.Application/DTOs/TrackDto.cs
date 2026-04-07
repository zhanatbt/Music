namespace MusicApp.Application.DTOs;

public class TrackDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Artist { get; init; } = string.Empty;
    public string Album { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int DurationSeconds { get; init; }
    public string? PreviewUrl { get; init; }
    public bool IsLocalFile { get; init; }
    public string SourceType { get; init; } = string.Empty;
}
