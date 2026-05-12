using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class AlbumRepository(AppDbContext context) : IAlbumRepository
{
    public async Task<IReadOnlyList<Album>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await context.Albums
            .Include(x => x.AlbumArtists).ThenInclude(x => x.Artist)
            .Include(x => x.Artist)
            .Include(x => x.Tracks)
            .OrderBy(x => x.Title)
            .ToListAsync(ct);

    public async Task<Album?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await context.Albums
            .Include(x => x.AlbumArtists).ThenInclude(x => x.Artist)
            .Include(x => x.Artist)
            .Include(x => x.Tracks).ThenInclude(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Include(x => x.Tracks).ThenInclude(t => t.TrackGenres).ThenInclude(tg => tg.Genre)
            .Include(x => x.Tracks).ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Album?> GetByTitleAndArtistAsync(string title, int artistId, CancellationToken ct = default)
        => await context.Albums.FirstOrDefaultAsync(x => x.Title == title && x.ArtistId == artistId, ct);

    public async Task AddAsync(Album album, CancellationToken ct = default)
    {
        context.Albums.Add(album);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Album album, CancellationToken ct = default)
    {
        context.Albums.Update(album);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        var album = await context.Albums.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (album is null) return false;
        context.Albums.Remove(album);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task AddArtistAsync(int albumId, int artistId, CancellationToken ct = default)
    {
        var exists = await context.AlbumArtists.AnyAsync(x => x.AlbumId == albumId && x.ArtistId == artistId, ct);
        if (!exists)
        {
            context.AlbumArtists.Add(new AlbumArtist { AlbumId = albumId, ArtistId = artistId });
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveArtistAsync(int albumId, int artistId, CancellationToken ct = default)
    {
        var item = await context.AlbumArtists.FirstOrDefaultAsync(x => x.AlbumId == albumId && x.ArtistId == artistId,
            ct);
        if (item is not null)
        {
            context.AlbumArtists.Remove(item);
            await context.SaveChangesAsync(ct);
        }
    }
}