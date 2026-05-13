using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class PlaylistRepository(AppDbContext context) : IPlaylistRepository
{
    public async Task<IReadOnlyList<Playlist>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.Playlists.Include(x => x.PlaylistTracks)
            .Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<Playlist?> GetByUserIdAndNameAsync(int userId, string name, CancellationToken ct = default)
        => await context.Playlists.Include(x => x.PlaylistTracks)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name, ct);

    public async Task<Playlist?> GetByIdAsync(int playlistId, CancellationToken ct = default)
        => await context.Playlists.Include(x => x.PlaylistTracks)
            .FirstOrDefaultAsync(x => x.Id == playlistId, ct);

    public async Task<IReadOnlyList<Track>> GetTracksByPlaylistIdAsync(int playlistId, CancellationToken ct = default)
    {
        var trackIds = await context.PlaylistTracks
            .Where(x => x.PlaylistId == playlistId)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => x.TrackId)
            .ToListAsync(ct);

        return await context.Tracks
            .Include(t => t.TrackAlbums).ThenInclude(ta => ta.Album)
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Include(t => t.TrackGenres).ThenInclude(tg => tg.Genre)
            .Include(t => t.Category)
            .Include(t => t.PlaylistTracks)
            .Where(t => trackIds.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Playlist playlist, CancellationToken ct = default)
    {
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Playlist playlist, CancellationToken ct = default)
    {
        context.Playlists.Remove(playlist);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddTrackAsync(int playlistId, int trackId, CancellationToken ct = default)
    {
        context.PlaylistTracks.Add(new PlaylistTrack { PlaylistId = playlistId, TrackId = trackId });
        await context.SaveChangesAsync(ct);
    }

    public async Task RemoveTrackAsync(int playlistId, int trackId, CancellationToken ct = default)
    {
        var item = await context.PlaylistTracks
            .FirstOrDefaultAsync(x => x.PlaylistId == playlistId && x.TrackId == trackId, ct);
        if (item is null) return;
        context.PlaylistTracks.Remove(item);
        await context.SaveChangesAsync(ct);
    }
}