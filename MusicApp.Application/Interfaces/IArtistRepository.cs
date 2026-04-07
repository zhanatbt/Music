using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IArtistRepository
{
    Task<Artist?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Artist artist, CancellationToken cancellationToken = default);
}
