using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;
using System.ComponentModel;

namespace MusicApp.UI.Forms;

public partial class AdminForm : Form, IAdminView
{
    private AdminPresenter? _presenter;

    // BindingSources
    private readonly BindingSource _categoriesSource = new();
    private readonly BindingSource _tracksSource = new();
    private readonly BindingSource _usersSource = new();
    private readonly BindingSource _userPlaylistsSource = new();
    private readonly BindingSource _userPlaylistTracksSource = new();
    private readonly BindingSource _categoryLookupSource = new();
    private readonly BindingSource _genreLookupSource = new();
    private readonly BindingSource _artistLookupSource = new();

    // Selection state
    private readonly HashSet<string> _selectedGenreNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedArtistNames = new(StringComparer.OrdinalIgnoreCase);

    // Controls — manage tab
    private TextBox _txtTrackTitle = null!;
    private TextBox _txtAlbum = null!;
    private TextBox _txtAudioFilePath = null!;
    private Label _lblDuration = null!; // shows auto-calculated duration
    private int _durationSeconds;
    private CheckedListBox _clbArtists = null!;
    private CheckedListBox _clbGenres = null!;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtArtistSearch = null!;
    private TextBox _txtGenreSearch = null!;

    // Controls — tracks tab
    private TextBox _txtTrackSearchTitle = null!;
    private TextBox _txtTrackSearchArtist = null!;
    private TextBox _txtTrackSearchAlbum = null!;
    private TextBox _txtTrackSearchGenre = null!;
    private DataGridView _gridTracks = null!;

    // Controls — lookup tabs
    private TextBox _txtCategoryLookupSearch = null!;
    private TextBox _txtGenreLookupSearch = null!;
    private TextBox _txtArtistLookupSearch = null!;
    private TextBox _txtNewCategoryName = null!;
    private TextBox _txtNewGenreName = null!;
    private TextBox _txtNewArtistName = null!;
    private DataGridView _gridCategoryLookup = null!;
    private DataGridView _gridGenreLookup = null!;
    private DataGridView _gridArtistLookup = null!;

    // Controls — users tab
    private TextBox _txtUserSearch = null!;
    private DataGridView _gridUsers = null!;
    private DataGridView _gridUserPlaylists = null!;
    private DataGridView _gridUserPlaylistTracks = null!;

    // Player
    private Label _lblNowPlaying = null!;
    private AxWindowsMediaPlayer _player = null!;

    // Misc state
    private int? _editingTrackId;
    private string? _importedGenreName;
    private bool _suppressUserSelectionChanged;
    private bool _suppressUserPlaylistSelectionChanged;

    private List<CategoryDto> _categoryLookupAll = [];
    private List<GenreDto> _genreLookupAll = [];
    private List<ArtistDto> _artistLookupAll = [];
    private List<GenreDto> _allGenres = [];
    private List<ArtistDto> _allArtists = [];
    private List<UserSessionDto> _allUsers = [];

    // ─── IAdminView properties ───────────────────────────────────────────────
    public string GenreName => SelectedGenreNames.FirstOrDefault() ?? _importedGenreName ?? string.Empty;
    public IReadOnlyList<string> SelectedGenreNames => _selectedGenreNames.ToList();
    public IReadOnlyList<string> SelectedArtistNames => _selectedArtistNames.ToList();
    public string CategoryName => _cmbCategory.Text;
    public string TrackTitle => _txtTrackTitle.Text;
    public string ArtistName => SelectedArtistNames.FirstOrDefault() ?? string.Empty;
    public string AlbumTitle => _txtAlbum.Text;
    public int DurationSeconds => _durationSeconds;
    public int? SelectedGenreId => _allGenres.FirstOrDefault(x => _selectedGenreNames.Contains(x.Name))?.Id;
    public int? SelectedCategoryId => (_cmbCategory.SelectedItem as CategoryDto)?.Id;
    public string TrackSearchTitle => _txtTrackSearchTitle.Text;
    public string TrackSearchArtist => _txtTrackSearchArtist.Text;
    public string TrackSearchAlbum => _txtTrackSearchAlbum.Text;
    public string TrackSearchGenre => _txtTrackSearchGenre.Text;
    public string CategoryLookupSearch => _txtCategoryLookupSearch?.Text ?? string.Empty;
    public string GenreLookupSearch => _txtGenreLookupSearch?.Text ?? string.Empty;
    public string ArtistLookupSearch => _txtArtistLookupSearch?.Text ?? string.Empty;
    public string NewCategoryName => _txtNewCategoryName?.Text ?? string.Empty;
    public string NewGenreName => _txtNewGenreName?.Text ?? string.Empty;
    public string NewArtistName => _txtNewArtistName?.Text ?? string.Empty;
    public int? SelectedCategoryLookupId => _gridCategoryLookup?.CurrentRow?.DataBoundItem is CategoryDto c ? c.Id : null;
    public int? SelectedGenreLookupId => _gridGenreLookup?.CurrentRow?.DataBoundItem is GenreDto g ? g.Id : null;
    public int? SelectedArtistLookupId => _gridArtistLookup?.CurrentRow?.DataBoundItem is ArtistDto a ? a.Id : null;
    public int? EditingTrackId => _editingTrackId;
    public string? ImportedAudioFilePath => string.IsNullOrWhiteSpace(_txtAudioFilePath.Text) ? null : _txtAudioFilePath.Text;
    public TrackDto? SelectedTrack => _gridTracks.CurrentRow?.DataBoundItem as TrackDto;
    public UserSessionDto? SelectedUser => _gridUsers?.CurrentRow?.DataBoundItem as UserSessionDto;
    public PlaylistDto? SelectedUserPlaylist => _gridUserPlaylists?.CurrentRow?.DataBoundItem as PlaylistDto;

    // ─── Constructors ────────────────────────────────────────────────────────
    public AdminForm()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            Text = "Music App - Admin Panel";
    }

    public AdminForm(AdminCatalogService catalogService, UserSessionDto session) : this()
    {
        _presenter = new AdminPresenter(this, catalogService);
        Text = $"Music App — Admin ({session.Username})";
        StartPosition = FormStartPosition.CenterParent;

        AppleMusicTheme.Apply(this);

        Load += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.LoadAsync();
        };
    }

    // ─── IAdminView methods ──────────────────────────────────────────────────
    public string? PickAudioFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Аудиофайлы (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac|Все файлы (*.*)|*.*",
            Title = "Выберите аудиофайл"
        };
        return dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
    }

    public void ApplyAudioMetadata(AudioMetadataDto metadata)
    {
        _txtAudioFilePath.Text = metadata.FilePath;
        _txtTrackTitle.Text = metadata.Title;
        _txtArtistSearch.Text = metadata.ArtistName;
        _txtAlbum.Text = metadata.AlbumTitle;
        _importedGenreName = metadata.GenreName;

        _durationSeconds = metadata.DurationSeconds;
        _lblDuration.Text = FormatDuration(_durationSeconds);

        TryCheckArtistByName(metadata.ArtistName);
    }

    public void TrySelectGenreByName(string? genreName)
    {
        _importedGenreName = genreName;
        if (string.IsNullOrWhiteSpace(genreName)) return;
        _txtGenreSearch.Text = genreName;
        _selectedGenreNames.Add(genreName);
        ApplyGenreFilter(_txtGenreSearch.Text);
    }

    public void SetGenres(IReadOnlyList<GenreDto> genres)
    {
        _allGenres = genres.ToList();
        _selectedGenreNames.RemoveWhere(n => _allGenres.All(g => !string.Equals(g.Name, n, StringComparison.OrdinalIgnoreCase)));
        ApplyGenreFilter(_txtGenreSearch?.Text ?? string.Empty);
    }

    public void SetArtists(IReadOnlyList<ArtistDto> artists)
    {
        _allArtists = artists.ToList();
        _selectedArtistNames.RemoveWhere(n => _allArtists.All(a => !string.Equals(a.Name, n, StringComparison.OrdinalIgnoreCase)));
        ApplyArtistFilter(_txtArtistSearch?.Text ?? string.Empty);
    }

    public void SetGenreLookupItems(IReadOnlyList<GenreDto> genres)
    {
        _genreLookupAll = genres.ToList();
        ApplyGenreLookupFilter(_txtGenreLookupSearch?.Text ?? string.Empty);
    }

    public void SetCategoryLookupItems(IReadOnlyList<CategoryDto> categories)
    {
        _categoryLookupAll = categories.ToList();
        ApplyCategoryLookupFilter(_txtCategoryLookupSearch?.Text ?? string.Empty);
    }

    public void SetArtistLookupItems(IReadOnlyList<ArtistDto> artists)
    {
        _artistLookupAll = artists.ToList();
        ApplyArtistLookupFilter(_txtArtistLookupSearch?.Text ?? string.Empty);
    }

    public void SetCategories(IReadOnlyList<CategoryDto> categories)
    {
        _categoriesSource.DataSource = categories;
        _cmbCategory.DataSource = _categoriesSource;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";
    }

    public void SetTracks(IReadOnlyList<TrackDto> tracks)
    {
        _tracksSource.DataSource = tracks;
        if (_gridTracks is not null) _gridTracks.DataSource = _tracksSource;
    }

    public void SetUsers(IReadOnlyList<UserSessionDto> users)
    {
        _allUsers = users.ToList();
        _suppressUserSelectionChanged = true;
        _usersSource.DataSource = _allUsers;
        _suppressUserSelectionChanged = false;
        ApplyUserFilter(_txtUserSearch?.Text ?? string.Empty);
    }

    public void SetUserPlaylists(IReadOnlyList<PlaylistDto> playlists)
    {
        _suppressUserPlaylistSelectionChanged = true;
        _userPlaylistsSource.DataSource = playlists.ToList();
        _suppressUserPlaylistSelectionChanged = false;
    }

    public void SetSelectedUserPlaylistTracks(IReadOnlyList<TrackDto> tracks)
        => _userPlaylistTracksSource.DataSource = tracks.ToList();

    public void ClearNewGenreInput() => _txtNewGenreName?.Clear();
    public void ClearNewCategoryInput() => _txtNewCategoryName?.Clear();
    public void ClearNewArtistInput() => _txtNewArtistName?.Clear();

    public void PlayPreview(string previewUrl, string trackTitle)
    {
        _lblNowPlaying.Text = $"▶  {trackTitle}";
        _player.URL = previewUrl;
        _player.Ctlcontrols.play();
    }

    public void ShowMessage(string message, string title = "Music App")
        => MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void ClearEntryFields()
    {
        _editingTrackId = null;
        _selectedGenreNames.Clear();
        _txtGenreSearch.Clear();
        ApplyGenreFilter(string.Empty);
        _cmbCategory.Text = string.Empty;
        _txtTrackTitle.Clear();
        _selectedArtistNames.Clear();
        _txtArtistSearch.Clear();
        ApplyArtistFilter(string.Empty);
        _txtAlbum.Clear();
        _txtAudioFilePath.Clear();
        _durationSeconds = 0;
        _lblDuration.Text = "— сек";
        _importedGenreName = null;
    }

    public void LoadTrackIntoEditor(TrackDto track)
    {
        _editingTrackId = track.Id;
        _txtTrackTitle.Text = track.Title;
        _txtAlbum.Text = track.Album;
        _durationSeconds = track.DurationSeconds;
        _lblDuration.Text = FormatDuration(_durationSeconds);
        _txtAudioFilePath.Clear();
        _importedGenreName = null;

        SetSelectedArtistsFromText(track.Artist);
        SetSelectedGenresFromText(track.Genre);
        SelectCategoryByName(track.Category);
    }

    // ─── Layout builders ─────────────────────────────────────────────────────
    private Control BuildManageLayout()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 340
        };

        // ── TOP ──
        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };
        top.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        top.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        top.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // fields row
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 3,
            AutoSize = true
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 3; i++) fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // row 0 — audio file
        fields.Controls.Add(MkLabel("Аудиофайл"), 0, 0);
        _txtAudioFilePath = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, MinimumSize = new Size(260, 0) };
        fields.Controls.Add(_txtAudioFilePath, 1, 0);
        fields.SetColumnSpan(_txtAudioFilePath, 2);
        var btnPickAudio = new Button { Text = "Выбрать файл…", AutoSize = true, Anchor = AnchorStyles.Left };
        fields.Controls.Add(btnPickAudio, 3, 0);

        // row 1 — title / album
        fields.Controls.Add(MkLabel("Трек"), 0, 1);
        _txtTrackTitle = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fields.Controls.Add(_txtTrackTitle, 1, 1);
        fields.Controls.Add(MkLabel("Альбом"), 2, 1);
        _txtAlbum = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fields.Controls.Add(_txtAlbum, 3, 1);

        // row 2 — category / duration (auto)
        fields.Controls.Add(MkLabel("Категория"), 0, 2);
        _cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fields.Controls.Add(_cmbCategory, 1, 2);
        fields.Controls.Add(MkLabel("Длительность"), 2, 2);
        _lblDuration = new Label
        {
            Text = "— сек",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = AppleMusicTheme.Accent
        };
        fields.Controls.Add(_lblDuration, 3, 2);

        top.Controls.Add(fields, 0, 0);

        // artists / genres selection
        var selLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            MinimumSize = new Size(0, 170),
            Margin = new Padding(0, 8, 0, 8)
        };
        selLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        selLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        selLayout.Controls.Add(BuildSelectionPanel(
            "Артисты", "Поиск артиста…",
            out _txtArtistSearch, out _clbArtists,
            ApplyArtistFilter, ArtistItemCheckChanged), 0, 0);
        selLayout.Controls.Add(BuildSelectionPanel(
            "Жанры", "Поиск жанра…",
            out _txtGenreSearch, out _clbGenres,
            ApplyGenreFilter, GenreItemCheckChanged), 1, 0);

        top.Controls.Add(selLayout, 0, 1);

        top.Controls.Add(new Label
        {
            Text = "Длительность рассчитывается автоматически из аудиофайла.",
            AutoSize = true,
            ForeColor = AppleMusicTheme.TextSecondary
        }, 0, 2);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true
        };
        var btnSave = new Button { Text = "💾  Сохранить трек", AutoSize = true };
        var btnUpdate = new Button { Text = "✏️  Обновить выбранный", AutoSize = true };
        var btnDelete = new Button { Text = "🗑  Удалить выбранный", AutoSize = true };
        var btnLoad = new Button { Text = "📋  Заполнить из выбранного", AutoSize = true };
        btnDelete.BackColor = Color.FromArgb(180, 40, 40);
        btnPanel.Controls.AddRange([btnSave, btnUpdate, btnDelete, btnLoad]);
        top.Controls.Add(btnPanel, 0, 3);

        root.Panel1.Controls.Add(top);
        root.Panel2.Controls.Add(CreateGrid(_tracksSource));

        btnPickAudio.Click += async (_, _) => { if (_presenter is not null) await _presenter.ImportAudioFileAsync(); };
        btnSave.Click += async (_, _) => { if (_presenter is not null) await _presenter.AddManualTrackAsync(); };
        btnUpdate.Click += async (_, _) => { if (_presenter is not null) await _presenter.UpdateSelectedTrackAsync(); };
        btnDelete.Click += async (_, _) => { if (_presenter is not null) await _presenter.DeleteSelectedTrackAsync(); };
        btnLoad.Click += (_, _) => _presenter?.LoadSelectedTrackIntoEditor();

        return root;
    }

    private Control BuildTracksLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
        searchPanel.Controls.Add(MkLabel("Название").WithPos(0, 10));
        _txtTrackSearchTitle = new TextBox { Left = 0, Top = 32, Width = 240 };
        searchPanel.Controls.Add(MkLabel("Исполнитель").WithPos(260, 10));
        _txtTrackSearchArtist = new TextBox { Left = 260, Top = 32, Width = 240 };
        searchPanel.Controls.Add(MkLabel("Альбом").WithPos(520, 10));
        _txtTrackSearchAlbum = new TextBox { Left = 520, Top = 32, Width = 240 };
        searchPanel.Controls.Add(MkLabel("Жанр").WithPos(780, 10));
        _txtTrackSearchGenre = new TextBox { Left = 780, Top = 32, Width = 180 };
        var btnSearch = new Button { Text = "🔍 Поиск", Left = 970, Top = 32, Width = 110, Height = 30 };
        var btnReset = new Button { Text = "✕ Сбросить", Left = 1090, Top = 32, Width = 110, Height = 30 };
        var btnPlay = new Button { Text = "▶ Слушать", Left = 12, Top = 64, Width = 180, Height = 30 };
        btnReset.BackColor = Color.FromArgb(70, 70, 72);

        searchPanel.Controls.AddRange([_txtTrackSearchTitle, _txtTrackSearchArtist,
            _txtTrackSearchAlbum, _txtTrackSearchGenre, btnSearch, btnReset, btnPlay]);

        _gridTracks = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = _tracksSource
        };

        panel.Controls.Add(_gridTracks);
        panel.Controls.Add(searchPanel);

        btnSearch.Click += async (_, _) => { if (_presenter is not null) await _presenter.SearchTracksAsync(); };
        btnReset.Click += async (_, _) =>
        {
            _txtTrackSearchTitle.Clear(); _txtTrackSearchArtist.Clear();
            _txtTrackSearchAlbum.Clear(); _txtTrackSearchGenre.Clear();
            if (_presenter is not null) await _presenter.SearchTracksAsync();
        };
        btnPlay.Click += async (_, _) => { if (_presenter is not null) await _presenter.PlaySelectedTrackAsync(); };

        return panel;
    }

    private Control BuildUsersLayout()
    {
        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var searchBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        _txtUserSearch = new TextBox { Width = 320, PlaceholderText = "Поиск по логину…" };
        var btnSearch = new Button { Text = "🔍 Поиск", AutoSize = true };
        var btnReset = new Button { Text = "✕", AutoSize = true };
        btnReset.BackColor = Color.FromArgb(70, 70, 72);
        searchBar.Controls.AddRange([_txtUserSearch, btnSearch, btnReset]);
        host.Controls.Add(searchBar, 0, 0);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 360
        };
        _gridUsers = CreateGrid(_usersSource);
        _gridUsers.SelectionChanged += GridUsers_SelectionChanged;
        split.Panel1.Controls.Add(_gridUsers);

        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 260
        };
        _gridUserPlaylists = CreateGrid(_userPlaylistsSource);
        _gridUserPlaylists.SelectionChanged += GridUserPlaylists_SelectionChanged;
        rightSplit.Panel1.Controls.Add(_gridUserPlaylists);
        _gridUserPlaylistTracks = CreateGrid(_userPlaylistTracksSource);
        rightSplit.Panel2.Controls.Add(_gridUserPlaylistTracks);
        split.Panel2.Controls.Add(rightSplit);

        host.Controls.Add(split, 0, 1);

        _txtUserSearch.TextChanged += (_, _) => ApplyUserFilter(_txtUserSearch.Text);
        btnSearch.Click += (_, _) => ApplyUserFilter(_txtUserSearch.Text);
        btnReset.Click += (_, _) => { _txtUserSearch.Clear(); ApplyUserFilter(string.Empty); };

        return host;
    }

    private Control BuildGenreCatalogLayout() => BuildLookupTab("Поиск жанра…", "Новый жанр", out _txtGenreLookupSearch, out _txtNewGenreName, out _gridGenreLookup, _genreLookupSource,
        async () => { if (_presenter is not null) await _presenter.AddGenreLookupAsync(); },
        async () => { if (_presenter is not null) await _presenter.DeleteSelectedGenreLookupAsync(); },
        t => ApplyGenreLookupFilter(t));

    private Control BuildCategoryCatalogLayout() => BuildLookupTab("Поиск категории…", "Новая категория", out _txtCategoryLookupSearch, out _txtNewCategoryName, out _gridCategoryLookup, _categoryLookupSource,
        async () => { if (_presenter is not null) await _presenter.AddCategoryLookupAsync(); },
        async () => { if (_presenter is not null) await _presenter.DeleteSelectedCategoryLookupAsync(); },
        t => ApplyCategoryLookupFilter(t));

    private Control BuildArtistCatalogLayout() => BuildLookupTab("Поиск артиста…", "Новый артист", out _txtArtistLookupSearch, out _txtNewArtistName, out _gridArtistLookup, _artistLookupSource,
        async () => { if (_presenter is not null) await _presenter.AddArtistLookupAsync(); },
        async () => { if (_presenter is not null) await _presenter.DeleteSelectedArtistLookupAsync(); },
        t => ApplyArtistLookupFilter(t));

    private static Control BuildLookupTab(
        string searchPlaceholder, string addPlaceholder,
        out TextBox txtSearch, out TextBox txtNew,
        out DataGridView grid, BindingSource source,
        Func<Task> onAdd, Func<Task> onDelete,
        Action<string> onFilterChanged)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        var search = new TextBox { Left = 12, Top = 12, Width = 360, PlaceholderText = searchPlaceholder };
        search.TextChanged += (_, _) => onFilterChanged(search.Text);

        var newItem = new TextBox { Left = 12, Top = 48, Width = 260, PlaceholderText = addPlaceholder };
        var btnAdd = new Button { Left = 280, Top = 48, Width = 90, Height = 28, Text = "➕ Добавить" };
        var btnDelete = new Button
        {
            Left = 380,
            Top = 48,
            Width = 130,
            Height = 28,
            Text = "🗑 Удалить",
            BackColor = Color.FromArgb(180, 40, 40)
        };

        var dg = new DataGridView
        {
            Left = 12,
            Top = 88,
            Width = 1180,
            Height = 540,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = source
        };

        btnAdd.Click += async (_, _) => await onAdd();
        btnDelete.Click += async (_, _) => await onDelete();

        panel.Controls.AddRange([search, newItem, btnAdd, btnDelete, dg]);
        txtSearch = search;
        txtNew = newItem;
        grid = dg;
        return panel;
    }

    private Control BuildPlayerPanel()
    {
        _player = new AxWindowsMediaPlayer();
        ((System.ComponentModel.ISupportInitialize)_player).BeginInit();
        _player.Enabled = true;
        _player.Dock = DockStyle.Fill;
        ((System.ComponentModel.ISupportInitialize)_player).EndInit();

        _lblNowPlaying = new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            Text = "▶  ничего не выбрано",
            ForeColor = AppleMusicTheme.Accent,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold)
        };

        var pp = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(28, 28, 30)
        };
        pp.Controls.Add(_player);
        pp.Controls.Add(_lblNowPlaying);
        return pp;
    }

    // ─── Filter helpers ──────────────────────────────────────────────────────
    private void ApplyGenreFilter(string filter)
    {
        var src = string.IsNullOrWhiteSpace(filter) ? _allGenres
            : _allGenres.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        RebindCheckedList(_clbGenres, src, _selectedGenreNames, g => g.Name);
    }

    private void ApplyArtistFilter(string filter)
    {
        var src = string.IsNullOrWhiteSpace(filter) ? _allArtists
            : _allArtists.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        RebindCheckedList(_clbArtists, src, _selectedArtistNames, a => a.Name);
    }

    private void ApplyGenreLookupFilter(string filter)
    {
        _genreLookupSource.DataSource = string.IsNullOrWhiteSpace(filter) ? _genreLookupAll
            : _genreLookupAll.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void ApplyCategoryLookupFilter(string filter)
    {
        _categoryLookupSource.DataSource = string.IsNullOrWhiteSpace(filter) ? _categoryLookupAll
            : _categoryLookupAll.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void ApplyArtistLookupFilter(string filter)
    {
        _artistLookupSource.DataSource = string.IsNullOrWhiteSpace(filter) ? _artistLookupAll
            : _artistLookupAll.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void ApplyUserFilter(string filter)
    {
        var filtered = string.IsNullOrWhiteSpace(filter) ? _allUsers
            : _allUsers.Where(x => x.Username.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        _suppressUserSelectionChanged = true;
        _usersSource.DataSource = filtered;
        _suppressUserSelectionChanged = false;
        if (!filtered.Any()) { SetUserPlaylists([]); SetSelectedUserPlaylistTracks([]); }
    }

    // ─── CheckedListBox helpers ──────────────────────────────────────────────
    private void TryCheckArtistByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        for (var i = 0; i < _clbArtists.Items.Count; i++)
        {
            if (_clbArtists.Items[i] is ArtistDto a &&
                string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                _clbArtists.SetItemChecked(i, true);
                _selectedArtistNames.Add(name);
                break;
            }
        }
    }

    private void TryCheckGenreByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        for (var i = 0; i < _clbGenres.Items.Count; i++)
        {
            if (_clbGenres.Items[i] is GenreDto g &&
                string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                _clbGenres.SetItemChecked(i, true);
                _selectedGenreNames.Add(name);
                break;
            }
        }
    }

    private void GenreItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbGenres.Items[e.Index] is GenreDto g)
            UpdateSet(_selectedGenreNames, g.Name, e.NewValue);
    }

    private void ArtistItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbArtists.Items[e.Index] is ArtistDto a)
            UpdateSet(_selectedArtistNames, a.Name, e.NewValue);
    }

    private static void UpdateSet(HashSet<string> set, string name, CheckState state)
    {
        if (state == CheckState.Checked) set.Add(name); else set.Remove(name);
    }

    private static void RebindCheckedList<T>(
        CheckedListBox listBox, IReadOnlyList<T> source,
        HashSet<string> selected, Func<T, string> getName) where T : class
    {
        listBox.BeginUpdate();
        listBox.Items.Clear();
        listBox.DisplayMember = "Name";
        foreach (var item in source)
        {
            var idx = listBox.Items.Add(item);
            var name = getName(item);
            if (!string.IsNullOrWhiteSpace(name) && selected.Contains(name))
                listBox.SetItemChecked(idx, true);
        }
        listBox.EndUpdate();
    }

    private void SetSelectedArtistsFromText(string text)
    {
        _selectedArtistNames.Clear();
        _txtArtistSearch.Clear();
        ApplyArtistFilter(string.Empty);
        foreach (var n in SplitNames(text)) { _selectedArtistNames.Add(n); TryCheckArtistByName(n); }
    }

    private void SetSelectedGenresFromText(string text)
    {
        _selectedGenreNames.Clear();
        _txtGenreSearch.Clear();
        ApplyGenreFilter(string.Empty);
        foreach (var n in SplitNames(text)) { _selectedGenreNames.Add(n); TryCheckGenreByName(n); }
    }

    private void SelectCategoryByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) { _cmbCategory.SelectedIndex = -1; _cmbCategory.Text = string.Empty; return; }
        for (var i = 0; i < _cmbCategory.Items.Count; i++)
        {
            if (_cmbCategory.Items[i] is CategoryDto c &&
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
            { _cmbCategory.SelectedIndex = i; return; }
        }
        _cmbCategory.Text = name;
    }

    // ─── Selection panel builder ─────────────────────────────────────────────
    private GroupBox BuildSelectionPanel(
        string title, string placeholder,
        out TextBox searchBox, out CheckedListBox checkedList,
        Action<string> applyFilter, ItemCheckEventHandler checkHandler)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var sb = new TextBox { Dock = DockStyle.Top, PlaceholderText = placeholder };
        var cl = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, MinimumSize = new Size(0, 120) };

        sb.TextChanged += (_, _) => applyFilter(sb.Text);
        cl.ItemCheck += checkHandler;

        layout.Controls.Add(sb, 0, 0);
        layout.Controls.Add(cl, 0, 1);
        group.Controls.Add(layout);

        searchBox = sb;
        checkedList = cl;
        return group;
    }

    // ─── Grid / event helpers ─────────────────────────────────────────────────
    private static DataGridView CreateGrid(object dataSource) => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AutoGenerateColumns = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AllowUserToAddRows = false,
        DataSource = dataSource
    };

    private async void GridUsers_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressUserSelectionChanged) return;
        if (_presenter is not null) await _presenter.UserSelectionChangedAsync();
    }

    private async void GridUserPlaylists_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressUserPlaylistSelectionChanged) return;
        if (_presenter is not null) await _presenter.UserPlaylistSelectionChangedAsync();
    }

    // ─── Static utils ─────────────────────────────────────────────────────────
    private static string FormatDuration(int seconds)
    {
        if (seconds <= 0) return "— сек";
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.Hours > 0
            ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}  ({seconds} сек)"
            : $"{ts.Minutes}:{ts.Seconds:D2}  ({seconds} сек)";
    }

    private static IEnumerable<string> SplitNames(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split([',', ';', '|', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static Label MkLabel(string text) =>
        new() { Text = text, Anchor = AnchorStyles.Left, AutoSize = true };
}

// tiny extension so we can set position inline
file static class LabelExt
{
    public static Label WithPos(this Label l, int x, int y) { l.Left = x; l.Top = y; return l; }
}