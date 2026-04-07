using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;

namespace MusicApp.UI.Forms;

public class MainForm : Form, IMainView
{
    private readonly MainPresenter _presenter;
    private readonly BindingSource _tracksSource = new();
    private readonly BindingSource _playlistsSource = new();

    private readonly TextBox _txtSearch;
    private readonly TextBox _txtNewPlaylist;
    private readonly DataGridView _gridTracks;
    private readonly ListBox _lstPlaylists;
    private readonly Label _lblNowPlaying;
    private readonly AxWindowsMediaPlayer _player;

    public MainForm(MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _presenter = new MainPresenter(this, musicLibraryService, session);

        Text = $"Music App - Пользователь {session.Username}";
        Width = 1080;
        Height = 640;
        StartPosition = FormStartPosition.CenterParent;

        var header = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12) };
        _txtSearch = new TextBox { Left = 12, Top = 14, Width = 420 };
        var btnSearch = new Button { Text = "Поиск", Left = 440, Top = 12, Width = 110 };
        var btnPlay = new Button { Text = "Слушать preview", Left = 560, Top = 12, Width = 140 };
        var btnAddToPlaylist = new Button { Text = "В плейлист", Left = 710, Top = 12, Width = 120 };
        header.Controls.AddRange([_txtSearch, btnSearch, btnPlay, btnAddToPlaylist]);

        var leftPanel = new Panel { Dock = DockStyle.Left, Width = 250, Padding = new Padding(12) };
        leftPanel.Controls.Add(new Label { Text = "Плейлисты", Dock = DockStyle.Top, Height = 24 });
        _lstPlaylists = new ListBox { Dock = DockStyle.Fill, DisplayMember = "Name", ValueMember = "Id" };
        leftPanel.Controls.Add(_lstPlaylists);

        var playlistBottom = new Panel { Dock = DockStyle.Bottom, Height = 72 };
        _txtNewPlaylist = new TextBox { Dock = DockStyle.Top };
        var btnCreatePlaylist = new Button { Text = "Создать плейлист", Dock = DockStyle.Bottom, Height = 32 };
        playlistBottom.Controls.Add(_txtNewPlaylist);
        playlistBottom.Controls.Add(btnCreatePlaylist);
        leftPanel.Controls.Add(playlistBottom);

        _gridTracks = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false
        };
        _gridTracks.DataSource = _tracksSource;

        _player = new AxWindowsMediaPlayer();
        ((System.ComponentModel.ISupportInitialize)_player).BeginInit();
        _player.Enabled = true;
        _player.Dock = DockStyle.Fill;
        ((System.ComponentModel.ISupportInitialize)_player).EndInit();

        var playerPanel = new Panel { Dock = DockStyle.Bottom, Height = 132, Padding = new Padding(12) };
        _lblNowPlaying = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = "Сейчас играет: ничего не выбрано"
        };

        playerPanel.Controls.Add(_player);
        playerPanel.Controls.Add(_lblNowPlaying);

        Controls.Add(_gridTracks);
        Controls.Add(playerPanel);
        Controls.Add(leftPanel);
        Controls.Add(header);

        Load += async (_, _) => await _presenter.LoadAsync();
        btnSearch.Click += async (_, _) => await _presenter.SearchAsync();
        btnCreatePlaylist.Click += async (_, _) => await _presenter.CreatePlaylistAsync();
        btnAddToPlaylist.Click += async (_, _) => await _presenter.AddSelectedTrackToPlaylistAsync();
        btnPlay.Click += async (_, _) => await _presenter.PlayPreviewAsync();
    }

    public string SearchText => _txtSearch.Text;

    public int? SelectedTrackId
    {
        get
        {
            if (_gridTracks.CurrentRow?.DataBoundItem is TrackDto track)
            {
                return track.Id;
            }

            return null;
        }
    }

    public int? SelectedPlaylistId
    {
        get
        {
            if (_lstPlaylists.SelectedItem is PlaylistDto playlist)
            {
                return playlist.Id;
            }

            return null;
        }
    }

    public string NewPlaylistName => _txtNewPlaylist.Text;

    public void SetTracks(IReadOnlyList<TrackDto> tracks)
    {
        _tracksSource.DataSource = tracks;
    }

    public void SetPlaylists(IReadOnlyList<PlaylistDto> playlists)
    {
        _playlistsSource.DataSource = playlists;
        _lstPlaylists.DataSource = _playlistsSource;
    }

    public void PlayPreview(string previewUrl, string trackTitle)
    {
        _lblNowPlaying.Text = $"Сейчас играет: {trackTitle}";
        _player.URL = previewUrl;
        _player.Ctlcontrols.play();
    }

    public void ShowMessage(string message, string title = "Music App")
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
