using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class TrackRepository : ITrackRepository
{
    private readonly AppDbContext _context;

    public TrackRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Track>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await IncludeGraph(_context.Tracks)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Track?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await IncludeGraph(_context.Tracks)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Track?> FindDuplicateAsync(
        string title,
        int artistId,
        int? albumId,
        string? deezerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = title.Trim();
        var normalizedDeezerId = string.IsNullOrWhiteSpace(deezerId) ? null : deezerId.Trim();

        var query = IncludeGraph(_context.Tracks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedDeezerId))
        {
            var byDeezerId = await query.FirstOrDefaultAsync(
                t => t.DeezerId != null && t.DeezerId == normalizedDeezerId,
                cancellationToken);

            if (byDeezerId is not null)
            {
                return byDeezerId;
            }
        }

        return await query.FirstOrDefaultAsync(
            t => t.ArtistId == artistId &&
                 t.AlbumId == albumId &&
                 t.Title == normalizedTitle,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> SearchAsync(string? query, int? genreId, int? categoryId, CancellationToken cancellationToken = default)
    {
        var normalized = query?.Trim();

        var trackQuery = IncludeGraph(_context.Tracks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            trackQuery = trackQuery.Where(t =>
                t.Title.Contains(normalized) ||
                t.Artist!.Name.Contains(normalized) ||
                (t.Album != null && t.Album.Title.Contains(normalized)) ||
                t.Genre!.Name.Contains(normalized) ||
                (t.Category != null && t.Category.Name.Contains(normalized)));
        }

        if (genreId.HasValue)
        {
            trackQuery = trackQuery.Where(t => t.GenreId == genreId.Value);
        }

        if (categoryId.HasValue)
        {
            trackQuery = trackQuery.Where(t => t.CategoryId == categoryId.Value);
        }

        return await trackQuery
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Track track, CancellationToken cancellationToken = default)
    {
        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Track> IncludeGraph(IQueryable<Track> query)
    {
        return query
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Include(t => t.Category);
    }
}
