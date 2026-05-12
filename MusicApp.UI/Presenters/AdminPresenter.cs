using MusicApp.Application.DTOs;
using MusicApp.Application.Services;

namespace MusicApp.UI.Presenters;

public class AdminPresenter
{
    private readonly IAdminView _view;
    private readonly AdminCatalogService _catalogService;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public AdminPresenter(IAdminView view, AdminCatalogService catalogService)
    {
        _view = view;
        _catalogService = catalogService;
    }

    public async Task LoadAsync()
    {
        await ExecuteSerializedAsync(ReloadAsync);
    }

    public async Task AddGenreAsync()
    {
        var result = await _catalogService.AddGenreAsync(_view.GenreName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
    }

    public async Task AddGenreLookupAsync()
    {
        var result = await _catalogService.AddGenreAsync(_view.NewGenreName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearNewGenreInput();
            await ReloadAsync();
        }
    }

    public async Task DeleteSelectedGenreLookupAsync()
    {
        if (!_view.SelectedGenreLookupId.HasValue)
        {
            _view.ShowMessage("Выберите жанр в таблице.", "Ошибка");
            return;
        }

        var result = await _catalogService.DeleteGenreAsync(_view.SelectedGenreLookupId.Value);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
    }

    public async Task AddCategoryAsync()
    {
        var result = await _catalogService.AddCategoryAsync(_view.CategoryName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
    }

    public async Task AddCategoryLookupAsync()
    {
        var result = await _catalogService.AddCategoryAsync(_view.NewCategoryName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearNewCategoryInput();
            await ReloadAsync();
        }
    }

    public async Task DeleteSelectedCategoryLookupAsync()
    {
        if (!_view.SelectedCategoryLookupId.HasValue)
        {
            _view.ShowMessage("Выберите категорию в таблице.", "Ошибка");
            return;
        }

        var result = await _catalogService.DeleteCategoryAsync(_view.SelectedCategoryLookupId.Value);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
    }

    public async Task AddArtistLookupAsync()
    {
        var result = await _catalogService.AddArtistAsync(_view.NewArtistName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearNewArtistInput();
            await ReloadAsync();
        }
    }

    public async Task DeleteSelectedArtistLookupAsync()
    {
        if (!_view.SelectedArtistLookupId.HasValue)
        {
            _view.ShowMessage("Выберите исполнителя в таблице.", "Ошибка");
            return;
        }

        var result = await _catalogService.DeleteArtistAsync(_view.SelectedArtistLookupId.Value);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
    }

    public async Task ImportAudioFileAsync()
    {
        var filePath = _view.PickAudioFile();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var metadata = await _catalogService.ReadAudioMetadataAsync(filePath);
        _view.ApplyAudioMetadata(metadata);
        _view.TrySelectGenreByName(metadata.GenreName);
    }

    public async Task AddManualTrackAsync()
    {
        var request = new TrackCreateDto
        {
            Title = _view.TrackTitle,
            ArtistName = _view.ArtistName,
            ArtistNames = _view.SelectedArtistNames,
            AlbumTitle = _view.AlbumTitle,
            DurationSeconds = _view.DurationSeconds,
            GenreId = _view.SelectedGenreId ?? 0,
            GenreName = _view.GenreName,
            GenreNames = _view.SelectedGenreNames,
            CategoryId = _view.SelectedCategoryId,
            CategoryName = _view.CategoryName,
            AudioFilePath = _view.ImportedAudioFilePath,
            SourceType = !string.IsNullOrWhiteSpace(_view.ImportedAudioFilePath) ? "LocalFile" : "Manual"
        };

        var result = await _catalogService.AddTrackAsync(request);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearEntryFields();
            await ReloadAsync();
        }
    }

    public void LoadSelectedTrackIntoEditor()
    {
        if (_view.SelectedTrack is null)
        {
            _view.ShowMessage("Выберите трек в таблице каталога.", "Ошибка");
            return;
        }

        _view.LoadTrackIntoEditor(_view.SelectedTrack);
    }

    public async Task UpdateSelectedTrackAsync()
    {
        if (!_view.EditingTrackId.HasValue)
        {
            _view.ShowMessage("Сначала загрузите трек в редактор кнопкой заполнения.", "Ошибка");
            return;
        }

        var request = new TrackCreateDto
        {
            Title = _view.TrackTitle,
            ArtistName = _view.ArtistName,
            ArtistNames = _view.SelectedArtistNames,
            AlbumTitle = _view.AlbumTitle,
            DurationSeconds = _view.DurationSeconds,
            GenreId = _view.SelectedGenreId ?? 0,
            GenreName = _view.GenreName,
            GenreNames = _view.SelectedGenreNames,
            CategoryId = _view.SelectedCategoryId,
            CategoryName = _view.CategoryName,
            AudioFilePath = _view.ImportedAudioFilePath,
            SourceType = !string.IsNullOrWhiteSpace(_view.ImportedAudioFilePath) ? "LocalFile" : "Manual"
        };

        var result = await _catalogService.UpdateTrackAsync(_view.EditingTrackId.Value, request);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearEntryFields();
            await ReloadAsync();
        }
    }

    public async Task DeleteSelectedTrackAsync()
    {
        if (_view.SelectedTrack is null)
        {
            _view.ShowMessage("Выберите трек в таблице каталога.", "Ошибка");
            return;
        }

        var result = await _catalogService.DeleteTrackAsync(_view.SelectedTrack.Id);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            _view.ClearEntryFields();
            await ReloadAsync();
        }
    }

    public async Task SearchTracksAsync()
    {
        var tracks = await _catalogService.SearchTracksAsync(
            album: _view.TrackSearchAlbum,
            genre: _view.TrackSearchGenre,
            title: _view.TrackSearchTitle,
            artist: _view.TrackSearchArtist);
        _view.SetTracks(tracks);
    }

    public async Task UserSelectionChangedAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (_view.SelectedUser is null)
            {
                _view.SetUserPlaylists([]);
                _view.SetSelectedUserPlaylistTracks([]);
                return;
            }

            var playlists = await _catalogService.GetUserPlaylistsAsync(_view.SelectedUser.UserId);
            _view.SetUserPlaylists(playlists);
            _view.SetSelectedUserPlaylistTracks([]);
        });
    }

    public async Task UserPlaylistSelectionChangedAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (_view.SelectedUserPlaylist is null)
            {
                _view.SetSelectedUserPlaylistTracks([]);
                return;
            }

            var tracks = await _catalogService.GetPlaylistTracksAsync(_view.SelectedUserPlaylist.Id);
            _view.SetSelectedUserPlaylistTracks(tracks);
        });
    }

    public Task PlaySelectedTrackAsync()
    {
        if (_view.SelectedTrack is null)
        {
            _view.ShowMessage("Выберите трек в каталоге.", "Ошибка");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_view.SelectedTrack.PreviewUrl))
        {
            _view.ShowMessage("У выбранного трека нет ссылки или файла для воспроизведения.", "Информация");
            return Task.CompletedTask;
        }

        _view.PlayPreview(_view.SelectedTrack.PreviewUrl, $"{_view.SelectedTrack.Artist} - {_view.SelectedTrack.Title}");
        return Task.CompletedTask;
    }


    private async Task ReloadAsync()
    {
        var genres = await _catalogService.GetGenresAsync();
        var artists = await _catalogService.GetArtistsAsync();
        var categories = await _catalogService.GetCategoriesAsync();

        _view.SetGenres(genres);
        _view.SetArtists(artists);
        _view.SetCategoryLookupItems(categories);
        _view.SetGenreLookupItems(genres);
        _view.SetArtistLookupItems(artists);
        _view.SetCategories(categories);
        _view.SetTracks(await _catalogService.GetTracksAsync());
        _view.SetUsers(await _catalogService.GetUsersAsync());
        _view.SetUserPlaylists([]);
        _view.SetSelectedUserPlaylistTracks([]);
    }

    private async Task ExecuteSerializedAsync(Func<Task> action)
    {
        await _operationLock.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _operationLock.Release();
        }
    }
}
