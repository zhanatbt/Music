using MusicApp.Application.DTOs;
using MusicApp.Application.Services;

namespace MusicApp.UI.Presenters;

public class MainPresenter
{
    private readonly IMainView _view;
    private readonly MusicLibraryService _musicLibraryService;
    private readonly UserSessionDto _session;

    public MainPresenter(IMainView view, MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _view = view;
        _musicLibraryService = musicLibraryService;
        _session = session;
    }

    public async Task LoadAsync()
    {
        await ReloadTracksAsync();
        await ReloadPlaylistsAsync();
    }

    public async Task SearchAsync()
    {
        await ReloadTracksAsync();
    }

    public async Task CreatePlaylistAsync()
    {
        var playlistName = _view.NewPlaylistName;
        var result = await _musicLibraryService.CreatePlaylistAsync(_session.UserId, playlistName);
        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

        if (result.Success && result.Data is not null)
        {
            _view.AddPlaylist(result.Data);
            _view.SelectPlaylistByName(playlistName.Trim());
            _view.ClearNewPlaylistName();
        }
    }

    public async Task AddSelectedTrackToPlaylistAsync()
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
        }
    }

    public async Task PlayPreviewAsync()
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
}
