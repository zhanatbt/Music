using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task<IReadOnlyList<Track>> GetAllAsync(CancellationToken ct = default)
        => await IncludeGraph(context.Tracks).OrderBy(t => t.Title).ToListAsync(ct);

    public async Task<Track?> GetByIdAsync(int id, CancellationToken ct = default)
        => await IncludeGraph(context.Tracks).FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Track?> FindDuplicateAsync(string title, int artistId, int? excludeTrackId = null, CancellationToken ct = default)
    {
        var normalized = title.Trim();
        var query = IncludeGraph(context.Tracks).AsQueryable();
        if (excludeTrackId.HasValue) query = query.Where(t => t.Id != excludeTrackId.Value);
        return await query.FirstOrDefaultAsync(
            t => t.Title == normalized && t.TrackArtists.Any(ta => ta.ArtistId == artistId), ct);
    }

    public async Task<IReadOnlyList<Track>> SearchAsync(string? query, int? genreId, int? categoryId,
        string? album, string? genre, string? title, string? artist, CancellationToken ct = default)
    {
        var trackQuery = IncludeGraph(context.Tracks).AsQueryable();
        if (!string.IsNullOrWhiteSpace(title)) trackQuery = trackQuery.Where(t => t.Title.Contains(title.Trim()));
        if (!string.IsNullOrWhiteSpace(album))
            trackQuery = trackQuery.Where(t => t.TrackAlbums.Any(ta => ta.Album != null && ta.Album.Title.Contains(album.Trim())));
        if (!string.IsNullOrWhiteSpace(artist))
            trackQuery = trackQuery.Where(t =>
                t.TrackArtists.Any(ta => ta.Artist != null && ta.Artist.Name.Contains(artist.Trim())));
        if (!string.IsNullOrWhiteSpace(genre))
            trackQuery = trackQuery.Where(t =>
                t.TrackGenres.Any(tg => tg.Genre != null && tg.Genre.Name.Contains(genre.Trim())));
        if (!string.IsNullOrWhiteSpace(query))
        {
            var norm = query.Trim();
            trackQuery = trackQuery.Where(t =>
                t.Title.Contains(norm) ||
                t.TrackArtists.Any(ta => ta.Artist != null && ta.Artist.Name.Contains(norm)) ||
                t.TrackAlbums.Any(ta => ta.Album != null && ta.Album.Title.Contains(norm)) ||
                t.TrackGenres.Any(tg => tg.Genre != null && tg.Genre.Name.Contains(norm)) ||
                (t.Category != null && t.Category.Name.Contains(norm)));
        }

        if (genreId.HasValue) trackQuery = trackQuery.Where(t => t.TrackGenres.Any(tg => tg.GenreId == genreId.Value));
        if (categoryId.HasValue) trackQuery = trackQuery.Where(t => t.CategoryId == categoryId.Value);
        return await trackQuery.OrderBy(t => t.Title).ToListAsync(ct);
    }

    public async Task AddAsync(Track track, CancellationToken ct = default)
    {
        context.Tracks.Add(track);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Track track, CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task DeleteAsync(Track track, CancellationToken ct = default)
    {
        context.Tracks.Remove(track);
        await context.SaveChangesAsync(ct);
    }

    private static IQueryable<Track> IncludeGraph(IQueryable<Track> query)
        => query
            .Include(t => t.TrackAlbums).ThenInclude(ta => ta.Album)
            .Include(t => t.TrackArtists).ThenInclude(x => x.Artist)
            .Include(t => t.TrackGenres).ThenInclude(x => x.Genre)
            .Include(t => t.Category)
            .Include(t => t.PlaylistTracks);
}