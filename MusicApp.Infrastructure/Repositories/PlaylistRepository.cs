using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _context;

    public PlaylistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Playlist>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Playlists
            .Include(x => x.PlaylistTracks)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Playlist?> GetByIdAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        return await _context.Playlists
            .Include(x => x.PlaylistTracks)
            .FirstOrDefaultAsync(x => x.Id == playlistId, cancellationToken);
    }

    public async Task<Playlist?> GetByUserIdAndNameAsync(int userId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Playlists
            .Include(x => x.PlaylistTracks)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> GetTracksByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        return await _context.PlaylistTracks
            .Where(x => x.PlaylistId == playlistId)
            .Include(x => x.Track)!.ThenInclude(x => x!.Album)
            .Include(x => x.Track)!.ThenInclude(x => x!.TrackArtists).ThenInclude(x => x.Artist)
            .Include(x => x.Track)!.ThenInclude(x => x!.TrackGenres).ThenInclude(x => x.Genre)
            .Include(x => x.Track)!.ThenInclude(x => x!.Category)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => x.Track!)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddTrackAsync(int playlistId, int trackId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.PlaylistTracks
            .AnyAsync(x => x.PlaylistId == playlistId && x.TrackId == trackId, cancellationToken);

        if (!exists)
        {
            _context.PlaylistTracks.Add(new PlaylistTrack
            {
                PlaylistId = playlistId,
                TrackId = trackId
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveTrackAsync(int playlistId, int trackId, CancellationToken cancellationToken = default)
    {
        var playlistTrack = await _context.PlaylistTracks
            .FirstOrDefaultAsync(x => x.PlaylistId == playlistId && x.TrackId == trackId, cancellationToken);

        if (playlistTrack is null)
        {
            return;
        }

        _context.PlaylistTracks.Remove(playlistTrack);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
