using MusicApp.Application.DTOs;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Mappers;

public static class TrackMapper
{
    public static TrackDto ToDto(Track track)
    {
        return new TrackDto
        {
            Id = track.Id,
            Title = track.Title,
            Artist = track.Artist?.Name ?? string.Empty,
            Album = track.Album?.Title ?? string.Empty,
            Genre = track.Genre?.Name ?? string.Empty,
            Category = track.Category?.Name ?? string.Empty,
            DurationSeconds = track.DurationSeconds,
            PreviewUrl = track.PreviewUrl,
            IsLocalFile = string.Equals(track.SourceType, "LocalFile", StringComparison.OrdinalIgnoreCase),
            SourceType = track.SourceType
        };
    }
}
