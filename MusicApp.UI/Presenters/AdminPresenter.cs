using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Views;

namespace MusicApp.UI.Presenters;

public class AdminPresenter(IAdminView view, AdminCatalogService catalogService)
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task LoadAsync() => await Exec(ReloadAsync);

    public async Task AddGenreLookupAsync()
    {
        var r = await catalogService.AddGenreAsync(view.NewGenreName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewGenreInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task RenameSelectedGenreAsync()
    {
        if (!view.SelectedGenreLookupId.HasValue)
        {
            view.ShowMessage("Выберите жанр.", "Ошибка");
            return;
        }

        var newName = view.NewGenreName;
        if (string.IsNullOrWhiteSpace(newName))
        {
            view.ShowMessage("Введите новое название.", "Ошибка");
            return;
        }

        var r = await catalogService.UpdateGenreAsync(view.SelectedGenreLookupId.Value, newName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewGenreInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task DeleteSelectedGenreLookupAsync()
    {
        if (!view.SelectedGenreLookupId.HasValue)
        {
            view.ShowMessage("Выберите жанр.", "Ошибка");
            return;
        }

        var r = await catalogService.DeleteGenreAsync(view.SelectedGenreLookupId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success) await Exec(ReloadAsync);
    }

    public async Task AddCategoryLookupAsync()
    {
        var r = await catalogService.AddCategoryAsync(view.NewCategoryName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewCategoryInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task RenameSelectedCategoryAsync()
    {
        if (!view.SelectedCategoryLookupId.HasValue)
        {
            view.ShowMessage("Выберите категорию.", "Ошибка");
            return;
        }

        var newName = view.NewCategoryName;
        if (string.IsNullOrWhiteSpace(newName))
        {
            view.ShowMessage("Введите новое название.", "Ошибка");
            return;
        }

        var r = await catalogService.UpdateCategoryAsync(view.SelectedCategoryLookupId.Value, newName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewCategoryInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task DeleteSelectedCategoryLookupAsync()
    {
        if (!view.SelectedCategoryLookupId.HasValue)
        {
            view.ShowMessage("Выберите категорию.", "Ошибка");
            return;
        }

        var r = await catalogService.DeleteCategoryAsync(view.SelectedCategoryLookupId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success) await Exec(ReloadAsync);
    }

    public async Task AddArtistLookupAsync()
    {
        var r = await catalogService.AddArtistAsync(view.NewArtistName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewArtistInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task RenameSelectedArtistAsync()
    {
        if (!view.SelectedArtistLookupId.HasValue)
        {
            view.ShowMessage("Выберите исполнителя.", "Ошибка");
            return;
        }

        var newName = view.NewArtistName;
        if (string.IsNullOrWhiteSpace(newName))
        {
            view.ShowMessage("Введите новое имя.", "Ошибка");
            return;
        }

        var r = await catalogService.UpdateArtistAsync(view.SelectedArtistLookupId.Value, newName);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearNewArtistInput();
            await Exec(ReloadAsync);
        }
    }

    public async Task DeleteSelectedArtistLookupAsync()
    {
        if (!view.SelectedArtistLookupId.HasValue)
        {
            view.ShowMessage("Выберите исполнителя.", "Ошибка");
            return;
        }

        var r = await catalogService.DeleteArtistAsync(view.SelectedArtistLookupId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success) await Exec(ReloadAsync);
    }

    public async Task ImportAudioFileAsync()
    {
        var filePath = view.PickAudioFile();
        if (string.IsNullOrWhiteSpace(filePath)) return;
        var metadata = await catalogService.ReadAudioMetadataAsync(filePath);
        view.ApplyAudioMetadata(metadata);
        view.TrySelectGenreByName(metadata.GenreName);
    }

    public async Task AddManualTrackAsync()
    {
        var request = BuildTrackRequest();
        var r = await catalogService.AddTrackAsync(request);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearEntryFields();
            await Exec(ReloadAsync);
        }
    }

    public void LoadSelectedTrackIntoEditor()
    {
        if (view.SelectedTrack is null)
        {
            view.ShowMessage("Выберите трек.", "Ошибка");
            return;
        }

        view.LoadTrackIntoEditor(view.SelectedTrack);
    }

    public async Task UpdateSelectedTrackAsync()
    {
        if (!view.EditingTrackId.HasValue)
        {
            view.ShowMessage("Сначала загрузите трек в редактор.", "Ошибка");
            return;
        }

        var request = BuildTrackRequest();
        var r = await catalogService.UpdateTrackAsync(view.EditingTrackId.Value, request);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearEntryFields();
            await Exec(ReloadAsync);
        }
    }

    public async Task DeleteSelectedTrackAsync()
    {
        if (view.SelectedTrack is null)
        {
            view.ShowMessage("Выберите трек.", "Ошибка");
            return;
        }

        var r = await catalogService.DeleteTrackAsync(view.SelectedTrack.Id);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearEntryFields();
            await Exec(ReloadAsync);
        }
    }

    public async Task SearchTracksAsync()
    {
        var tracks = await catalogService.SearchTracksAsync(
            album: view.TrackSearchAlbum, genre: view.TrackSearchGenre,
            title: view.TrackSearchTitle, artist: view.TrackSearchArtist);
        view.SetTracks(tracks);
    }

    public Task PlaySelectedTrackAsync()
    {
        if (view.SelectedTrack is null)
        {
            view.ShowMessage("Выберите трек.", "Ошибка");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(view.SelectedTrack.PreviewUrl))
        {
            view.ShowMessage("У трека нет файла для воспроизведения.", "Информация");
            return Task.CompletedTask;
        }

        view.PlayPreview(view.SelectedTrack.PreviewUrl, $"{view.SelectedTrack.Artist} — {view.SelectedTrack.Title}");
        return Task.CompletedTask;
    }

    public async Task UserSelectionChangedAsync() => await Exec(async () =>
    {
        if (view.SelectedUser is null)
        {
            view.SetUserPlaylists([]);
            view.SetSelectedUserPlaylistTracks([]);
            return;
        }

        var playlists = await catalogService.GetUserPlaylistsAsync(view.SelectedUser.UserId);
        view.SetUserPlaylists(playlists);
        view.SetSelectedUserPlaylistTracks([]);
    });

    public async Task UserPlaylistSelectionChangedAsync() => await Exec(async () =>
    {
        if (view.SelectedUserPlaylist is null)
        {
            view.SetSelectedUserPlaylistTracks([]);
            return;
        }

        view.SetSelectedUserPlaylistTracks(await catalogService.GetPlaylistTracksAsync(view.SelectedUserPlaylist.Id));
    });

    public async Task BlockSelectedUserAsync()
    {
        if (view.SelectedUser is null)
        {
            view.ShowMessage("Выберите пользователя.", "Ошибка");
            return;
        }

        var r = await catalogService.BlockUserAsync(view.SelectedUser.UserId);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success) await Exec(ReloadAsync);
    }

    public async Task UnblockSelectedUserAsync()
    {
        if (view.SelectedUser is null)
        {
            view.ShowMessage("Выберите пользователя.", "Ошибка");
            return;
        }

        var r = await catalogService.UnblockUserAsync(view.SelectedUser.UserId);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success) await Exec(ReloadAsync);
    }

    public async Task AlbumSelectionChangedAsync() => await Exec(async () =>
    {
        if (view.SelectedAlbumId is null)
        {
            view.SetAlbumTracks([]);
            return;
        }

        view.SetAlbumTracks(await catalogService.GetAlbumTracksAsync(view.SelectedAlbumId.Value));
    });

    public async Task AddAlbumAsync()
    {
        var r = await catalogService.AddAlbumAsync(view.NewAlbumTitle, view.SelectedAlbumArtistNames);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearAlbumFields();
            await Exec(ReloadAsync);
        }
    }

    public async Task UpdateSelectedAlbumAsync()
    {
        if (!view.EditingAlbumId.HasValue)
        {
            view.ShowMessage("Сначала загрузите альбом в редактор.", "Ошибка");
            return;
        }

        var r = await catalogService.UpdateAlbumAsync(view.EditingAlbumId.Value, view.NewAlbumTitle,
            view.SelectedAlbumArtistNames);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearAlbumFields();
            await Exec(ReloadAsync);
        }
    }

    public async Task DeleteSelectedAlbumAsync()
    {
        if (view.SelectedAlbumId is null)
        {
            view.ShowMessage("Выберите альбом.", "Ошибка");
            return;
        }

        var r = await catalogService.DeleteAlbumAsync(view.SelectedAlbumId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
        {
            view.ClearAlbumFields();
            await Exec(ReloadAsync);
        }
    }

    public void LoadSelectedAlbumIntoEditor()
    {
        if (view.SelectedAlbumId is null)
        {
            view.ShowMessage("Выберите альбом.", "Ошибка");
            return;
        }

        view.LoadAlbumIntoEditor(null!);
    }

    public async Task AddTrackToAlbumAsync()
    {
        if (view.SelectedAlbumId is null)
        {
            view.ShowMessage("Выберите альбом.", "Ошибка");
            return;
        }

        if (view.SelectedTrackForAlbumId is null)
        {
            view.ShowMessage("Выберите трек из каталога.", "Ошибка");
            return;
        }

        var r = await catalogService.AddTrackToAlbumAsync(view.SelectedAlbumId.Value,
            view.SelectedTrackForAlbumId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
            await Exec(async () =>
                view.SetAlbumTracks(await catalogService.GetAlbumTracksAsync(view.SelectedAlbumId!.Value)));
    }

    public async Task RemoveTrackFromAlbumAsync()
    {
        if (view.SelectedAlbumId is null)
        {
            view.ShowMessage("Выберите альбом.", "Ошибка");
            return;
        }

        if (view.SelectedAlbumTrackId is null)
        {
            view.ShowMessage("Выберите трек в таблице альбома.", "Ошибка");
            return;
        }

        var r = await catalogService.RemoveTrackFromAlbumAsync(view.SelectedAlbumId.Value,
            view.SelectedAlbumTrackId.Value);
        view.ShowMessage(r.Message, r.Success ? "Успех" : "Ошибка");
        if (r.Success)
            await Exec(async () =>
                view.SetAlbumTracks(await catalogService.GetAlbumTracksAsync(view.SelectedAlbumId!.Value)));
    }

    private TrackCreateDto BuildTrackRequest() => new()
    {
        Title = view.TrackTitle,
        ArtistName = view.ArtistName,
        ArtistNames = view.SelectedArtistNames,
        AlbumTitle = view.AlbumTitle,
        DurationSeconds = view.DurationSeconds,
        GenreId = view.SelectedGenreId ?? 0,
        GenreName = view.GenreName,
        GenreNames = view.SelectedGenreNames,
        CategoryId = view.SelectedCategoryId,
        CategoryName = view.CategoryName,
        AudioFilePath = view.ImportedAudioFilePath
    };

    private async Task ReloadAsync()
    {
        var genres = await catalogService.GetGenresAsync();
        var artists = await catalogService.GetArtistsAsync();
        var categories = await catalogService.GetCategoriesAsync();
        var albums = await catalogService.GetAlbumsAsync();

        view.SetGenres(genres);
        view.SetArtists(artists);
        view.SetCategories(categories);
        view.SetCategoryLookupItems(categories);
        view.SetGenreLookupItems(genres);
        view.SetArtistLookupItems(artists);
        view.SetAlbumArtists(artists);
        view.SetTracks(await catalogService.GetTracksAsync());
        view.SetUsers(await catalogService.GetUsersAsync());
        view.SetAlbums(albums);
        view.SetAlbumTracks([]);
        view.SetUserPlaylists([]);
        view.SetSelectedUserPlaylistTracks([]);
    }

    private async Task Exec(Func<Task> action)
    {
        await _lock.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _lock.Release();
        }
    }
}