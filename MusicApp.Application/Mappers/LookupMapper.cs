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
}
