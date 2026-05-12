using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IGenreRepository
{
    Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Genre genre, CancellationToken cancellationToken = default);
    Task UpdateAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
}
