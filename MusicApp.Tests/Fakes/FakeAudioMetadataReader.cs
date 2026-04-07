using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Tests.Fakes;

public class FakeAudioMetadataReader : IAudioMetadataReader
{
    public Task<AudioMetadataDto> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AudioMetadataDto
        {
            FilePath = filePath,
            Title = "Imported Track",
            ArtistName = "Imported Artist",
            AlbumTitle = "Imported Album",
            GenreName = "Pop",
            DurationSeconds = 120
        });
    }
}
