using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IArtistRepository
{
    Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Artist?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Artist artist, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
}
