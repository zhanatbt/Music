using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IAlbumRepository
{
    Task<Album?> GetByTitleAndArtistAsync(string title, int artistId, CancellationToken cancellationToken = default);
    Task AddAsync(Album album, CancellationToken cancellationToken = default);
}
