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
            t => t.AlbumId == albumId &&
                 t.Title == normalizedTitle &&
                 t.TrackArtists.Any(ta => ta.ArtistId == artistId),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> SearchAsync(
        string? query,
        int? genreId,
        int? categoryId,
        string? album,
        string? genre,
        string? title,
        string? artist,
        CancellationToken cancellationToken = default)
    {
        var normalized = query?.Trim();
        var normalizedAlbum = album?.Trim();
        var normalizedGenre = genre?.Trim();
        var normalizedTitle = title?.Trim();
        var normalizedArtist = artist?.Trim();

        var trackQuery = IncludeGraph(_context.Tracks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedTitle))
        {
            trackQuery = trackQuery.Where(t => t.Title.Contains(normalizedTitle));
        }

        if (!string.IsNullOrWhiteSpace(normalizedAlbum))
        {
            trackQuery = trackQuery.Where(t => t.Album != null && t.Album.Title.Contains(normalizedAlbum));
        }

        if (!string.IsNullOrWhiteSpace(normalizedArtist))
        {
            trackQuery = trackQuery.Where(t => t.TrackArtists.Any(ta => ta.Artist != null && ta.Artist.Name.Contains(normalizedArtist)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedGenre))
        {
            trackQuery = trackQuery.Where(t => t.TrackGenres.Any(tg => tg.Genre != null && tg.Genre.Name.Contains(normalizedGenre)));
        }

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            trackQuery = trackQuery.Where(t =>
                t.Title.Contains(normalized) ||
                t.TrackArtists.Any(ta => ta.Artist != null && ta.Artist.Name.Contains(normalized)) ||
                (t.Album != null && t.Album.Title.Contains(normalized)) ||
                t.TrackGenres.Any(tg => tg.Genre != null && tg.Genre.Name.Contains(normalized)) ||
                (t.Category != null && t.Category.Name.Contains(normalized)));
        }

        if (genreId.HasValue)
        {
            trackQuery = trackQuery.Where(t => t.TrackGenres.Any(tg => tg.GenreId == genreId.Value));
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
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(x => x.Artist)
            .Include(t => t.TrackGenres)
                .ThenInclude(x => x.Genre)
            .Include(t => t.Category);
    }
}
