using MusicApp.Application.DTOs;
using MusicApp.Application.Services;

namespace MusicApp.UI.Presenters;

public class MainPresenter
{
    private readonly IMainView _view;
    private readonly MusicLibraryService _musicLibraryService;
    private readonly UserSessionDto _session;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public MainPresenter(IMainView view, MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _view = view;
        _musicLibraryService = musicLibraryService;
        _session = session;
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
            var result = await _musicLibraryService.CreatePlaylistAsync(_session.UserId, playlistName);
            _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

            if (result.Success && result.Data is not null)
            {
                _view.AddPlaylist(result.Data);
                _view.SelectPlaylistByName(playlistName.Trim());
                _view.ClearNewPlaylistName();
                await ReloadSelectedPlaylistTracksAsync();
            }
        });
    }

    public async Task AddSelectedTrackToPlaylistAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (!_view.SelectedTrackId.HasValue || !_view.SelectedPlaylistId.HasValue)
            {
                _view.ShowMessage("Сначала выберите трек в таблице и плейлист слева.", "Ошибка");
                return;
            }

            var result = await _musicLibraryService.AddTrackToPlaylistAsync(_view.SelectedPlaylistId.Value, _view.SelectedTrackId.Value);
            _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

            if (result.Success)
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
            if (!_view.SelectedPlaylistId.HasValue || !_view.SelectedPlaylistTrackId.HasValue)
            {
                _view.ShowMessage("Сначала выберите плейлист и трек в нижней таблице.", "Ошибка");
                return;
            }

            var result = await _musicLibraryService.RemoveTrackFromPlaylistAsync(
                _view.SelectedPlaylistId.Value,
                _view.SelectedPlaylistTrackId.Value);

            _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

            if (result.Success)
            {
                await ReloadPlaylistsAsync();
                await ReloadSelectedPlaylistTracksAsync();
            }
        });
    }

    public async Task PlayPreviewAsync()
    {
        await ExecuteSerializedAsync(async () =>
        {
            if (!_view.SelectedTrackId.HasValue)
            {
                _view.ShowMessage("Выберите трек для прослушивания.", "Ошибка");
                return;
            }

            var tracks = await _musicLibraryService.SearchTracksAsync(_view.SearchText);
            var selected = tracks.FirstOrDefault(x => x.Id == _view.SelectedTrackId.Value);
            if (selected is null || string.IsNullOrWhiteSpace(selected.PreviewUrl))
            {
                _view.ShowMessage("Для этого трека нет preview-ссылки.", "Информация");
                return;
            }

            _view.PlayPreview(selected.PreviewUrl, $"{selected.Artist} - {selected.Title}");
        });
    }

    private async Task ReloadTracksAsync()
    {
        var tracks = await _musicLibraryService.SearchTracksAsync(_view.SearchText);
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
}
