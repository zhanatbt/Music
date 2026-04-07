using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class AlbumRepository : IAlbumRepository
{
    private readonly AppDbContext _context;

    public AlbumRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Album?> GetByTitleAndArtistAsync(string title, int artistId, CancellationToken cancellationToken = default)
    {
        return await _context.Albums.FirstOrDefaultAsync(x => x.Title == title && x.ArtistId == artistId, cancellationToken);
    }

    public async Task AddAsync(Album album, CancellationToken cancellationToken = default)
    {
        _context.Albums.Add(album);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
