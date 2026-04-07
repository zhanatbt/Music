using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class ArtistRepository : IArtistRepository
{
    private readonly AppDbContext _context;

    public ArtistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Artist?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Artists.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task AddAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
