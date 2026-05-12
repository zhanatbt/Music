using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class GenreRepository(AppDbContext context) : IGenreRepository
{
    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken ct = default)
        => await context.Genres.OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default)
        => await context.Genres.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default)
        => await context.Genres.FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task AddAsync(Genre genre, CancellationToken ct = default)
    {
        context.Genres.Add(genre);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Genre genre, CancellationToken ct = default)
    {
        context.Genres.Update(genre);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        var genre = await context.Genres.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (genre is null) return false;
        context.Genres.Remove(genre);
        await context.SaveChangesAsync(ct);
        return true;
    }
}