using MusicApp.Application.DTOs;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Mappers;

public static class LookupMapper
{
    public static GenreDto ToDto(Genre genre) => new() { Id = genre.Id, Name = genre.Name };
    public static CategoryDto ToDto(Category category) => new() { Id = category.Id, Name = category.Name };
    public static ArtistDto ToDto(Artist artist) => new() { Id = artist.Id, Name = artist.Name };

    public static PlaylistDto ToDto(Playlist playlist) => new()
    {
        Id = playlist.Id,
        Name = playlist.Name,
        TrackCount = playlist.PlaylistTracks.Count
    };

    public static AlbumDto ToDto(Album album) => new()
    {
        Id = album.Id,
        Title = album.Title,
        Artists = album.AlbumArtists.Count != 0
            ? string.Join(", ", album.AlbumArtists
                .Where(x => x.Artist != null)
                .Select(x => x.Artist!.Name)
                .OrderBy(x => x))
            : album.Artist?.Name ?? string.Empty,
        TrackCount = album.Tracks.Count
    };
}