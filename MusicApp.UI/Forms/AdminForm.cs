using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;
using System.ComponentModel;
using MusicApp.UI.Views;

namespace MusicApp.UI.Forms;

public partial class AdminForm : Form, IAdminView
{
    private AdminPresenter? _presenter;

    private readonly BindingSource _categoriesSource = new();
    private readonly BindingSource _tracksSource = new();
    private readonly BindingSource _usersSource = new();
    private readonly BindingSource _userPlaylistsSource = new();
    private readonly BindingSource _userPlaylistTracksSource = new();
    private readonly BindingSource _categoryLookupSource = new();
    private readonly BindingSource _genreLookupSource = new();
    private readonly BindingSource _artistLookupSource = new();
    private readonly BindingSource _albumsSource = new();
    private readonly BindingSource _albumTracksSource = new();
    private readonly BindingSource _albumCatalogTracksSource = new();

    private readonly HashSet<string> _selectedGenreNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedArtistNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedAlbumArtistNames = new(StringComparer.OrdinalIgnoreCase);

    private TextBox _txtTrackTitle = null!;
    private CheckedListBox _clbTrackAlbums = null!;
    private TextBox _txtTrackAlbumSearch = null!;
    private readonly HashSet<int> _selectedTrackAlbumIds = [];
    private TextBox _txtAudioFilePath = null!;
    private Label _lblDuration = null!;
    private int _durationSeconds;
    private CheckedListBox _clbArtists = null!;
    private CheckedListBox _clbGenres = null!;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtArtistSearch = null!;
    private TextBox _txtGenreSearch = null!;

    private ComboBox _cmbTrackSearchTitle = null!;
    private ComboBox _cmbTrackSearchArtist = null!;
    private ComboBox _cmbTrackSearchAlbum = null!;
    private ComboBox _cmbTrackSearchGenre = null!;
    private DataGridView _gridTracks = null!;

    private ComboBox _cmbCategoryLookupSearch = null!;
    private ComboBox _cmbGenreLookupSearch = null!;
    private ComboBox _cmbArtistLookupSearch = null!;
    private TextBox _txtNewCategoryName = null!;
    private TextBox _txtNewGenreName = null!;
    private TextBox _txtNewArtistName = null!;
    private DataGridView _gridCategoryLookup = null!;
    private DataGridView _gridGenreLookup = null!;
    private DataGridView _gridArtistLookup = null!;

    private TextBox _txtUserSearch = null!;
    private DataGridView _gridUsers = null!;
    private DataGridView _gridUserPlaylists = null!;
    private DataGridView _gridUserPlaylistTracks = null!;

    private TextBox _txtNewAlbumTitle = null!;
    private ComboBox _cmbAlbumSearch = null!;
    private CheckedListBox _clbAlbumArtists = null!;
    private TextBox _txtAlbumArtistSearch = null!;
    private DataGridView _gridAlbums = null!;
    private DataGridView _gridAlbumTracks = null!;
    private DataGridView _gridAlbumCatalogTracks = null!;
    private ComboBox _cmbAlbumCatalogSearch = null!;

    private int? _editingTrackId;
    private int? _editingAlbumId;
    private string? _importedGenreName;
    private bool _suppressUserSelectionChanged;
    private bool _suppressUserPlaylistSelectionChanged;
    private bool _suppressAlbumSelectionChanged;

    private List<CategoryDto> _categoryLookupAll = [];
    private List<GenreDto> _genreLookupAll = [];
    private List<ArtistDto> _artistLookupAll = [];
    private List<GenreDto> _allGenres = [];
    private List<ArtistDto> _allArtists = [];
    private List<UserAdminDto> _allUsers = [];
    private List<AlbumDto> _allAlbums = [];
    private List<ArtistDto> _allAlbumArtists = [];
    private List<TrackDto> _allCatalogTracks = [];

    public string GenreName => SelectedGenreNames.FirstOrDefault() ?? _importedGenreName ?? string.Empty;
    public IReadOnlyList<string> SelectedGenreNames => _selectedGenreNames.ToList();
    public IReadOnlyList<string> SelectedArtistNames => _selectedArtistNames.ToList();
    public string CategoryName => _cmbCategory.Text;
    public string TrackTitle => _txtTrackTitle.Text;
    public string ArtistName => SelectedArtistNames.FirstOrDefault() ?? string.Empty;
    public int DurationSeconds => _durationSeconds;
    public int? SelectedGenreId => _allGenres.FirstOrDefault(x => _selectedGenreNames.Contains(x.Name))?.Id;
    public int? SelectedCategoryId => (_cmbCategory.SelectedItem as CategoryDto)?.Id;
    public string TrackSearchTitle => _cmbTrackSearchTitle.Text;
    public string TrackSearchArtist => _cmbTrackSearchArtist.Text;
    public string TrackSearchAlbum => _cmbTrackSearchAlbum.Text;
    public string TrackSearchGenre => _cmbTrackSearchGenre.Text;
    public string CategoryLookupSearch => _cmbCategoryLookupSearch?.Text ?? string.Empty;
    public string GenreLookupSearch => _cmbGenreLookupSearch?.Text ?? string.Empty;
    public string ArtistLookupSearch => _cmbArtistLookupSearch?.Text ?? string.Empty;
    public string NewCategoryName => _txtNewCategoryName?.Text ?? string.Empty;
    public string NewGenreName => _txtNewGenreName?.Text ?? string.Empty;
    public string NewArtistName => _txtNewArtistName?.Text ?? string.Empty;

    public int? SelectedCategoryLookupId =>
        _gridCategoryLookup?.CurrentRow?.DataBoundItem is CategoryDto c ? c.Id : null;

    public int? SelectedGenreLookupId => _gridGenreLookup?.CurrentRow?.DataBoundItem is GenreDto g ? g.Id : null;
    public int? SelectedArtistLookupId => _gridArtistLookup?.CurrentRow?.DataBoundItem is ArtistDto a ? a.Id : null;
    public int? EditingTrackId => _editingTrackId;

    public string? ImportedAudioFilePath =>
        string.IsNullOrWhiteSpace(_txtAudioFilePath.Text) ? null : _txtAudioFilePath.Text;

    public TrackDto? SelectedTrack => _gridTracks?.CurrentRow?.DataBoundItem as TrackDto;
    public UserAdminDto? SelectedUser => _gridUsers?.CurrentRow?.DataBoundItem as UserAdminDto;
    public PlaylistDto? SelectedUserPlaylist => _gridUserPlaylists?.CurrentRow?.DataBoundItem as PlaylistDto;
    public string NewAlbumTitle => _txtNewAlbumTitle?.Text ?? string.Empty;
    public IReadOnlyList<string> SelectedAlbumArtistNames => _selectedAlbumArtistNames.ToList();
    public int? SelectedAlbumId => _gridAlbums?.CurrentRow?.DataBoundItem is AlbumDto al ? al.Id : null;
    public int? SelectedAlbumTrackId => _gridAlbumTracks?.CurrentRow?.DataBoundItem is TrackDto t ? t.Id : null;
    public int? EditingAlbumId => _editingAlbumId;
    public IReadOnlyList<int> SelectedAlbumIds => _selectedTrackAlbumIds.ToList();

    public int? SelectedTrackForAlbumId =>
        _gridAlbumCatalogTracks?.CurrentRow?.DataBoundItem is TrackDto t ? t.Id : null;

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
        _selectedGenreNames.RemoveWhere(n =>
            _allGenres.All(g => !g.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
        ApplyGenreFilter(_txtGenreSearch?.Text ?? string.Empty);
    }

    public void SetArtists(IReadOnlyList<ArtistDto> artists)
    {
        _allArtists = artists.ToList();
        _selectedArtistNames.RemoveWhere(n =>
            _allArtists.All(a => !a.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
        ApplyArtistFilter(_txtArtistSearch?.Text ?? string.Empty);
    }

    public void SetAlbumArtists(IReadOnlyList<ArtistDto> artists)
    {
        _allAlbumArtists = artists.ToList();
        _selectedAlbumArtistNames.RemoveWhere(n =>
            _allAlbumArtists.All(a => !a.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
        ApplyAlbumArtistFilter(_txtAlbumArtistSearch?.Text ?? string.Empty);
    }

    public void SetGenreLookupItems(IReadOnlyList<GenreDto> genres)
    {
        _genreLookupAll = genres.ToList();
        PopulateComboBox(_cmbGenreLookupSearch, genres.Select(g => g.Name).ToList());
        ApplyGenreLookupFilter(_cmbGenreLookupSearch?.Text ?? string.Empty);
    }

    public void SetCategoryLookupItems(IReadOnlyList<CategoryDto> categories)
    {
        _categoryLookupAll = categories.ToList();
        PopulateComboBox(_cmbCategoryLookupSearch, categories.Select(c => c.Name).ToList());
        ApplyCategoryLookupFilter(_cmbCategoryLookupSearch?.Text ?? string.Empty);
    }

    public void SetArtistLookupItems(IReadOnlyList<ArtistDto> artists)
    {
        _artistLookupAll = artists.ToList();
        PopulateComboBox(_cmbArtistLookupSearch, artists.Select(a => a.Name).ToList());
        ApplyArtistLookupFilter(_cmbArtistLookupSearch?.Text ?? string.Empty);
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
        _allCatalogTracks = tracks.ToList();
        _tracksSource.DataSource = tracks;
        if (_gridTracks is not null) _gridTracks.DataSource = _tracksSource;
        PopulateComboBox(_cmbAlbumCatalogSearch,
            tracks.Select(t => t.Title).Distinct().OrderBy(x => x).ToList());
        ApplyAlbumCatalogTrackFilter();
    }

    public void SetUsers(IReadOnlyList<UserAdminDto> users)
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

    public void SetAlbums(IReadOnlyList<AlbumDto> albums)
    {
        var selectedId = SelectedAlbumId;
        _allAlbums = albums.ToList();
        _suppressAlbumSelectionChanged = true;
        PopulateComboBox(_cmbAlbumSearch, albums.Select(a => a.Title).ToList());
        ApplyAlbumFilter(_cmbAlbumSearch?.Text ?? string.Empty);
        if (selectedId.HasValue) RestoreAlbumSelection(selectedId.Value);
        _suppressAlbumSelectionChanged = false;
    }

    public void SetAlbumTracks(IReadOnlyList<TrackDto> tracks)
        => _albumTracksSource.DataSource = tracks.ToList();

    public void ClearNewGenreInput() => _txtNewGenreName?.Clear();
    public void ClearNewCategoryInput() => _txtNewCategoryName?.Clear();
    public void ClearNewArtistInput() => _txtNewArtistName?.Clear();

    public void ClearAlbumFields()
    {
        _editingAlbumId = null;
        _txtNewAlbumTitle?.Clear();
        _selectedAlbumArtistNames.Clear();
        _txtAlbumArtistSearch?.Clear();
        ApplyAlbumArtistFilter(string.Empty);
    }

    public void LoadAlbumIntoEditor(AlbumDto album)
    {
        if (_gridAlbums?.CurrentRow?.DataBoundItem is not AlbumDto current) return;
        _editingAlbumId = current.Id;
        _txtNewAlbumTitle.Text = current.Title;
        _selectedAlbumArtistNames.Clear();
        foreach (var name in SplitNames(current.Artists))
        {
            _selectedAlbumArtistNames.Add(name);
            TryCheckAlbumArtistByName(name);
        }

        ApplyAlbumArtistFilter(_txtAlbumArtistSearch?.Text ?? string.Empty);
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
        _selectedTrackAlbumIds.Clear();
        _txtTrackAlbumSearch?.Clear();
        ApplyTrackAlbumFilter(string.Empty);
        _txtAudioFilePath.Clear();
        _durationSeconds = 0;
        _lblDuration.Text = "— сек";
        _importedGenreName = null;
    }

    public void LoadTrackIntoEditor(TrackDto track)
    {
        _editingTrackId = track.Id;
        _txtTrackTitle.Text = track.Title;
        _selectedTrackAlbumIds.Clear();
        foreach (var id in track.AlbumIds)
            _selectedTrackAlbumIds.Add(id);
        ApplyTrackAlbumFilter(string.Empty);
        _durationSeconds = track.DurationSeconds;
        _lblDuration.Text = FormatDuration(_durationSeconds);
        _txtAudioFilePath.Clear();
        _importedGenreName = null;
        SetSelectedArtistsFromText(track.Artist);
        SetSelectedGenresFromText(track.Genre);
        SelectCategoryByName(track.Category);
    }

    private Control BuildManageLayout()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 340
        };

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

        fields.Controls.Add(MkLabel("Аудиофайл"), 0, 0);
        _txtAudioFilePath = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, MinimumSize = new Size(260, 0) };
        fields.Controls.Add(_txtAudioFilePath, 1, 0);
        fields.SetColumnSpan(_txtAudioFilePath, 2);
        var btnPickAudio = new Button { Text = "Выбрать файл…", AutoSize = true, Anchor = AnchorStyles.Left };
        fields.Controls.Add(btnPickAudio, 3, 0);

        fields.Controls.Add(MkLabel("Трек"), 0, 1);
        _txtTrackTitle = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fields.Controls.Add(_txtTrackTitle, 1, 1);

        fields.Controls.Add(MkLabel("Категория"), 0, 2);
        _cmbCategory = new ComboBox
            { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
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
        selLayout.Controls.Add(
            BuildSelectionPanel("Артисты", "Поиск артиста…", out _txtArtistSearch, out _clbArtists, ApplyArtistFilter,
                ArtistItemCheckChanged), 0, 0);
        selLayout.Controls.Add(
            BuildSelectionPanel("Жанры", "Поиск жанра…", out _txtGenreSearch, out _clbGenres, ApplyGenreFilter,
                GenreItemCheckChanged), 1, 0);
        selLayout.ColumnCount = 3;
        selLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        selLayout.Controls.Add(BuildAlbumSelectionPanel(), 2, 0);
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

        btnPickAudio.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.ImportAudioFileAsync();
        };
        btnSave.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.AddManualTrackAsync();
        };
        btnUpdate.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.UpdateSelectedTrackAsync();
        };
        btnDelete.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.DeleteSelectedTrackAsync();
        };
        btnLoad.Click += (_, _) => _presenter?.LoadSelectedTrackIntoEditor();

        return root;
    }

    private GroupBox BuildAlbumSelectionPanel()
    {
        var group = new GroupBox { Text = "Альбомы (опционально)", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _txtTrackAlbumSearch = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Поиск альбома…" };
        _clbTrackAlbums = new CheckedListBox
            { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, MinimumSize = new Size(0, 120) };
        _txtTrackAlbumSearch.TextChanged += (_, _) => ApplyTrackAlbumFilter(_txtTrackAlbumSearch.Text);
        _clbTrackAlbums.ItemCheck += TrackAlbumItemCheckChanged;
        layout.Controls.Add(_txtTrackAlbumSearch, 0, 0);
        layout.Controls.Add(_clbTrackAlbums, 0, 1);
        group.Controls.Add(layout);
        return group;
    }

    private void ApplyTrackAlbumFilter(string f)
    {
        if (_clbTrackAlbums is null) return;
        var src = string.IsNullOrWhiteSpace(f)
            ? _allAlbums
            : _allAlbums.Where(x => x.Title.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                                    x.Artists.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        _clbTrackAlbums.BeginUpdate();
        _clbTrackAlbums.Items.Clear();
        foreach (var idx in from album in src
                 let idx = _clbTrackAlbums.Items.Add(album)
                 where _selectedTrackAlbumIds.Contains(album.Id)
                 select idx)
        {
            _clbTrackAlbums.SetItemChecked(idx, true);
        }

        _clbTrackAlbums.EndUpdate();
    }

    private void TrackAlbumItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbTrackAlbums.Items[e.Index] is AlbumDto a)
        {
            if (e.NewValue == CheckState.Checked) _selectedTrackAlbumIds.Add(a.Id);
            else _selectedTrackAlbumIds.Remove(a.Id);
        }
    }

    private Control BuildTracksLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 76 };

        searchPanel.Controls.Add(MkLabel("Название").WithPos(0, 10));
        _cmbTrackSearchTitle = new ComboBox { Left = 0, Top = 30, Width = 240, DropDownStyle = ComboBoxStyle.DropDown };
        searchPanel.Controls.Add(_cmbTrackSearchTitle);

        searchPanel.Controls.Add(MkLabel("Исполнитель").WithPos(260, 10));
        _cmbTrackSearchArtist = new ComboBox
            { Left = 260, Top = 30, Width = 240, DropDownStyle = ComboBoxStyle.DropDown };
        searchPanel.Controls.Add(_cmbTrackSearchArtist);

        searchPanel.Controls.Add(MkLabel("Альбом").WithPos(520, 10));
        _cmbTrackSearchAlbum = new ComboBox
            { Left = 520, Top = 30, Width = 240, DropDownStyle = ComboBoxStyle.DropDown };
        searchPanel.Controls.Add(_cmbTrackSearchAlbum);

        searchPanel.Controls.Add(MkLabel("Жанр").WithPos(780, 10));
        _cmbTrackSearchGenre = new ComboBox
            { Left = 780, Top = 30, Width = 180, DropDownStyle = ComboBoxStyle.DropDown };
        searchPanel.Controls.Add(_cmbTrackSearchGenre);

        var btnSearch = new Button { Text = "🔍 Поиск", Left = 970, Top = 30, Width = 110, Height = 30 };
        var btnReset = new Button { Text = "✕ Сбросить", Left = 1090, Top = 30, Width = 110, Height = 30 };
        btnReset.BackColor = Color.FromArgb(70, 70, 72);
        searchPanel.Controls.AddRange([btnSearch, btnReset]);

        _gridTracks = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false, AllowUserToAddRows = false, DataSource = _tracksSource
        };
        panel.Controls.Add(_gridTracks);
        panel.Controls.Add(searchPanel);

        btnSearch.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.SearchTracksAsync();
        };
        btnReset.Click += async (_, _) =>
        {
            _cmbTrackSearchTitle.Text = string.Empty;
            _cmbTrackSearchArtist.Text = string.Empty;
            _cmbTrackSearchAlbum.Text = string.Empty;
            _cmbTrackSearchGenre.Text = string.Empty;
            if (_presenter is not null) await _presenter.SearchTracksAsync();
        };
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

        var topBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        _txtUserSearch = new TextBox { Width = 320, PlaceholderText = "Поиск по логину…" };
        var btnSearch = new Button { Text = "🔍 Поиск", AutoSize = true };
        var btnReset = new Button { Text = "✕", AutoSize = true };
        var btnBlock = new Button { Text = "🔒 Заблокировать", AutoSize = true };
        var btnUnblock = new Button { Text = "🔓 Разблокировать", AutoSize = true };
        btnReset.BackColor = Color.FromArgb(70, 70, 72);
        btnBlock.BackColor = Color.FromArgb(180, 40, 40);
        btnUnblock.BackColor = Color.FromArgb(40, 130, 40);
        topBar.Controls.AddRange([_txtUserSearch, btnSearch, btnReset, btnBlock, btnUnblock]);
        host.Controls.Add(topBar, 0, 0);

        var split = new SplitContainer
            { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 400 };
        _gridUsers = CreateGrid(_usersSource);
        _gridUsers.SelectionChanged += GridUsers_SelectionChanged;
        split.Panel1.Controls.Add(_gridUsers);

        var rightSplit = new SplitContainer
            { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };
        _gridUserPlaylists = CreateGrid(_userPlaylistsSource);
        _gridUserPlaylists.SelectionChanged += GridUserPlaylists_SelectionChanged;
        rightSplit.Panel1.Controls.Add(_gridUserPlaylists);
        _gridUserPlaylistTracks = CreateGrid(_userPlaylistTracksSource);
        rightSplit.Panel2.Controls.Add(_gridUserPlaylistTracks);
        split.Panel2.Controls.Add(rightSplit);
        host.Controls.Add(split, 0, 1);

        _txtUserSearch.TextChanged += (_, _) => ApplyUserFilter(_txtUserSearch.Text);
        btnSearch.Click += (_, _) => ApplyUserFilter(_txtUserSearch.Text);
        btnReset.Click += (_, _) =>
        {
            _txtUserSearch.Clear();
            ApplyUserFilter(string.Empty);
        };
        btnBlock.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.BlockSelectedUserAsync();
        };
        btnUnblock.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.UnblockSelectedUserAsync();
        };
        return host;
    }

    private Control BuildAlbumsLayout()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300
        };

        var topPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8)
        };
        topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var editorRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };
        editorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        editorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        editorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        editorRow.Controls.Add(MkLabel("Название"), 0, 0);
        _txtNewAlbumTitle = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(200, 0) };
        editorRow.Controls.Add(_txtNewAlbumTitle, 1, 0);
        editorRow.Controls.Add(BuildSelectionPanel("Исполнители альбома", "Поиск…",
            out _txtAlbumArtistSearch, out _clbAlbumArtists,
            ApplyAlbumArtistFilter, AlbumArtistItemCheckChanged), 2, 0);

        var btnAlbumSave = new Button { Text = "💾 Сохранить", AutoSize = true, Dock = DockStyle.Top };
        var btnAlbumUpdate = new Button { Text = "✏️ Обновить", AutoSize = true, Dock = DockStyle.Top };
        var btnAlbumDelete = new Button { Text = "🗑 Удалить", AutoSize = true, Dock = DockStyle.Top };
        var btnAlbumLoad = new Button { Text = "📋 Загрузить", AutoSize = true, Dock = DockStyle.Top };
        btnAlbumDelete.BackColor = Color.FromArgb(180, 40, 40);
        var btnBox = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        btnBox.Controls.AddRange([btnAlbumSave, btnAlbumUpdate, btnAlbumDelete, btnAlbumLoad]);
        editorRow.Controls.Add(btnBox, 3, 0);
        topPanel.Controls.Add(editorRow, 0, 0);

        var albumSearchBar = new FlowLayoutPanel
            { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 4) };
        _cmbAlbumSearch = new ComboBox { Width = 360, DropDownStyle = ComboBoxStyle.DropDown };
        _cmbAlbumSearch.TextChanged += (_, _) => ApplyAlbumFilter(_cmbAlbumSearch.Text);
        albumSearchBar.Controls.Add(_cmbAlbumSearch);
        topPanel.Controls.Add(albumSearchBar, 0, 1);

        _gridAlbums = CreateGrid(_albumsSource);
        _gridAlbums.SelectionChanged += GridAlbums_SelectionChanged;
        topPanel.Controls.Add(_gridAlbums, 0, 2);
        root.Panel1.Controls.Add(topPanel);

        var bottomSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 550
        };

        var albumTracksPanel = new Panel { Dock = DockStyle.Fill };

        var btnRemoveFromAlbum = new Button
        {
            Text = "➖ Убрать трек из альбома",
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = Color.FromArgb(180, 40, 40)
        };

        var albumTracksLabel = new Label
        {
            Text = "Треки в альбоме",
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        _gridAlbumTracks = CreateGrid(_albumTracksSource);
        _gridAlbumTracks.Dock = DockStyle.Fill;
        
        albumTracksPanel.Controls.Add(_gridAlbumTracks);
        albumTracksPanel.Controls.Add(albumTracksLabel);
        albumTracksPanel.Controls.Add(btnRemoveFromAlbum);
        
        bottomSplit.Panel1.Controls.Add(albumTracksPanel);

        var pickerPanel = new Panel { Dock = DockStyle.Fill };
        var pickerHeader = new Panel { Dock = DockStyle.Top, Height = 70 };
        pickerHeader.Controls.Add(MkLabel("Добавить трек из каталога:").WithPos(4, 4));
        _cmbAlbumCatalogSearch = new ComboBox { Left = 4, Top = 24, Width = 320, DropDownStyle = ComboBoxStyle.DropDown};
        _cmbAlbumCatalogSearch.TextChanged += (_, _) => ApplyAlbumCatalogTrackFilter();
        var btnAddToAlbum = new Button
            { Text = "➕ Добавить выбранный", Left = 332, Top = 22, Width = 200, Height = 28 };
        pickerHeader.Controls.AddRange([_cmbAlbumCatalogSearch, btnAddToAlbum]);
        _gridAlbumCatalogTracks = CreateGrid(_albumCatalogTracksSource);
        pickerPanel.Controls.Add(_gridAlbumCatalogTracks);
        pickerPanel.Controls.Add(pickerHeader);
        bottomSplit.Panel2.Controls.Add(pickerPanel);
        root.Panel2.Controls.Add(bottomSplit);

        btnAlbumSave.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.AddAlbumAsync();
        };
        btnAlbumUpdate.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.UpdateSelectedAlbumAsync();
        };
        btnAlbumDelete.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.DeleteSelectedAlbumAsync();
        };
        btnAlbumLoad.Click += (_, _) => LoadAlbumIntoEditor(null!);
        btnAddToAlbum.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.AddTrackToAlbumAsync();
        };
        btnRemoveFromAlbum.Click += async (_, _) =>
        {
            if (_presenter is not null) await _presenter.RemoveTrackFromAlbumAsync();
        };
        return root;
    }

    private Control BuildGenreCatalogLayout() => BuildLookupTab(
        "Поиск жанра…", "Новый жанр / новое название",
        out _cmbGenreLookupSearch, out _txtNewGenreName, out _gridGenreLookup, _genreLookupSource,
        async () =>
        {
            if (_presenter is not null) await _presenter.AddGenreLookupAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.RenameSelectedGenreAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.DeleteSelectedGenreLookupAsync();
        },
        t => ApplyGenreLookupFilter(t));

    private Control BuildCategoryCatalogLayout() => BuildLookupTab(
        "Поиск категории…", "Новая категория / новое название",
        out _cmbCategoryLookupSearch, out _txtNewCategoryName, out _gridCategoryLookup, _categoryLookupSource,
        async () =>
        {
            if (_presenter is not null) await _presenter.AddCategoryLookupAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.RenameSelectedCategoryAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.DeleteSelectedCategoryLookupAsync();
        },
        t => ApplyCategoryLookupFilter(t));

    private Control BuildArtistCatalogLayout() => BuildLookupTab(
        "Поиск исполнителя…", "Новый исполнитель / новое имя",
        out _cmbArtistLookupSearch, out _txtNewArtistName, out _gridArtistLookup, _artistLookupSource,
        async () =>
        {
            if (_presenter is not null) await _presenter.AddArtistLookupAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.RenameSelectedArtistAsync();
        },
        async () =>
        {
            if (_presenter is not null) await _presenter.DeleteSelectedArtistLookupAsync();
        },
        t => ApplyArtistLookupFilter(t));

    private static Control BuildLookupTab(
        string searchPlaceholder, string inputPlaceholder,
        out ComboBox cmbSearch, out TextBox txtNew,
        out DataGridView grid, BindingSource source,
        Func<Task> onAdd, Func<Task> onRename, Func<Task> onDelete,
        Action<string> onFilterChanged)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        var search = new ComboBox
        {
            Left = 12, Top = 12, Width = 360,
            DropDownStyle = ComboBoxStyle.DropDown
        };
        search.TextChanged += (_, _) => onFilterChanged(search.Text);

        var newItem = new TextBox { Left = 12, Top = 48, Width = 260, PlaceholderText = inputPlaceholder };
        var btnAdd = new Button { Left = 280, Top = 48, Width = 90, Height = 28, Text = "➕ Добавить" };
        var btnRename = new Button { Left = 378, Top = 48, Width = 120, Height = 28, Text = "✏️ Переименовать" };
        var btnDelete = new Button
        {
            Left = 506, Top = 48, Width = 110, Height = 28,
            Text = "🗑 Удалить",
            BackColor = Color.FromArgb(180, 40, 40)
        };

        btnAdd.Click += async (_, _) => await onAdd();
        btnRename.Click += async (_, _) => await onRename();
        btnDelete.Click += async (_, _) => await onDelete();

        var dg = new DataGridView
        {
            Left = 12, Top = 88,
            Width = 1180, Height = 540,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = source
        };

        panel.Controls.AddRange([search, newItem, btnAdd, btnRename, btnDelete, dg]);
        cmbSearch = search;
        txtNew = newItem;
        grid = dg;
        return panel;
    }

    private void ApplyGenreFilter(string f)
    {
        var src = string.IsNullOrWhiteSpace(f)
            ? _allGenres
            : _allGenres.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        RebindCheckedList(_clbGenres, src, _selectedGenreNames, g => g.Name);
    }

    private void ApplyArtistFilter(string f)
    {
        var src = string.IsNullOrWhiteSpace(f)
            ? _allArtists
            : _allArtists.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        RebindCheckedList(_clbArtists, src, _selectedArtistNames, a => a.Name);
    }

    private void ApplyAlbumArtistFilter(string f)
    {
        if (_clbAlbumArtists is null) return;
        var src = string.IsNullOrWhiteSpace(f)
            ? _allAlbumArtists
            : _allAlbumArtists.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        RebindCheckedList(_clbAlbumArtists, src, _selectedAlbumArtistNames, a => a.Name);
    }

    private void ApplyGenreLookupFilter(string f)
        => _genreLookupSource.DataSource = string.IsNullOrWhiteSpace(f)
            ? _genreLookupAll
            : _genreLookupAll.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();

    private void ApplyCategoryLookupFilter(string f)
        => _categoryLookupSource.DataSource = string.IsNullOrWhiteSpace(f)
            ? _categoryLookupAll
            : _categoryLookupAll.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();

    private void ApplyArtistLookupFilter(string f)
        => _artistLookupSource.DataSource = string.IsNullOrWhiteSpace(f)
            ? _artistLookupAll
            : _artistLookupAll.Where(x => x.Name.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();

    private void ApplyUserFilter(string f)
    {
        var filtered = string.IsNullOrWhiteSpace(f)
            ? _allUsers
            : _allUsers.Where(x => x.Username.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
        _suppressUserSelectionChanged = true;
        _usersSource.DataSource = filtered;
        _suppressUserSelectionChanged = false;
        if (filtered.Count == 0)
        {
            SetUserPlaylists([]);
            SetSelectedUserPlaylistTracks([]);
        }
    }

    private void ApplyAlbumFilter(string f)
        => _albumsSource.DataSource = string.IsNullOrWhiteSpace(f)
            ? _allAlbums
            : _allAlbums.Where(x =>
                x.Title.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                x.Artists.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();

    private void ApplyAlbumCatalogTrackFilter(string? f = null)
    {
        f ??= _cmbAlbumCatalogSearch?.Text ?? string.Empty;

        var albumArtistNames = GetSelectedAlbumArtistNames();
        var filtered = _allCatalogTracks.AsEnumerable();
        if (albumArtistNames.Count > 0)
            filtered = filtered.Where(t => albumArtistNames.Any(an =>
                t.Artist.Contains(an, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(f))
            filtered = filtered.Where(x =>
                x.Title.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                x.Artist.Contains(f, StringComparison.OrdinalIgnoreCase));

        _albumCatalogTracksSource.DataSource = filtered.ToList();
    }

    private void TryCheckArtistByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        for (var i = 0; i < _clbArtists.Items.Count; i++)
            if (_clbArtists.Items[i] is ArtistDto a && a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                _clbArtists.SetItemChecked(i, true);
                _selectedArtistNames.Add(name);
                break;
            }
    }

    private void TryCheckGenreByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        for (var i = 0; i < _clbGenres.Items.Count; i++)
            if (_clbGenres.Items[i] is GenreDto g && g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                _clbGenres.SetItemChecked(i, true);
                _selectedGenreNames.Add(name);
                break;
            }
    }

    private void TryCheckAlbumArtistByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || _clbAlbumArtists is null) return;
        for (var i = 0; i < _clbAlbumArtists.Items.Count; i++)
            if (_clbAlbumArtists.Items[i] is ArtistDto a && a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                _clbAlbumArtists.SetItemChecked(i, true);
                _selectedAlbumArtistNames.Add(name);
                break;
            }
    }

    private void GenreItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbGenres.Items[e.Index] is GenreDto g) UpdateSet(_selectedGenreNames, g.Name, e.NewValue);
    }

    private void ArtistItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbArtists.Items[e.Index] is ArtistDto a) UpdateSet(_selectedArtistNames, a.Name, e.NewValue);
    }

    private void AlbumArtistItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbAlbumArtists.Items[e.Index] is ArtistDto a) UpdateSet(_selectedAlbumArtistNames, a.Name, e.NewValue);
    }

    private static void UpdateSet(HashSet<string> set, string name, CheckState state)
    {
        if (state == CheckState.Checked) set.Add(name);
        else set.Remove(name);
    }

    private static void RebindCheckedList<T>(CheckedListBox lb, IReadOnlyList<T> source,
        HashSet<string> selected, Func<T, string> getName) where T : class
    {
        lb.BeginUpdate();
        lb.Items.Clear();
        lb.DisplayMember = "Name";
        foreach (var item in source)
        {
            var idx = lb.Items.Add(item);
            var name = getName(item);
            if (!string.IsNullOrWhiteSpace(name) && selected.Contains(name))
                lb.SetItemChecked(idx, true);
        }

        lb.EndUpdate();
    }

    private void SetSelectedArtistsFromText(string text)
    {
        _selectedArtistNames.Clear();
        _txtArtistSearch.Clear();
        ApplyArtistFilter(string.Empty);
        foreach (var n in SplitNames(text))
        {
            _selectedArtistNames.Add(n);
            TryCheckArtistByName(n);
        }
    }

    private void SetSelectedGenresFromText(string text)
    {
        _selectedGenreNames.Clear();
        _txtGenreSearch.Clear();
        ApplyGenreFilter(string.Empty);
        foreach (var n in SplitNames(text))
        {
            _selectedGenreNames.Add(n);
            TryCheckGenreByName(n);
        }
    }

    private void SelectCategoryByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _cmbCategory.SelectedIndex = -1;
            _cmbCategory.Text = string.Empty;
            return;
        }

        for (var i = 0; i < _cmbCategory.Items.Count; i++)
            if (_cmbCategory.Items[i] is CategoryDto c && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                _cmbCategory.SelectedIndex = i;
                return;
            }

        _cmbCategory.Text = name;
    }

    private GroupBox BuildSelectionPanel(string title, string placeholder,
        out TextBox searchBox, out CheckedListBox checkedList,
        Action<string> applyFilter, ItemCheckEventHandler checkHandler)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var sb = new TextBox { Dock = DockStyle.Top, PlaceholderText = placeholder };
        var cl = new CheckedListBox
            { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, MinimumSize = new Size(0, 120) };
        sb.TextChanged += (_, _) => applyFilter(sb.Text);
        cl.ItemCheck += checkHandler;
        layout.Controls.Add(sb, 0, 0);
        layout.Controls.Add(cl, 0, 1);
        group.Controls.Add(layout);
        searchBox = sb;
        checkedList = cl;
        return group;
    }

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

    private async void GridAlbums_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressAlbumSelectionChanged) return;
        ApplyAlbumCatalogTrackFilter();
        if (_presenter is not null) await _presenter.AlbumSelectionChangedAsync();
    }

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
        return string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split([',', ';', '|', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static Label MkLabel(string text) => new() { Text = text, Anchor = AnchorStyles.Left, AutoSize = true };
    
    private void RestoreAlbumSelection(int albumId)
    {
        for (var i = 0; i < _gridAlbums.Rows.Count; i++)
        {
            if (_gridAlbums.Rows[i].DataBoundItem is AlbumDto a && a.Id == albumId)
            {
                _gridAlbums.ClearSelection();
                _gridAlbums.Rows[i].Selected = true;
                if (_gridAlbums.Rows[i].Cells.Count > 0)
                    _gridAlbums.CurrentCell = _gridAlbums.Rows[i].Cells[0];
                return;
            }
        }
    }

    private IReadOnlyList<string> GetSelectedAlbumArtistNames()
    {
        if (_gridAlbums?.CurrentRow?.DataBoundItem is not AlbumDto album) return [];
        return album.Artists.Split([',', ';'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static void PopulateComboBox(ComboBox? cmb, IReadOnlyList<string> items)
    {
        if (cmb is null) return;
        var current = cmb.Text;
        cmb.Items.Clear();
        cmb.Items.Add(string.Empty);
        foreach (var item in items) cmb.Items.Add(item);
        cmb.Text = current;
    }
    
    public void PopulateTrackSearchFilters(IReadOnlyList<string> titles, IReadOnlyList<string> artists,
        IReadOnlyList<string> albums, IReadOnlyList<string> genres)
    {
        PopulateComboBox(_cmbTrackSearchTitle, titles);
        PopulateComboBox(_cmbTrackSearchArtist, artists);
        PopulateComboBox(_cmbTrackSearchAlbum, albums);
        PopulateComboBox(_cmbTrackSearchGenre, genres);
    }

    public void PopulateAlbumSearch(IReadOnlyList<string> titles)
        => PopulateComboBox(_cmbAlbumSearch, titles);

    public void PopulateCategorySearch(IReadOnlyList<string> names)
        => PopulateComboBox(_cmbCategoryLookupSearch, names);

    public void PopulateGenreSearch(IReadOnlyList<string> names)
        => PopulateComboBox(_cmbGenreLookupSearch, names);

    public void PopulateArtistSearch(IReadOnlyList<string> names)
        => PopulateComboBox(_cmbArtistLookupSearch, names);
}

file static class LabelExt
{
    public static Label WithPos(this Label l, int x, int y)
    {
        l.Left = x;
        l.Top = y;
        return l;
    }
}