using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IAlbumRepository
{
    Task<IReadOnlyList<Album>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Album?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Album?> GetByTitleAndArtistAsync(string title, int artistId, CancellationToken cancellationToken = default);
    Task AddAsync(Album album, CancellationToken cancellationToken = default);
    Task UpdateAsync(Album album, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddArtistAsync(int albumId, int artistId, CancellationToken cancellationToken = default);
    Task RemoveArtistAsync(int albumId, int artistId, CancellationToken cancellationToken = default);
    Task AddTrackAsync(int albumId, int trackId, CancellationToken ct = default);
    Task RemoveTrackAsync(int albumId, int trackId, CancellationToken ct = default);
}
