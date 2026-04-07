using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.Domain.Entities;
using MusicApp.Tests.Fakes;

namespace MusicApp.Tests;

public class AdminCatalogServiceTests
{
    [Fact]
    public async Task AddTrack_Should_Not_Add_Duplicate_By_DeezerId()
    {
        var genreRepository = new FakeGenreRepository();
        var categoryRepository = new FakeCategoryRepository();
        var trackRepository = new FakeTrackRepository();
        var artistRepository = new FakeArtistRepository();
        var albumRepository = new FakeAlbumRepository();
        var userRepository = new FakeUserRepository();
        var importClient = new FakeMusicImportClient();
        var audioMetadataReader = new FakeAudioMetadataReader();

        await genreRepository.AddAsync(new Genre { Id = 1, Name = "Pop" });
        await artistRepository.AddAsync(new Artist { Id = 1, Name = "Artist" });
        await trackRepository.AddAsync(new Track
        {
            Id = 1,
            Title = "Song",
            ArtistId = 1,
            GenreId = 1,
            DeezerId = "123"
        });

        var service = new AdminCatalogService(
            genreRepository,
            categoryRepository,
            trackRepository,
            artistRepository,
            albumRepository,
            userRepository,
            importClient,
            audioMetadataReader);

        var result = await service.AddTrackAsync(new TrackCreateDto
        {
            Title = "Song",
            ArtistName = "Artist",
            GenreId = 1,
            DeezerId = "123",
            SourceType = "Deezer"
        });

        Assert.False(result.Success);
        Assert.Equal("Такой трек уже есть в каталоге.", result.Message);
    }
}
