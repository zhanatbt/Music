using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeAlbumRepository : IAlbumRepository
{
    private readonly List<Album> _albums = [];
    private int _nextId = 1;

    public Task<Album?> GetByTitleAndArtistAsync(string title, int artistId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_albums.FirstOrDefault(x => x.Title == title && x.ArtistId == artistId));
    }

    public Task AddAsync(Album album, CancellationToken cancellationToken = default)
    {
        if (album.Id == 0)
        {
            album.Id = _nextId++;
        }

        _albums.Add(album);
        return Task.CompletedTask;
    }
}
