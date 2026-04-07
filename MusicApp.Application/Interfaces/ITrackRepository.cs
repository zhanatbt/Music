using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ITrackRepository
{
    Task<IReadOnlyList<Track>> SearchAsync(string? query, int? genreId, int? categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Track>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Track?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Track track, CancellationToken cancellationToken = default);
}
