using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;
using System.ComponentModel;

namespace MusicApp.UI.Forms;

public partial class AdminForm : Form, IAdminView
{
    private AdminPresenter? _presenter;
    private readonly BindingSource _categoriesSource = new();
    private readonly BindingSource _tracksSource = new();
    private readonly BindingSource _usersSource = new();
    private readonly BindingSource _userPlaylistsSource = new();
    private readonly BindingSource _userPlaylistTracksSource = new();
    private readonly BindingSource _deezerSource = new();
    private readonly BindingSource _categoryLookupSource = new();
    private readonly BindingSource _genreLookupSource = new();
    private readonly BindingSource _artistLookupSource = new();
    private readonly HashSet<string> _selectedGenreNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedArtistNames = new(StringComparer.OrdinalIgnoreCase);

    private TextBox _txtTrackTitle = null!;
    private TextBox _txtAlbum = null!;
    private TextBox _txtAudioFilePath = null!;
    private NumericUpDown _numDuration = null!;
    private CheckedListBox _clbArtists = null!;
    private CheckedListBox _clbGenres = null!;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtArtistSearch = null!;
    private TextBox _txtGenreSearch = null!;
    private TextBox _txtTrackSearchTitle = null!;
    private TextBox _txtTrackSearchArtist = null!;
    private TextBox _txtTrackSearchAlbum = null!;
    private TextBox _txtTrackSearchGenre = null!;
    private TextBox _txtCategoryLookupSearch = null!;
    private TextBox _txtGenreLookupSearch = null!;
    private TextBox _txtArtistLookupSearch = null!;
    private TextBox _txtNewCategoryName = null!;
    private TextBox _txtNewGenreName = null!;
    private TextBox _txtNewArtistName = null!;
    private TextBox _txtDeezerQuery = null!;
    private DataGridView _gridDeezer = null!;
    private DataGridView _gridTracks = null!;
    private DataGridView _gridCategoryLookup = null!;
    private DataGridView _gridGenreLookup = null!;
    private DataGridView _gridArtistLookup = null!;
    private DataGridView _gridUsers = null!;
    private DataGridView _gridUserPlaylists = null!;
    private DataGridView _gridUserPlaylistTracks = null!;
    private Label _lblNowPlaying = null!;
    private AxWindowsMediaPlayer _player = null!;
    private readonly List<DeezerTrackDto> _deezerResults = new();
    private List<CategoryDto> _categoryLookupAll = [];
    private List<GenreDto> _genreLookupAll = [];
    private List<ArtistDto> _artistLookupAll = [];
    private List<GenreDto> _allGenres = [];
    private List<ArtistDto> _allArtists = [];
    private int? _editingTrackId;
    private string? _importedGenreName;
    private bool _suppressUserSelectionChanged;
    private bool _suppressUserPlaylistSelectionChanged;

    public AdminForm()
    {
        InitializeComponent();

        if (IsInDesigner())
        {
            Text = "Music App - Admin Panel";
        }
    }

    public AdminForm(AdminCatalogService catalogService, UserSessionDto session)
        : this()
    {
        _presenter = new AdminPresenter(this, catalogService);

        Text = $"Music App - Admin Panel ({session.Username})";
        StartPosition = FormStartPosition.CenterParent;

        Load += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.LoadAsync();
            }
        };
    }

    public string GenreName => SelectedGenreNames.FirstOrDefault() ?? _importedGenreName ?? string.Empty;
    public IReadOnlyList<string> SelectedGenreNames
    {
        get => _selectedGenreNames.ToList();
    }
    public string CategoryName => _cmbCategory.Text;
    public string TrackTitle => _txtTrackTitle.Text;
    public string ArtistName => SelectedArtistNames.FirstOrDefault() ?? string.Empty;
    public IReadOnlyList<string> SelectedArtistNames
    {
        get => _selectedArtistNames.ToList();
    }
    public string AlbumTitle => _txtAlbum.Text;
    public int DurationSeconds => (int)_numDuration.Value;
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
    public int? SelectedCategoryLookupId => _gridCategoryLookup?.CurrentRow?.DataBoundItem is CategoryDto category ? category.Id : null;
    public int? SelectedGenreLookupId => _gridGenreLookup?.CurrentRow?.DataBoundItem is GenreDto genre ? genre.Id : null;
    public int? SelectedArtistLookupId => _gridArtistLookup?.CurrentRow?.DataBoundItem is ArtistDto artist ? artist.Id : null;
    public int? EditingTrackId => _editingTrackId;
    public string DeezerQuery => _txtDeezerQuery.Text;
    public string? ImportedAudioFilePath => string.IsNullOrWhiteSpace(_txtAudioFilePath.Text) ? null : _txtAudioFilePath.Text;
    public string? ImportedGenreName => _importedGenreName;
    public TrackDto? SelectedTrack => _gridTracks.CurrentRow?.DataBoundItem as TrackDto;
    public UserSessionDto? SelectedUser => _gridUsers?.CurrentRow?.DataBoundItem as UserSessionDto;
    public PlaylistDto? SelectedUserPlaylist => _gridUserPlaylists?.CurrentRow?.DataBoundItem as PlaylistDto;
    public DeezerTrackDto? SelectedDeezerTrack => _gridDeezer.CurrentRow?.DataBoundItem as DeezerTrackDto;
    public IReadOnlyList<DeezerTrackDto> SelectedDeezerTracks =>
        _gridDeezer.Rows
            .Cast<DataGridViewRow>()
            .Where(row => row.Cells["SelectColumn"].Value is bool isChecked && isChecked)
            .Select(row => row.DataBoundItem as DeezerTrackDto)
            .Where(track => track is not null)
            .Cast<DeezerTrackDto>()
            .ToList();
    public IReadOnlyList<DeezerTrackDto> AllDeezerTracks => _deezerResults;

    public string? PickAudioFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "MP3 files (*.mp3)|*.mp3|Audio files (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac|All files (*.*)|*.*",
            Title = "Выберите аудиофайл"
        };

        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }

    public void ApplyAudioMetadata(AudioMetadataDto metadata)
    {
        _txtAudioFilePath.Text = metadata.FilePath;
        _txtTrackTitle.Text = metadata.Title;
        _txtArtistSearch.Text = metadata.ArtistName;
        _txtAlbum.Text = metadata.AlbumTitle;
        _numDuration.Value = Math.Min(_numDuration.Maximum, Math.Max(_numDuration.Minimum, metadata.DurationSeconds));
        _importedGenreName = metadata.GenreName;
        TryCheckArtistByName(metadata.ArtistName);
    }

    public void TrySelectGenreByName(string? genreName)
    {
        _importedGenreName = genreName;
        if (string.IsNullOrWhiteSpace(genreName))
        {
            return;
        }

        _txtGenreSearch.Text = genreName;
        _selectedGenreNames.Add(genreName);
        ApplyGenreFilter(_txtGenreSearch.Text);
    }

    public void SetGenres(IReadOnlyList<GenreDto> genres)
    {
        _allGenres = genres.ToList();
        _selectedGenreNames.RemoveWhere(name => _allGenres.All(genre => !string.Equals(genre.Name, name, StringComparison.OrdinalIgnoreCase)));
        ApplyGenreFilter(_txtGenreSearch?.Text ?? string.Empty);
    }

    public void SetArtists(IReadOnlyList<ArtistDto> artists)
    {
        _allArtists = artists.ToList();
        _selectedArtistNames.RemoveWhere(name => _allArtists.All(artist => !string.Equals(artist.Name, name, StringComparison.OrdinalIgnoreCase)));
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
        if (_gridTracks is not null)
        {
            _gridTracks.DataSource = _tracksSource;
        }
    }

    public void SetUsers(IReadOnlyList<UserSessionDto> users)
    {
        _suppressUserSelectionChanged = true;
        _usersSource.DataSource = users;
        _suppressUserSelectionChanged = false;
    }

    public void SetUserPlaylists(IReadOnlyList<PlaylistDto> playlists)
    {
        _suppressUserPlaylistSelectionChanged = true;
        _userPlaylistsSource.DataSource = playlists.ToList();
        _suppressUserPlaylistSelectionChanged = false;
    }

    public void SetSelectedUserPlaylistTracks(IReadOnlyList<TrackDto> tracks)
    {
        _userPlaylistTracksSource.DataSource = tracks.ToList();
    }

    public void SetDeezerResults(IReadOnlyList<DeezerTrackDto> tracks)
    {
        _deezerResults.Clear();
        _deezerResults.AddRange(tracks);
        _deezerSource.DataSource = _deezerResults.ToList();
        _gridDeezer.DataSource = _deezerSource;
    }

    public void ClearNewGenreInput()
    {
        if (_txtNewGenreName is not null)
        {
            _txtNewGenreName.Clear();
        }
    }

    public void ClearNewCategoryInput()
    {
        if (_txtNewCategoryName is not null)
        {
            _txtNewCategoryName.Clear();
        }
    }

    public void ClearNewArtistInput()
    {
        if (_txtNewArtistName is not null)
        {
            _txtNewArtistName.Clear();
        }
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
        _numDuration.Value = 180;
        _importedGenreName = null;
    }

    public void LoadTrackIntoEditor(TrackDto track)
    {
        _editingTrackId = track.Id;
        _txtTrackTitle.Text = track.Title;
        _txtAlbum.Text = track.Album;
        _numDuration.Value = Math.Min(_numDuration.Maximum, Math.Max(_numDuration.Minimum, track.DurationSeconds));
        _txtAudioFilePath.Clear();
        _importedGenreName = null;

        SetSelectedArtistsFromText(track.Artist);
        SetSelectedGenresFromText(track.Genre);
        SelectCategoryByName(track.Category);
    }

    private Control BuildManageLayout()
    {
        var root = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };

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

        var fieldsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 3,
            AutoSize = true
        };
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        fieldsLayout.Controls.Add(new Label { Text = "Аудиофайл", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        _txtAudioFilePath = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, MinimumSize = new Size(260, 0) };
        fieldsLayout.Controls.Add(_txtAudioFilePath, 1, 0);
        fieldsLayout.SetColumnSpan(_txtAudioFilePath, 2);
        var btnPickAudio = new Button { Text = "Выбрать", AutoSize = true, Anchor = AnchorStyles.Left };
        fieldsLayout.Controls.Add(btnPickAudio, 3, 0);

        fieldsLayout.Controls.Add(new Label { Text = "Трек", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        _txtTrackTitle = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fieldsLayout.Controls.Add(_txtTrackTitle, 1, 1);

        fieldsLayout.Controls.Add(new Label { Text = "Альбом", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 1);
        _txtAlbum = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fieldsLayout.Controls.Add(_txtAlbum, 3, 1);

        fieldsLayout.Controls.Add(new Label { Text = "Категория", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
        _cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fieldsLayout.Controls.Add(_cmbCategory, 1, 2);

        fieldsLayout.Controls.Add(new Label { Text = "Длительность, сек", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 2);
        _numDuration = new NumericUpDown { Maximum = 10000, Minimum = 0, Value = 180, Dock = DockStyle.Fill, MinimumSize = new Size(220, 0) };
        fieldsLayout.Controls.Add(_numDuration, 3, 2);
        top.Controls.Add(fieldsLayout, 0, 0);

        var selectionLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            MinimumSize = new Size(0, 170),
            Margin = new Padding(0, 8, 0, 8)
        };
        selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var artistPanel = BuildSelectionPanel(
            "Артисты",
            "Поиск артиста...",
            out _txtArtistSearch,
            out _clbArtists,
            ApplyArtistFilter,
            ArtistItemCheckChanged);

        var genrePanel = BuildSelectionPanel(
            "Жанры",
            "Поиск жанра...",
            out _txtGenreSearch,
            out _clbGenres,
            ApplyGenreFilter,
            GenreItemCheckChanged);

        selectionLayout.Controls.Add(artistPanel, 0, 0);
        selectionLayout.Controls.Add(genrePanel, 1, 0);
        top.Controls.Add(selectionLayout, 0, 1);

        top.Controls.Add(new Label
        {
            Text = "Если у mp3 есть теги, поля заполнятся автоматически. Жанр и категория будут выбраны или созданы при сохранении.",
            AutoSize = true
        }, 0, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true
        };
        var btnAddTrack = new Button { Text = "Сохранить трек", AutoSize = true };
        var btnUpdateTrack = new Button { Text = "Обновить выбранный", AutoSize = true };
        var btnDeleteTrack = new Button { Text = "Удалить выбранный", AutoSize = true };
        var btnLoadTrack = new Button { Text = "Заполнить из выбранного", AutoSize = true };
        buttonPanel.Controls.Add(btnAddTrack);
        buttonPanel.Controls.Add(btnUpdateTrack);
        buttonPanel.Controls.Add(btnDeleteTrack);
        buttonPanel.Controls.Add(btnLoadTrack);
        top.Controls.Add(buttonPanel, 0, 3);
        buttonPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;

        var lowerGrid = CreateGrid(_tracksSource);

        root.Panel1.Controls.Add(top);
        root.Panel2.Controls.Add(lowerGrid);

        btnPickAudio.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.ImportAudioFileAsync();
            }
        };
        btnAddTrack.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.AddManualTrackAsync();
            }
        };
        btnUpdateTrack.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.UpdateSelectedTrackAsync();
            }
        };
        btnDeleteTrack.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.DeleteSelectedTrackAsync();
            }
        };
        btnLoadTrack.Click += (_, _) => _presenter?.LoadSelectedTrackIntoEditor();

        return root;
    }
    private Control BuildGenreCatalogLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        _txtGenreLookupSearch = new TextBox { Left = 12, Top = 12, Width = 360, PlaceholderText = "Search genre..." };
        _txtGenreLookupSearch.TextChanged += (_, _) => ApplyGenreLookupFilter(_txtGenreLookupSearch.Text);

        _txtNewGenreName = new TextBox { Left = 12, Top = 48, Width = 260, PlaceholderText = "New genre" };
        var btnAdd = new Button { Left = 280, Top = 48, Width = 90, Height = 28, Text = "Add" };
        var btnDelete = new Button { Left = 380, Top = 48, Width = 120, Height = 28, Text = "Delete selected" };

        _gridGenreLookup = new DataGridView
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
            DataSource = _genreLookupSource
        };

        btnAdd.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.AddGenreLookupAsync();
            }
        };
        btnDelete.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.DeleteSelectedGenreLookupAsync();
            }
        };

        panel.Controls.Add(_txtGenreLookupSearch);
        panel.Controls.Add(_txtNewGenreName);
        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(_gridGenreLookup);

        return panel;
    }

    private Control BuildCategoryCatalogLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        _txtCategoryLookupSearch = new TextBox { Left = 12, Top = 12, Width = 360, PlaceholderText = "Search category..." };
        _txtCategoryLookupSearch.TextChanged += (_, _) => ApplyCategoryLookupFilter(_txtCategoryLookupSearch.Text);

        _txtNewCategoryName = new TextBox { Left = 12, Top = 48, Width = 260, PlaceholderText = "New category" };
        var btnAdd = new Button { Left = 280, Top = 48, Width = 90, Height = 28, Text = "Add" };
        var btnDelete = new Button { Left = 380, Top = 48, Width = 120, Height = 28, Text = "Delete selected" };

        _gridCategoryLookup = new DataGridView
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
            DataSource = _categoryLookupSource
        };

        btnAdd.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.AddCategoryLookupAsync();
            }
        };
        btnDelete.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.DeleteSelectedCategoryLookupAsync();
            }
        };

        panel.Controls.Add(_txtCategoryLookupSearch);
        panel.Controls.Add(_txtNewCategoryName);
        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(_gridCategoryLookup);

        return panel;
    }

    private Control BuildArtistCatalogLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        _txtArtistLookupSearch = new TextBox { Left = 12, Top = 12, Width = 360, PlaceholderText = "Search artist..." };
        _txtArtistLookupSearch.TextChanged += (_, _) => ApplyArtistLookupFilter(_txtArtistLookupSearch.Text);

        _txtNewArtistName = new TextBox { Left = 12, Top = 48, Width = 260, PlaceholderText = "New artist" };
        var btnAdd = new Button { Left = 280, Top = 48, Width = 90, Height = 28, Text = "Add" };
        var btnDelete = new Button { Left = 380, Top = 48, Width = 120, Height = 28, Text = "Delete selected" };

        _gridArtistLookup = new DataGridView
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
            DataSource = _artistLookupSource
        };

        btnAdd.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.AddArtistLookupAsync();
            }
        };
        btnDelete.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.DeleteSelectedArtistLookupAsync();
            }
        };

        panel.Controls.Add(_txtArtistLookupSearch);
        panel.Controls.Add(_txtNewArtistName);
        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnDelete);
        panel.Controls.Add(_gridArtistLookup);

        return panel;
    }
    private Control BuildTracksLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var btnPlaySelected = new Button { Text = "Прослушать выбранный", Left = 12, Top = 58, Width = 180, Height = 30 };

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
        var titleLabel = new Label { Text = "Название", Left = 0, Top = 10, Width = 70 };
        _txtTrackSearchTitle = new TextBox { Left = 0, Top = 32, Width = 240 };
        var artistLabel = new Label { Text = "Исполнитель", Left = 260, Top = 10, Width = 90 };
        _txtTrackSearchArtist = new TextBox { Left = 260, Top = 32, Width = 240 };
        var albumLabel = new Label { Text = "Альбом", Left = 520, Top = 10, Width = 60 };
        _txtTrackSearchAlbum = new TextBox { Left = 520, Top = 32, Width = 240 };
        var genreLabel = new Label { Text = "Жанр", Left = 780, Top = 10, Width = 50 };
        _txtTrackSearchGenre = new TextBox { Left = 780, Top = 32, Width = 180 };
        var btnSearch = new Button { Text = "Поиск", Left = 970, Top = 32, Width = 110, Height = 30 };
        var btnReset = new Button { Text = "Сбросить", Left = 1090, Top = 32, Width = 110, Height = 30 };

        searchPanel.Controls.AddRange([titleLabel, _txtTrackSearchTitle, artistLabel, _txtTrackSearchArtist, albumLabel, _txtTrackSearchAlbum, genreLabel, _txtTrackSearchGenre, btnSearch, btnReset, btnPlaySelected]);

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

        btnSearch.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.SearchTracksAsync();
            }
        };
        btnReset.Click += async (_, _) =>
        {
            _txtTrackSearchTitle.Clear();
            _txtTrackSearchArtist.Clear();
            _txtTrackSearchAlbum.Clear();
            _txtTrackSearchGenre.Clear();
            if (_presenter is not null)
            {
                await _presenter.SearchTracksAsync();
            }
        };
        btnPlaySelected.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.PlaySelectedTrackAsync();
            }
        };
        return panel;
    }

    private Control BuildUsersLayout()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 360
        };

        _gridUsers = CreateGrid(_usersSource);
        _gridUsers.SelectionChanged += GridUsers_SelectionChanged;
        root.Panel1.Controls.Add(_gridUsers);

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

        root.Panel2.Controls.Add(rightSplit);
        return root;
    }

    private Control BuildDeezerLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        _txtDeezerQuery = new TextBox { Left = 12, Top = 12, Width = 560 };
        var btnSearch = new Button { Text = "Поиск Deezer", Left = 584, Top = 12, Width = 140, Height = 30 };
        var btnImport = new Button { Text = "Импортировать", Left = 730, Top = 12, Width = 150, Height = 30 };
        var btnPlayPreview = new Button { Text = "Слушать preview", Left = 12, Top = 48, Width = 150, Height = 30 };
        var btnSelectAll = new Button { Text = "Выбрать все", Left = 174, Top = 48, Width = 130, Height = 30 };
        var btnClearSelection = new Button { Text = "Сбросить выбор", Left = 316, Top = 48, Width = 140, Height = 30 };

        _gridDeezer = new DataGridView
        {
            Top = 90,
            Left = 12,
            Width = 1180,
            Height = 520,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = false,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = true,
            AllowUserToAddRows = false,
            DataSource = _deezerSource
        };

        var selectColumn = new DataGridViewCheckBoxColumn
        {
            Name = "SelectColumn",
            HeaderText = "Выбрать",
            Width = 60,
            ReadOnly = false,
            TrueValue = true,
            FalseValue = false
        };

        panel.Controls.Add(_txtDeezerQuery);
        panel.Controls.Add(btnSearch);
        panel.Controls.Add(btnImport);
        panel.Controls.Add(btnPlayPreview);
        panel.Controls.Add(btnSelectAll);
        panel.Controls.Add(btnClearSelection);
        panel.Controls.Add(_gridDeezer);

        btnSearch.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.SearchDeezerAsync();
            }
        };
        btnImport.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.ImportDeezerTracksAsync();
            }
        };
        btnPlayPreview.Click += async (_, _) =>
        {
            if (_presenter is not null)
            {
                await _presenter.PlaySelectedDeezerTrackAsync();
            }
        };
        btnSelectAll.Click += (_, _) => SetAllDeezerSelection(true);
        btnClearSelection.Click += (_, _) => SetAllDeezerSelection(false);

        _gridDeezer.DataBindingComplete += (_, _) =>
        {
            if (!_gridDeezer.Columns.Contains("SelectColumn"))
            {
                _gridDeezer.Columns.Insert(0, selectColumn);
            }

            foreach (DataGridViewColumn column in _gridDeezer.Columns)
            {
                if (column.Name != "SelectColumn")
                {
                    column.ReadOnly = true;
                }
            }
        };

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
            Height = 24,
            Text = "Сейчас играет: ничего не выбрано"
        };

        var playerPanel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(12) };
        playerPanel.Controls.Add(_player);
        playerPanel.Controls.Add(_lblNowPlaying);
        return playerPanel;
    }

    private void SetAllDeezerSelection(bool isSelected)
    {
        foreach (DataGridViewRow row in _gridDeezer.Rows)
        {
            if (row.Cells["SelectColumn"] is DataGridViewCheckBoxCell checkBoxCell)
            {
                checkBoxCell.Value = isSelected;
            }
        }
    }

    private void ApplyGenreFilter(string filterText)
    {
        var source = string.IsNullOrWhiteSpace(filterText)
            ? _allGenres
            : _allGenres.Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToList();

        RebindCheckedList(_clbGenres, source, _selectedGenreNames, static genre => genre.Name);
    }

    private void ApplyArtistFilter(string filterText)
    {
        var source = string.IsNullOrWhiteSpace(filterText)
            ? _allArtists
            : _allArtists.Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToList();

        RebindCheckedList(_clbArtists, source, _selectedArtistNames, static artist => artist.Name);
    }

    private void TryCheckArtistByName(string? artistName)
    {
        if (string.IsNullOrWhiteSpace(artistName))
        {
            return;
        }

        var found = false;
        for (var i = 0; i < _clbArtists.Items.Count; i++)
        {
            if (_clbArtists.Items[i] is ArtistDto artist &&
                string.Equals(artist.Name, artistName, StringComparison.OrdinalIgnoreCase))
            {
                _clbArtists.SetItemChecked(i, true);
                found = true;
                break;
            }
        }

        if (found)
        {
            _selectedArtistNames.Add(artistName);
        }
    }

    private void TryCheckGenreByName(string? genreName)
    {
        if (string.IsNullOrWhiteSpace(genreName))
        {
            return;
        }

        for (var i = 0; i < _clbGenres.Items.Count; i++)
        {
            if (_clbGenres.Items[i] is GenreDto genre &&
                string.Equals(genre.Name, genreName, StringComparison.OrdinalIgnoreCase))
            {
                _clbGenres.SetItemChecked(i, true);
                _selectedGenreNames.Add(genreName);
                break;
            }
        }
    }

    private void ApplyGenreLookupFilter(string filterText)
    {
        var filtered = string.IsNullOrWhiteSpace(filterText)
            ? _genreLookupAll
            : _genreLookupAll
                .Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _genreLookupSource.DataSource = filtered;
    }

    private void ApplyCategoryLookupFilter(string filterText)
    {
        var filtered = string.IsNullOrWhiteSpace(filterText)
            ? _categoryLookupAll
            : _categoryLookupAll
                .Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _categoryLookupSource.DataSource = filtered;
    }

    private void ApplyArtistLookupFilter(string filterText)
    {
        var filtered = string.IsNullOrWhiteSpace(filterText)
            ? _artistLookupAll
            : _artistLookupAll
                .Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _artistLookupSource.DataSource = filtered;
    }

    private static DataGridView CreateGrid(object dataSource)
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = dataSource
        };
    }

    private GroupBox BuildSelectionPanel(
        string title,
        string placeholder,
        out TextBox searchTextBox,
        out CheckedListBox checkedListBox,
        Action<string> applyFilter,
        ItemCheckEventHandler itemCheckHandler)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var searchBox = new TextBox { Dock = DockStyle.Top, PlaceholderText = placeholder };
        var checkedList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            IntegralHeight = false,
            MinimumSize = new Size(0, 120)
        };

        searchBox.TextChanged += (_, _) => applyFilter(searchBox.Text);
        checkedList.ItemCheck += itemCheckHandler;

        layout.Controls.Add(searchBox, 0, 0);
        layout.Controls.Add(checkedList, 0, 1);
        group.Controls.Add(layout);

        searchTextBox = searchBox;
        checkedListBox = checkedList;

        return group;
    }

    private void GenreItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbGenres.Items[e.Index] is GenreDto genre)
        {
            UpdateSelectionSet(_selectedGenreNames, genre.Name, e.NewValue);
        }
    }

    private void ArtistItemCheckChanged(object? sender, ItemCheckEventArgs e)
    {
        if (_clbArtists.Items[e.Index] is ArtistDto artist)
        {
            UpdateSelectionSet(_selectedArtistNames, artist.Name, e.NewValue);
        }
    }

    private static void UpdateSelectionSet(HashSet<string> selectedNames, string itemName, CheckState newValue)
    {
        if (newValue == CheckState.Checked)
        {
            selectedNames.Add(itemName);
            return;
        }

        selectedNames.Remove(itemName);
    }

    private static void RebindCheckedList<TItem>(
        CheckedListBox listBox,
        IReadOnlyList<TItem> source,
        HashSet<string> selectedNames,
        Func<TItem, string> getName)
        where TItem : class
    {
        listBox.BeginUpdate();
        listBox.Items.Clear();
        listBox.DisplayMember = "Name";

        foreach (var item in source)
        {
            var index = listBox.Items.Add(item);
            var name = getName(item);
            if (!string.IsNullOrWhiteSpace(name) && selectedNames.Contains(name))
            {
                listBox.SetItemChecked(index, true);
            }
        }

        listBox.EndUpdate();
    }

    private void SetSelectedArtistsFromText(string artistText)
    {
        _selectedArtistNames.Clear();
        _txtArtistSearch.Clear();
        ApplyArtistFilter(string.Empty);

        foreach (var artistName in SplitNames(artistText))
        {
            _selectedArtistNames.Add(artistName);
            TryCheckArtistByName(artistName);
        }
    }

    private void SetSelectedGenresFromText(string genreText)
    {
        _selectedGenreNames.Clear();
        _txtGenreSearch.Clear();
        ApplyGenreFilter(string.Empty);

        foreach (var genreName in SplitNames(genreText))
        {
            _selectedGenreNames.Add(genreName);
            TryCheckGenreByName(genreName);
        }
    }

    private void SelectCategoryByName(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            _cmbCategory.SelectedIndex = -1;
            _cmbCategory.Text = string.Empty;
            return;
        }

        for (var i = 0; i < _cmbCategory.Items.Count; i++)
        {
            if (_cmbCategory.Items[i] is CategoryDto category &&
                string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase))
            {
                _cmbCategory.SelectedIndex = i;
                return;
            }
        }

        _cmbCategory.Text = categoryName;
    }

    private static IEnumerable<string> SplitNames(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return [];
        }

        return rawValue.Split([',', ';', '|', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsInDesigner()
    {
        return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    }

    private async void GridUsers_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressUserSelectionChanged)
        {
            return;
        }

        if (_presenter is not null)
        {
            await _presenter.UserSelectionChangedAsync();
        }
    }

    private async void GridUserPlaylists_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressUserPlaylistSelectionChanged)
        {
            return;
        }

        if (_presenter is not null)
        {
            await _presenter.UserPlaylistSelectionChangedAsync();
        }
    }

}





