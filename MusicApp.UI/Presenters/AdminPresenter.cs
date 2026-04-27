using MusicApp.Application.DTOs;
using MusicApp.Application.Services;

namespace MusicApp.UI.Presenters;

public class AdminPresenter
{
    private readonly IAdminView _view;
    private readonly AdminCatalogService _catalogService;

    public AdminPresenter(IAdminView view, AdminCatalogService catalogService)
    {
        _view = view;
        _catalogService = catalogService;
    }

    public async Task LoadAsync()
    {
        await ReloadAsync();
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

    public async Task SearchDeezerAsync()
    {
        var tracks = await _catalogService.SearchDeezerAsync(_view.DeezerQuery);
        _view.SetDeezerResults(tracks);
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

    public async Task ImportDeezerTracksAsync()
    {
        var tracksToImport = _view.SelectedDeezerTracks.ToList();
        if (!tracksToImport.Any() && _view.SelectedDeezerTrack is not null)
        {
            tracksToImport.Add(_view.SelectedDeezerTrack);
        }

        if (!tracksToImport.Any())
        {
            _view.ShowMessage("Выберите трек или отметьте несколько треков для импорта.", "Ошибка");
            return;
        }

        var importResults = new List<string>();
        foreach (var track in tracksToImport)
        {
            try
            {
                var request = CreateDeezerTrackRequest(track);
                var result = await _catalogService.AddTrackAsync(request);
                importResults.Add(result.Success
                    ? $"{track.ArtistName} - {track.Title}: импортировано"
                    : $"{track.ArtistName} - {track.Title}: {result.Message}");
            }
            catch (Exception ex)
            {
                importResults.Add($"{track.ArtistName} - {track.Title}: ошибка {ex.Message}");
            }
        }

        _view.ShowMessage(string.Join(Environment.NewLine, importResults), "Результат импорта");
        await ReloadAsync();
    }

    private TrackCreateDto CreateDeezerTrackRequest(DeezerTrackDto track)
    {
        return new TrackCreateDto
        {
            Title = track.Title,
            ArtistName = track.ArtistName,
            ArtistNames = track.ArtistNames,
            AlbumTitle = track.AlbumTitle,
            DurationSeconds = track.DurationSeconds,
            DeezerId = track.DeezerId,
            PreviewUrl = track.PreviewUrl,
            GenreId = _view.SelectedGenreId ?? 0,
            GenreName = track.GenreName,
            GenreNames = track.GenreNames,
            CategoryId = _view.SelectedCategoryId,
            CategoryName = _view.CategoryName,
            SourceType = "Deezer"
        };
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

    public Task PlaySelectedDeezerTrackAsync()
    {
        if (_view.SelectedDeezerTrack is null)
        {
            _view.ShowMessage("Выберите трек из результатов Deezer.", "Ошибка");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_view.SelectedDeezerTrack.PreviewUrl))
        {
            _view.ShowMessage("У выбранного Deezer-трека нет preview-ссылки.", "Информация");
            return Task.CompletedTask;
        }

        _view.PlayPreview(_view.SelectedDeezerTrack.PreviewUrl, $"{_view.SelectedDeezerTrack.ArtistName} - {_view.SelectedDeezerTrack.Title}");
        return Task.CompletedTask;
    }

    private async Task ReloadAsync()
    {
        var genres = await _catalogService.GetGenresAsync();
        var artists = await _catalogService.GetArtistsAsync();

        _view.SetGenres(genres);
        _view.SetArtists(artists);
        _view.SetGenreLookupItems(genres);
        _view.SetArtistLookupItems(artists);
        _view.SetCategories(await _catalogService.GetCategoriesAsync());
        _view.SetTracks(await _catalogService.GetTracksAsync());
        _view.SetUsers(await _catalogService.GetUsersAsync());
    }
}
