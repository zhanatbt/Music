using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Infrastructure.External;

public class TagLibAudioMetadataReader : IAudioMetadataReader
{
    public Task<AudioMetadataDto> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var file = TagLib.File.Create(filePath);
        var title = !string.IsNullOrWhiteSpace(file.Tag.Title)
            ? file.Tag.Title
            : Path.GetFileNameWithoutExtension(filePath);

        return Task.FromResult(new AudioMetadataDto
        {
            FilePath = filePath,
            Title = title,
            ArtistName = file.Tag.FirstPerformer ?? "Unknown Artist",
            AlbumTitle = file.Tag.Album ?? string.Empty,
            GenreName = file.Tag.FirstGenre,
            DurationSeconds = (int)Math.Round(file.Properties.Duration.TotalSeconds)
        });
    }
}
