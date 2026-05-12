using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class ArtistRepository(AppDbContext context) : IArtistRepository
{
    public async Task<IReadOnlyList<Artist>> GetAllAsync(CancellationToken ct = default)
        => await context.Artists.OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<Artist?> GetByNameAsync(string name, CancellationToken ct = default)
        => await context.Artists.FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task AddAsync(Artist artist, CancellationToken ct = default)
    {
        context.Artists.Add(artist);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Artist artist, CancellationToken ct = default)
    {
        context.Artists.Update(artist);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        var artist = await context.Artists.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (artist is null) return false;
        context.Artists.Remove(artist);
        await context.SaveChangesAsync(ct);
        return true;
    }
}