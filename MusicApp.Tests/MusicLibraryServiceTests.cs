using MusicApp.Application.Services;
using MusicApp.Domain.Entities;
using MusicApp.Tests.Fakes;

namespace MusicApp.Tests;

public class MusicLibraryServiceTests
{
    [Fact]
    public async Task AddTrackToPlaylist_Should_Add_Only_Once()
    {
        var trackRepository = new FakeTrackRepository();
        var playlistRepository = new FakePlaylistRepository();

        await trackRepository.AddAsync(new Track { Id = 1, Title = "Mock", Artist = new Artist { Name = "Artist" }, Genre = new Genre { Name = "Pop" } });
        await playlistRepository.AddAsync(new Playlist { Id = 1, Name = "Favourites", UserId = 10 });

        var service = new MusicLibraryService(trackRepository, playlistRepository);

        await service.AddTrackToPlaylistAsync(1, 1);
        await service.AddTrackToPlaylistAsync(1, 1);

        var playlist = await playlistRepository.GetByIdAsync(1);
        Assert.NotNull(playlist);
        Assert.Single(playlist!.PlaylistTracks);
    }
}
