using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.Domain.Common;

namespace MusicApp.UI.Presenters;

public class MainPresenter
{
    private readonly IMainView _view;
    private readonly MusicLibraryService _musicLibraryService;
    private readonly UserSessionDto _session;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private List<TrackDto> _currentQueue = [];
    private int _currentIndex = -1;
    private readonly Random _random = new();

    public MainPresenter(IMainView view, MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _view = view;
        _musicLibraryService = musicLibraryService;
        _session = session;
        _view.TrackFinished += OnTrackFinished;
    }

    public async Task LoadAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            await ReloadTracksAsync();
            await ReloadPlaylistsAsync();
            await ReloadSelectedPlaylistTracksAsync();
        });
    }

    public async Task SearchAsync()
    {
        await ExecuteSerializedAsync(ReloadTracksAsync);
    }

    public async Task PlaylistSelectionChangedAsync()
    {
        await ExecuteSerializedAsync(ReloadSelectedPlaylistTracksAsync);
    }

    public async Task CreatePlaylistAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            var playlistName = _view.NewPlaylistName;

            if (string.IsNullOrWhiteSpace(playlistName))
            {
                _view.ShowMessage("Напишите название плейлиста перед созданием!");
                return;
            }

            var result = await _musicLibraryService.CreatePlaylistAsync(_session.UserId, playlistName);

            if (result.Success && result.Data != null)
            {
                _view.AddPlaylist(result.Data);
                _view.ClearNewPlaylistName();
                await ReloadPlaylistsAsync();
            }
            _view.ShowMessage(result.Message);
        });
    }

    public async Task AddSelectedTrackToPlaylistAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (!_view.SelectedPlaylistId.HasValue)
            {
                _view.ShowMessage("Сначала выберите плейлист слева.", "Ошибка");
                return;
            }

            var trackIds = _view.SelectedTrackIds.ToList();
            if (!trackIds.Any() && _view.SelectedTrackId.HasValue)
            {
                trackIds.Add(_view.SelectedTrackId.Value);
            }

            if (!trackIds.Any())
            {
                _view.ShowMessage("Отметьте треки галочками в таблице справа.", "Ошибка");
                return;
            }

            var successCount = 0;
            var errors = new List<string>();
            foreach (var trackId in trackIds.Distinct())
            {
                var result = await _musicLibraryService.AddTrackToPlaylistAsync(_view.SelectedPlaylistId.Value, trackId);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    errors.Add(result.Message);
                }
            }

            var message = $"Добавлено треков: {successCount} из {trackIds.Distinct().Count()}.";
            if (errors.Count > 0)
            {
                message += $"{Environment.NewLine}Ошибки: {string.Join("; ", errors.Distinct())}";
            }
            _view.ShowMessage(message, errors.Count == 0 ? "Успех" : "Частично выполнено");

            if (successCount > 0)
            {
                await ReloadPlaylistsAsync();
                await ReloadSelectedPlaylistTracksAsync();
            }
        });
    }

    public async Task RemoveSelectedTrackFromPlaylistAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (!_view.SelectedPlaylistId.HasValue)
            {
                _view.ShowMessage("Сначала выберите плейлист.", "Ошибка");
                return;
            }

            var trackIds = _view.SelectedPlaylistTrackIds.ToList();
            if (!trackIds.Any() && _view.SelectedPlaylistTrackId.HasValue)
            {
                trackIds.Add(_view.SelectedPlaylistTrackId.Value);
            }

            if (!trackIds.Any())
            {
                _view.ShowMessage("Отметьте треки галочками в таблице плейлиста.", "Ошибка");
                return;
            }

            var successCount = 0;
            var errors = new List<string>();
            foreach (var trackId in trackIds.Distinct())
            {
                var result = await _musicLibraryService.RemoveTrackFromPlaylistAsync(
                    _view.SelectedPlaylistId.Value,
                    trackId);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    errors.Add(result.Message);
                }
            }

            var message = $"Удалено треков: {successCount} из {trackIds.Distinct().Count()}.";
            if (errors.Count > 0)
            {
                message += $"{Environment.NewLine}Ошибки: {string.Join("; ", errors.Distinct())}";
            }
            _view.ShowMessage(message, errors.Count == 0 ? "Успех" : "Частично выполнено");

            if (successCount > 0)
            {
                await ReloadPlaylistsAsync();
                await ReloadSelectedPlaylistTracksAsync();
            }
        });
    }

    public async Task DeleteSelectedPlaylistAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (!_view.SelectedPlaylistId.HasValue)
            {
                _view.ShowMessage("Сначала выберите плейлист.", "Ошибка");
                return;
            }

            var result = await _musicLibraryService.DeletePlaylistAsync(_view.SelectedPlaylistId.Value);
            _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

            if (result.Success)
            {
                await ReloadPlaylistsAsync();
                _view.SetPlaylistTracks([]);
            }
        });
    }

    public async Task PlaySelectedQueueTrackAsync()
    {
        var tracks = await _musicLibraryService.SearchTracksAsync(
            album: _view.AlbumFilter, genre: _view.GenreFilter,
            title: _view.TitleFilter, artist: _view.ArtistFilter);

        _currentQueue = tracks.ToList();
        _currentIndex = _currentQueue.FindIndex(t => t.Id == _view.SelectedTrackId);

        if (_currentIndex != -1) _view.PlayTrack(_currentQueue[_currentIndex]);
    }

    // Когда кликают на трек внутри ПЛЕЙЛИСТА
    public async Task PlaySelectedPlaylistTrackAsync()
    {
        if (!_view.SelectedPlaylistId.HasValue) return;

        _currentQueue = (await _musicLibraryService.GetPlaylistTracksAsync(_view.SelectedPlaylistId.Value)).ToList();
        _currentIndex = _currentQueue.FindIndex(t => t.Id == _view.SelectedPlaylistTrackId);

        if (_currentIndex != -1) _view.PlayTrack(_currentQueue[_currentIndex]);
    }

    // Когда жмут кнопку "Прослушать плейлист"
    public async Task StartPlaylistPlaybackAsync()
    {
        if (!_view.SelectedPlaylistId.HasValue)
        {
            _view.ShowMessage("Сначала выберите плейлист слева.");
            return;
        }

        _currentQueue = (await _musicLibraryService.GetPlaylistTracksAsync(_view.SelectedPlaylistId.Value)).ToList();
        if (!_currentQueue.Any()) return;

        // Если режим "Снизу вверх", начинаем с конца
        _currentIndex = (_view.CurrentMode == PlaybackMode.Reverse) ? _currentQueue.Count - 1 : 0;

        _view.PlayTrack(_currentQueue[_currentIndex]);
    }

    private async Task ReloadTracksAsync()
    {
        var tracks = await _musicLibraryService.SearchTracksAsync(
            query: null,
            album: _view.AlbumFilter,
            genre: _view.GenreFilter,
            title: _view.TitleFilter,
            artist: _view.ArtistFilter);
        _view.SetTracks(tracks);
    }

    private async Task ReloadPlaylistsAsync()
    {
        var playlists = await _musicLibraryService.GetPlaylistsAsync(_session.UserId);
        _view.SetPlaylists(playlists);
    }

    private async Task ReloadSelectedPlaylistTracksAsync()
    {
        if (!_view.SelectedPlaylistId.HasValue)
        {
            _view.SetPlaylistTracks([]);
            return;
        }

        var tracks = await _musicLibraryService.GetPlaylistTracksAsync(_view.SelectedPlaylistId.Value);
        _view.SetPlaylistTracks(tracks);
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
    
    public void OnTrackFinished()
    {
        if (_currentQueue.Count == 0) return;

        switch (_view.CurrentMode)
        {
            case PlaybackMode.Normal:
                _currentIndex++;
                break;
            case PlaybackMode.Reverse:
                _currentIndex--;
                break;
            case PlaybackMode.Random:
                _currentIndex = _random.Next(0, _currentQueue.Count);
                break;
        }

        if (_currentIndex >= _currentQueue.Count) _currentIndex = 0;
        if (_currentIndex < 0) _currentIndex = _currentQueue.Count - 1;

        var nextTrack = _currentQueue[_currentIndex];
        _view.PlayTrack(nextTrack);
    }
}
