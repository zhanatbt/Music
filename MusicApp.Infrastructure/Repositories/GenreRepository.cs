using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;

    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Genres.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Genres.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Genres.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
