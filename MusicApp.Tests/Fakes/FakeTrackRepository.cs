using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeTrackRepository : ITrackRepository
{
    private readonly List<Track> _tracks = [];

    public Task<IReadOnlyList<Track>> SearchAsync(string? query, int? genreId, int? categoryId, CancellationToken cancellationToken = default)
    {
        IEnumerable<Track> result = _tracks;

        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (genreId.HasValue)
        {
            result = result.Where(t => t.GenreId == genreId.Value);
        }

        if (categoryId.HasValue)
        {
            result = result.Where(t => t.CategoryId == categoryId.Value);
        }

        return Task.FromResult((IReadOnlyList<Track>)result.ToList());
    }

    public Task<IReadOnlyList<Track>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Track>)_tracks.ToList());
    }

    public Task<Track?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tracks.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(Track track, CancellationToken cancellationToken = default)
    {
        _tracks.Add(track);
        return Task.CompletedTask;
    }
}
