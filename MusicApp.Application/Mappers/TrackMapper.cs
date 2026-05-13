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
            Artist = string.Join(", ", track.TrackArtists
                .Where(x => x.Artist is not null)
                .Select(x => x.Artist!.Name)
                .OrderBy(x => x)),
            Album = string.Join(", ", track.TrackAlbums
                .Where(x => x.Album is not null)
                .Select(x => x.Album!.Title)
                .OrderBy(x => x)),
            AlbumIds = track.TrackAlbums.Select(x => x.AlbumId).ToList(),
            Genre = string.Join(", ", track.TrackGenres
                .Where(x => x.Genre is not null)
                .Select(x => x.Genre!.Name)
                .OrderBy(x => x)),
            Category = track.Category?.Name ?? string.Empty,
            DurationSeconds = track.DurationSeconds,
            PreviewUrl = track.AudioFilePath
        };
    }
}
