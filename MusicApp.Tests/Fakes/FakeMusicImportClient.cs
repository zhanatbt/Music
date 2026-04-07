using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Tests.Fakes;

public class FakeMusicImportClient : IMusicImportClient
{
    public Task<IReadOnlyList<DeezerTrackDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<DeezerTrackDto>)Array.Empty<DeezerTrackDto>());
    }
}
