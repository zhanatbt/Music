using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeGenreRepository : IGenreRepository
{
    private readonly List<Genre> _genres = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Genre>)_genres.OrderBy(x => x.Name).ToList());
    }

    public Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_genres.FirstOrDefault(x => x.Id == id));
    }

    public Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_genres.FirstOrDefault(x => x.Name == name));
    }

    public Task AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        if (genre.Id == 0)
        {
            genre.Id = _nextId++;
        }

        _genres.Add(genre);
        return Task.CompletedTask;
    }
}
