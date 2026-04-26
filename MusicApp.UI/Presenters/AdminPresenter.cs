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

    public async Task AddCategoryAsync()
    {
        var result = await _catalogService.AddCategoryAsync(_view.CategoryName);
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
            AlbumTitle = _view.AlbumTitle,
            DurationSeconds = _view.DurationSeconds,
            GenreId = _view.SelectedGenreId ?? 0,
            GenreName = _view.ImportedGenreName,
            CategoryId = _view.SelectedCategoryId,
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

    public async Task ImportSelectedDeezerTrackAsync()
    {
        if (_view.SelectedDeezerTrack is null)
        {
            _view.ShowMessage("Выберите трек из результатов Deezer.", "Ошибка");
            return;
        }

        if (!_view.SelectedGenreId.HasValue)
        {
            _view.ShowMessage("Для импортируемого трека выберите жанр.", "Ошибка");
            return;
        }

        var request = new TrackCreateDto
        {
            Title = _view.SelectedDeezerTrack.Title,
            ArtistName = _view.SelectedDeezerTrack.ArtistName,
            AlbumTitle = _view.SelectedDeezerTrack.AlbumTitle,
            DurationSeconds = _view.SelectedDeezerTrack.DurationSeconds,
            DeezerId = _view.SelectedDeezerTrack.DeezerId,
            PreviewUrl = _view.SelectedDeezerTrack.PreviewUrl,
            GenreId = _view.SelectedGenreId.Value,
            CategoryId = _view.SelectedCategoryId,
            SourceType = "Deezer"
        };

        var result = await _catalogService.AddTrackAsync(request);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");
        if (result.Success)
        {
            await ReloadAsync();
        }
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
        _view.SetGenres(await _catalogService.GetGenresAsync());
        _view.SetCategories(await _catalogService.GetCategoriesAsync());
        _view.SetTracks(await _catalogService.GetTracksAsync());
        _view.SetUsers(await _catalogService.GetUsersAsync());
    }
}
