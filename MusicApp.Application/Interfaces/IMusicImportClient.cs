using MusicApp.Application.DTOs;

namespace MusicApp.Application.Interfaces;

public interface IMusicImportClient
{
    Task<IReadOnlyList<DeezerTrackDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
