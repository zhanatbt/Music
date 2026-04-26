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
    private readonly BindingSource _playlistTracksSource = new();
    private bool _suppressPlaylistSelectionChanged;

    private TextBox _txtTitleFilter = null!;
    private TextBox _txtArtistFilter = null!;
    private TextBox _txtAlbumFilter = null!;
    private TextBox _txtGenreFilter = null!;
    private TextBox _txtNewPlaylist = null!;
    private readonly DataGridView _gridTracks;
    private DataGridView _gridPlaylists = null!;
    private DataGridView _gridPlaylistTracks = null!;
    private readonly Label _lblNowPlaying;
    private readonly AxWindowsMediaPlayer _player;

    public MainForm(MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _presenter = new MainPresenter(this, musicLibraryService, session);

        Text = $"Music App - Пользователь {session.Username}";
        Width = 1280;
        Height = 760;
        StartPosition = FormStartPosition.CenterParent;

        var header = new Panel { Dock = DockStyle.Top, Height = 106, Padding = new Padding(12) };

        var titleLabel = new Label { Text = "Название", Left = 12, Top = 14, Width = 80 };
        _txtTitleFilter = new TextBox { Left = 98, Top = 12, Width = 220 };

        var artistLabel = new Label { Text = "Исполнитель", Left = 330, Top = 14, Width = 90 };
        _txtArtistFilter = new TextBox { Left = 425, Top = 12, Width = 220 };

        var albumLabel = new Label { Text = "Альбом", Left = 12, Top = 48, Width = 80 };
        _txtAlbumFilter = new TextBox { Left = 98, Top = 46, Width = 220 };

        var genreLabel = new Label { Text = "Жанр", Left = 330, Top = 48, Width = 90 };
        _txtGenreFilter = new TextBox { Left = 425, Top = 46, Width = 220 };

        var btnSearch = new Button { Text = "Поиск", Left = 660, Top = 28, Width = 110 };
        var btnReset = new Button { Text = "Сбросить", Left = 780, Top = 28, Width = 110 };
        var btnPlay = new Button { Text = "Слушать preview", Left = 900, Top = 12, Width = 140 };
        var btnAddToPlaylist = new Button { Text = "Добавить в выбранный плейлист", Left = 1050, Top = 12, Width = 220 };

        header.Controls.AddRange([titleLabel, _txtTitleFilter, artistLabel, _txtArtistFilter, albumLabel, _txtAlbumFilter, genreLabel, _txtGenreFilter, btnSearch, btnReset, btnPlay, btnAddToPlaylist]);

        var leftSplit = new SplitContainer
        {
            Dock = DockStyle.Left,
            Width = 420,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300
        };

        leftSplit.Panel1.Padding = new Padding(12, 12, 6, 6);
        leftSplit.Panel2.Padding = new Padding(12, 6, 6, 12);
        leftSplit.Panel1.Controls.Add(BuildPlaylistsPanel());
        leftSplit.Panel2.Controls.Add(BuildPlaylistTracksPanel());

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
        Controls.Add(leftSplit);
        Controls.Add(header);

        Load += async (_, _) => await _presenter.LoadAsync();
        btnSearch.Click += async (_, _) => await _presenter.SearchAsync();
        btnReset.Click += async (_, _) =>
        {
            _txtTitleFilter.Clear();
            _txtArtistFilter.Clear();
            _txtAlbumFilter.Clear();
            _txtGenreFilter.Clear();
            await _presenter.SearchAsync();
        };
        btnAddToPlaylist.Click += async (_, _) => await _presenter.AddSelectedTrackToPlaylistAsync();
        btnPlay.Click += async (_, _) => await _presenter.PlayPreviewAsync();
    }

    private Control BuildPlaylistsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 68 };
        var titleLabel = new Label
        {
            Text = "Плейлисты",
            Dock = DockStyle.Top,
            Height = 24
        };
        var hintLabel = new Label
        {
            Text = "Выбери плейлист ниже. Его треки появятся в нижней таблице.",
            Dock = DockStyle.Fill
        };
        topPanel.Controls.Add(hintLabel);
        topPanel.Controls.Add(titleLabel);

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 72 };
        _txtNewPlaylist = new TextBox { Dock = DockStyle.Top };
        var btnCreatePlaylist = new Button { Text = "Создать плейлист", Dock = DockStyle.Bottom, Height = 32 };
        btnCreatePlaylist.Click += async (_, _) => await _presenter.CreatePlaylistAsync();
        bottomPanel.Controls.Add(_txtNewPlaylist);
        bottomPanel.Controls.Add(btnCreatePlaylist);

        _gridPlaylists = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            DataSource = _playlistsSource
        };
        _gridPlaylists.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PlaylistDto.Name),
            HeaderText = "Плейлист",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _gridPlaylists.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PlaylistDto.TrackCount),
            HeaderText = "Треков",
            Width = 70
        });
        _gridPlaylists.SelectionChanged += GridPlaylists_SelectionChanged;

        panel.Controls.Add(_gridPlaylists);
        panel.Controls.Add(bottomPanel);
        panel.Controls.Add(topPanel);
        return panel;
    }

    private Control BuildPlaylistTracksPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 56 };
        var titleLabel = new Label
        {
            Text = "Треки выбранного плейлиста",
            Dock = DockStyle.Top,
            Height = 24
        };
        var btnRemoveTrack = new Button
        {
            Text = "Удалить из плейлиста",
            Dock = DockStyle.Bottom,
            Height = 28
        };
        btnRemoveTrack.Click += async (_, _) => await _presenter.RemoveSelectedTrackFromPlaylistAsync();
        topPanel.Controls.Add(btnRemoveTrack);
        topPanel.Controls.Add(titleLabel);

        _gridPlaylistTracks = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = _playlistTracksSource
        };

        panel.Controls.Add(_gridPlaylistTracks);
        panel.Controls.Add(topPanel);
        return panel;
    }

    public string TitleFilter => _txtTitleFilter.Text;
    public string ArtistFilter => _txtArtistFilter.Text;
    public string AlbumFilter => _txtAlbumFilter.Text;
    public string GenreFilter => _txtGenreFilter.Text;

    public int? SelectedTrackId => _gridTracks.CurrentRow?.DataBoundItem is TrackDto track ? track.Id : null;

    public int? SelectedPlaylistId => _gridPlaylists.CurrentRow?.DataBoundItem is PlaylistDto playlist ? playlist.Id : null;

    public int? SelectedPlaylistTrackId => _gridPlaylistTracks.CurrentRow?.DataBoundItem is TrackDto track ? track.Id : null;

    public string NewPlaylistName => _txtNewPlaylist.Text;

    public void SetTracks(IReadOnlyList<TrackDto> tracks)
    {
        _tracksSource.DataSource = tracks.ToList();
    }

    public void SetPlaylists(IReadOnlyList<PlaylistDto> playlists)
    {
        _suppressPlaylistSelectionChanged = true;
        _playlistsSource.DataSource = playlists.ToList();
        _suppressPlaylistSelectionChanged = false;
    }

    public void SetPlaylistTracks(IReadOnlyList<TrackDto> tracks)
    {
        _playlistTracksSource.DataSource = tracks.ToList();
    }

    public void AddPlaylist(PlaylistDto playlist)
    {
        _suppressPlaylistSelectionChanged = true;
        var playlists = (_playlistsSource.DataSource as List<PlaylistDto>) ?? [];
        playlists.Add(playlist);
        _playlistsSource.DataSource = null;
        _playlistsSource.DataSource = playlists;
        _suppressPlaylistSelectionChanged = false;
    }

    public void SelectPlaylistByName(string playlistName)
    {
        if (string.IsNullOrWhiteSpace(playlistName))
        {
            return;
        }

        for (var i = 0; i < _gridPlaylists.Rows.Count; i++)
        {
            if (_gridPlaylists.Rows[i].DataBoundItem is PlaylistDto playlist &&
                string.Equals(playlist.Name, playlistName, StringComparison.OrdinalIgnoreCase))
            {
                _gridPlaylists.ClearSelection();
                _gridPlaylists.Rows[i].Selected = true;
                if (_gridPlaylists.Rows[i].Cells.Count > 0)
                {
                    _gridPlaylists.CurrentCell = _gridPlaylists.Rows[i].Cells[0];
                }
                return;
            }
        }
    }

    public void ClearNewPlaylistName()
    {
        _txtNewPlaylist.Clear();
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

    private async void GridPlaylists_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressPlaylistSelectionChanged)
        {
            return;
        }

        await _presenter.PlaylistSelectionChangedAsync();
    }
}
