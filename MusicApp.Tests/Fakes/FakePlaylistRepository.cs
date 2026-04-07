using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakePlaylistRepository : IPlaylistRepository
{
    private readonly List<Playlist> _playlists = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Playlist>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Playlist>)_playlists.Where(x => x.UserId == userId).ToList());
    }

    public Task<Playlist?> GetByIdAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_playlists.FirstOrDefault(x => x.Id == playlistId));
    }

    public Task<Playlist?> GetByUserIdAndNameAsync(int userId, string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_playlists.FirstOrDefault(x => x.UserId == userId && x.Name == name));
    }

    public Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        if (playlist.Id == 0)
        {
            playlist.Id = _nextId++;
        }

        _playlists.Add(playlist);
        return Task.CompletedTask;
    }

    public Task AddTrackAsync(int playlistId, int trackId, CancellationToken cancellationToken = default)
    {
        var playlist = _playlists.FirstOrDefault(x => x.Id == playlistId);
        if (playlist is null)
        {
            return Task.CompletedTask;
        }

        var exists = playlist.PlaylistTracks.Any(x => x.TrackId == trackId);
        if (!exists)
        {
            playlist.PlaylistTracks.Add(new PlaylistTrack
            {
                PlaylistId = playlistId,
                TrackId = trackId
            });
        }

        return Task.CompletedTask;
    }
}
