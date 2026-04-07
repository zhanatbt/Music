using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeArtistRepository : IArtistRepository
{
    private readonly List<Artist> _artists = [];
    private int _nextId = 1;

    public Task<Artist?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_artists.FirstOrDefault(x => x.Name == name));
    }

    public Task AddAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        if (artist.Id == 0)
        {
            artist.Id = _nextId++;
        }

        _artists.Add(artist);
        return Task.CompletedTask;
    }
}
