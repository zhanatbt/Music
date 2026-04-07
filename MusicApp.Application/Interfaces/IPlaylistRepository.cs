using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IPlaylistRepository
{
    Task<IReadOnlyList<Playlist>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Playlist?> GetByIdAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<Playlist?> GetByUserIdAndNameAsync(int userId, string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Track>> GetTracksByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default);
    Task AddAsync(Playlist playlist, CancellationToken cancellationToken = default);
    Task AddTrackAsync(int playlistId, int trackId, CancellationToken cancellationToken = default);
    Task RemoveTrackAsync(int playlistId, int trackId, CancellationToken cancellationToken = default);
}
