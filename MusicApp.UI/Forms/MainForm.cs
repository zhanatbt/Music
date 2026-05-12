using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.Domain.Common;
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
    private ComboBox _cmbPlaybackMode;
    public event Action? TrackFinished;

    public MainForm(MusicLibraryService musicLibraryService, UserSessionDto session)
    {
        _presenter = new MainPresenter(this, musicLibraryService, session);

        Text = $"Music App - Пользователь {session.Username}";
        Width = 1280;
        Height = 820;
        StartPosition = FormStartPosition.CenterParent;

        var header = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(12) };

        // 1. Поля фильтров (Лево)
        header.Controls.Add(MkLabel("Название").WithPos(12, 14));
        _txtTitleFilter = new TextBox { Left = 98, Top = 12, Width = 200 };
        header.Controls.Add(_txtTitleFilter);

        header.Controls.Add(MkLabel("Исполнитель").WithPos(310, 14));
        _txtArtistFilter = new TextBox { Left = 410, Top = 12, Width = 200 };
        header.Controls.Add(_txtArtistFilter);

        header.Controls.Add(MkLabel("Альбом").WithPos(12, 48));
        _txtAlbumFilter = new TextBox { Left = 98, Top = 46, Width = 200 };
        header.Controls.Add(_txtAlbumFilter);

        header.Controls.Add(MkLabel("Жанр").WithPos(310, 48));
        _txtGenreFilter = new TextBox { Left = 410, Top = 46, Width = 200 };
        header.Controls.Add(_txtGenreFilter);

        // 2. Кнопки поиска (Центр)
        var btnSearch = new Button { Text = "🔍 Поиск", Left = 630, Top = 12, Width = 110, Height = 30 };
        var btnReset = new Button { Text = "✕ Сброс", Left = 630, Top = 46, Width = 110, Height = 30 };
        header.Controls.AddRange([btnSearch, btnReset]);

        // 3. Управление плейлистом и режимы (Право)
        // Устанавливаем координаты так, чтобы ничего не накладывалось
        var lblMode = MkLabel("Режим:").WithPos(760, 14);
        _cmbPlaybackMode = new ComboBox
        {
            Left = 760,
            Top = 36,
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _cmbPlaybackMode.Items.AddRange(["Сверху вниз", "Снизу вверх", "Рандом"]);
        _cmbPlaybackMode.SelectedIndex = 0;

        var btnPlayPlaylist = new Button
        {
            Text = "▶ Прослушать плейлист",
            Left = 925,
            Top = 12,
            Width = 160,
            Height = 54,
            BackColor = AppleMusicTheme.Accent,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        var btnAddToPlaylist = new Button
        {
            Text = "➕ Добавить в плейлист",
            Left = 1095,
            Top = 12,
            Width = 165,
            Height = 54
        };

        header.Controls.AddRange([lblMode, _cmbPlaybackMode, btnPlayPlaylist, btnAddToPlaylist]);

        // --- ПАНЕЛИ И ГРИДЫ ---
        var leftSplit = new SplitContainer
        {
            Dock = DockStyle.Left,
            Width = 420,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 220
        };
        leftSplit.Panel1.Controls.Add(BuildPlaylistsPanel());
        leftSplit.Panel2.Controls.Add(BuildPlaylistTracksPanel());

        _gridTracks = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            ScrollBars = ScrollBars.Both
        };
        _gridTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Artist",
            HeaderText = "Исполнитель",
            Width = 150
        });
        _gridTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Title",
            HeaderText = "Название",
            Width = 200
        });
        _gridTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Genre",
            HeaderText = "Жанр",
            Width = 120
        });
        _gridTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Album",
            HeaderText = "Альбом",
            Width = 150
        });
        _gridTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Category",
            HeaderText = "Категория",
            Width = 120
        });

        _player = new AxWindowsMediaPlayer();
        ((System.ComponentModel.ISupportInitialize)_player).BeginInit();
        _player.Enabled = true;
        _player.Dock = DockStyle.Fill;
        ((System.ComponentModel.ISupportInitialize)_player).EndInit();

        var playerPanel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(12) };
        _lblNowPlaying = new Label { Dock = DockStyle.Top, Height = 25, ForeColor = Color.White, Text = "Остановлено" };
        playerPanel.Controls.Add(_player);
        playerPanel.Controls.Add(_lblNowPlaying);

        // Добавляем всё на форму
        Controls.Add(_gridTracks);
        Controls.Add(playerPanel);
        Controls.Add(leftSplit);
        Controls.Add(header);

        // --- СОБЫТИЯ ---
        Load += async (_, _) => await _presenter.LoadAsync();
        btnSearch.Click += async (_, _) => await _presenter.SearchAsync();
        btnReset.Click += async (_, _) => {
            _txtTitleFilter.Clear(); _txtArtistFilter.Clear();
            _txtAlbumFilter.Clear(); _txtGenreFilter.Clear();
            await _presenter.SearchAsync();
        };
        btnAddToPlaylist.Click += async (_, _) => await _presenter.AddSelectedTrackToPlaylistAsync();
        btnPlayPlaylist.Click += async (_, _) => await _presenter.StartPlaylistPlaybackAsync();

        // Двойные клики по таблицам
        _gridTracks.CellDoubleClick += async (s, e) => {
            if (e.RowIndex >= 0) await _presenter.PlaySelectedQueueTrackAsync();
        };
        _gridPlaylistTracks.CellDoubleClick += async (s, e) => {
            if (e.RowIndex >= 0) await _presenter.PlaySelectedPlaylistTrackAsync();
        };

        // Инициализация событий плеера (Action? TrackFinished)
        InitializePlaybackEvents();

        // Применяем визуальную тему (она покрасит Label в белый)
        AppleMusicTheme.Apply(this);
    }

    private Control BuildPlaylistsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
        var titleLabel = new Label
        {
            Text = "Плейлисты",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 25
        };
        var hintLabel = new Label
        {
            Text = "Выбери плейлист ниже. Его треки появятся в нижней таблице.",
            Dock = DockStyle.Fill
        };
        topPanel.Controls.Add(hintLabel);
        topPanel.Controls.Add(titleLabel);

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 120, Padding = new Padding(5) };
        _txtNewPlaylist = new TextBox
        {
            Dock = DockStyle.Top,
            PlaceholderText = "Введите название...",
            Margin = new Padding(0, 0, 0, 10)
        };
        var btnCreatePlaylist = new Button
        {
            Text = "➕ Создать плейлист",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = AppleMusicTheme.Accent
        };
        var btnDeletePlaylist = new Button
        {
            Text = "🗑 Удалить выбранный",
            Dock = DockStyle.Top,
            Height = 35,
            Margin = new Padding(0, 5, 0, 0)
        };
        btnCreatePlaylist.Click += async (_, _) => await _presenter.CreatePlaylistAsync();
        btnDeletePlaylist.Click += async (_, _) => await _presenter.DeleteSelectedPlaylistAsync();
        bottomPanel.Controls.Add(btnDeletePlaylist);
        bottomPanel.Controls.Add(new Panel { Height = 5, Dock = DockStyle.Top });
        bottomPanel.Controls.Add(btnCreatePlaylist);
        bottomPanel.Controls.Add(_txtNewPlaylist);

        _gridPlaylists = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
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

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 48 };
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
            Height = 30
        };
        btnRemoveTrack.Click += async (_, _) => await _presenter.RemoveSelectedTrackFromPlaylistAsync();
        topPanel.Controls.Add(btnRemoveTrack);
        topPanel.Controls.Add(titleLabel);

        _gridPlaylistTracks = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = _playlistTracksSource,
            ScrollBars = ScrollBars.Both
        };
        _gridPlaylistTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrackDto.Artist),
            HeaderText = "Исполнитель",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 28
        });
        _gridPlaylistTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrackDto.Title),
            HeaderText = "Название",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 32
        });
        _gridPlaylistTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrackDto.Genre),
            HeaderText = "Жанр",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });
        _gridPlaylistTracks.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrackDto.Category),
            HeaderText = "Категория",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });
        _gridPlaylistTracks.DataBindingComplete += (_, _) => EnsureSelectColumn(_gridPlaylistTracks, "PlaylistSelectColumn");

        panel.Controls.Add(_gridPlaylistTracks);
        panel.Controls.Add(topPanel);
        return panel;
    }

    public string TitleFilter => _txtTitleFilter.Text;
    public string ArtistFilter => _txtArtistFilter.Text;
    public string AlbumFilter => _txtAlbumFilter.Text;
    public string GenreFilter => _txtGenreFilter.Text;

    public int? SelectedTrackId => _gridTracks.CurrentRow?.DataBoundItem is TrackDto track ? track.Id : null;
    public IReadOnlyList<int> SelectedTrackIds => GetCheckedTrackIds(_gridTracks, "TrackSelectColumn");

    public int? SelectedPlaylistId => _gridPlaylists.CurrentRow?.DataBoundItem is PlaylistDto playlist ? playlist.Id : null;

    public int? SelectedPlaylistTrackId => _gridPlaylistTracks.CurrentRow?.DataBoundItem is TrackDto track ? track.Id : null;
    public IReadOnlyList<int> SelectedPlaylistTrackIds => GetCheckedTrackIds(_gridPlaylistTracks, "PlaylistSelectColumn");

    public string NewPlaylistName => _txtNewPlaylist.Text;

    public void SetTracks(IReadOnlyList<TrackDto> tracks)
    {
        _tracksSource.DataSource = tracks.ToList();
        _gridTracks.DataSource = _tracksSource;
        EnsureSelectColumn(_gridTracks, "TrackSelectColumn");
    }

    public void SetPlaylists(IReadOnlyList<PlaylistDto> playlists)
    {
        _suppressPlaylistSelectionChanged = true;
        _playlistsSource.DataSource = playlists.ToList();
        _gridPlaylists.DataSource = _playlistsSource;
        _suppressPlaylistSelectionChanged = false;
    }

    public void SetPlaylistTracks(IReadOnlyList<TrackDto> tracks)
    {
        _playlistTracksSource.DataSource = tracks.ToList();
        _gridPlaylistTracks.DataSource = _playlistTracksSource;
        EnsureSelectColumn(_gridPlaylistTracks, "PlaylistSelectColumn");
    }

    public void AddPlaylist(PlaylistDto playlist)
    {
        var playlists = (_playlistsSource.DataSource as List<PlaylistDto>) ?? new List<PlaylistDto>();
        playlists.Add(playlist);
        SetPlaylists(playlists);
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

    private static IReadOnlyList<int> GetCheckedTrackIds(DataGridView grid, string selectColumnName)
    {
        return grid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => row.DataBoundItem is TrackDto &&
                          row.Cells[selectColumnName].Value is bool isChecked &&
                          isChecked)
            .Select(row => ((TrackDto)row.DataBoundItem).Id)
            .ToList();
    }

    private static void EnsureSelectColumn(DataGridView grid, string selectColumnName)
    {
        if (!grid.Columns.Contains(selectColumnName))
        {
            grid.Columns.Insert(0, new DataGridViewCheckBoxColumn
            {
                Name = selectColumnName,
                HeaderText = "Выбрать",
                Width = 70,
                ReadOnly = false,
                TrueValue = true,
                FalseValue = false
            });
        }

        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (column.Name != selectColumnName)
            {
                column.ReadOnly = true;
            }
        }
    }

    public PlaybackMode CurrentMode => _cmbPlaybackMode.SelectedIndex switch {
        1 => PlaybackMode.Reverse,
        2 => PlaybackMode.Random,
        _ => PlaybackMode.Normal
    };
    
    private void InitializePlaybackEvents()
    {
        _player.PlayStateChange += (s, e) => {
            // 8 = MediaEnded (трек завершен)
            if (e.newState == 8) 
            {
                // Вызываем событие, на которое подписан Презентер
                this.BeginInvoke(() => TrackFinished?.Invoke());
            }
        };
    }

    // Реализация метода интерфейса
    public void PlayTrack(TrackDto track)
    {
        if (string.IsNullOrEmpty(track.PreviewUrl)) return;
        _lblNowPlaying.Text = $"▶ Сейчас играет: {track.Artist} - {track.Title}";
        _player.URL = track.PreviewUrl;
        _player.Ctlcontrols.play();
    }

    private static Label MkLabel(string text) => 
        new Label { Text = text, AutoSize = true, ForeColor = Color.White };
}

file static class LabelExt 
{
    public static Label WithPos(this Label l, int x, int y) { l.Left = x; l.Top = y; return l; }
}
