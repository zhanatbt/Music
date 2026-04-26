using AxWMPLib;
using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;

namespace MusicApp.UI.Forms;

public class AdminForm : Form, IAdminView
{
    private readonly AdminPresenter _presenter;
    private readonly BindingSource _genresSource = new();
    private readonly BindingSource _categoriesSource = new();
    private readonly BindingSource _tracksSource = new();
    private readonly BindingSource _usersSource = new();
    private readonly BindingSource _deezerSource = new();

    private TextBox _txtGenre = null!;
    private TextBox _txtCategory = null!;
    private TextBox _txtTrackTitle = null!;
    private TextBox _txtArtist = null!;
    private TextBox _txtAlbum = null!;
    private TextBox _txtAudioFilePath = null!;
    private NumericUpDown _numDuration = null!;
    private ComboBox _cmbGenre = null!;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtTrackSearchTitle = null!;
    private TextBox _txtTrackSearchArtist = null!;
    private TextBox _txtTrackSearchAlbum = null!;
    private TextBox _txtTrackSearchGenre = null!;
    private TextBox _txtDeezerQuery = null!;
    private DataGridView _gridDeezer = null!;
    private DataGridView _gridTracks = null!;
    private Label _lblNowPlaying = null!;
    private AxWindowsMediaPlayer _player = null!;
    private readonly List<DeezerTrackDto> _deezerResults = new();
    private string? _importedGenreName;

    public AdminForm(AdminCatalogService catalogService, UserSessionDto session)
    {
        _presenter = new AdminPresenter(this, catalogService);

        Text = $"Music App - Admin Panel ({session.Username})";
        Width = 1240;
        Height = 820;
        StartPosition = FormStartPosition.CenterParent;

        var tabControl = new TabControl { Dock = DockStyle.Fill };

        var manageTab = new TabPage("Управление каталогом");
        manageTab.Controls.Add(BuildManageLayout());

        var tracksTab = new TabPage("Треки");
        tracksTab.Controls.Add(BuildTracksLayout());

        var usersTab = new TabPage("Пользователи");
        usersTab.Controls.Add(CreateGrid(_usersSource));

        var deezerTab = new TabPage("Импорт Deezer");
        deezerTab.Controls.Add(BuildDeezerLayout());

        tabControl.TabPages.Add(manageTab);
        tabControl.TabPages.Add(tracksTab);
        tabControl.TabPages.Add(usersTab);
        tabControl.TabPages.Add(deezerTab);

        Controls.Add(tabControl);
        Controls.Add(BuildPlayerPanel());

        Load += async (_, _) => await _presenter.LoadAsync();
    }

    public string GenreName => _txtGenre.Text;
    public string CategoryName => _txtCategory.Text;
    public string TrackTitle => _txtTrackTitle.Text;
    public string ArtistName => _txtArtist.Text;
    public string AlbumTitle => _txtAlbum.Text;
    public int DurationSeconds => (int)_numDuration.Value;
    public int? SelectedGenreId => (_cmbGenre.SelectedItem as GenreDto)?.Id;
    public int? SelectedCategoryId => (_cmbCategory.SelectedItem as CategoryDto)?.Id;
    public string TrackSearchTitle => _txtTrackSearchTitle.Text;
    public string TrackSearchArtist => _txtTrackSearchArtist.Text;
    public string TrackSearchAlbum => _txtTrackSearchAlbum.Text;
    public string TrackSearchGenre => _txtTrackSearchGenre.Text;
    public string DeezerQuery => _txtDeezerQuery.Text;
    public string? ImportedAudioFilePath => string.IsNullOrWhiteSpace(_txtAudioFilePath.Text) ? null : _txtAudioFilePath.Text;
    public string? ImportedGenreName => _importedGenreName;
    public TrackDto? SelectedTrack => _gridTracks.CurrentRow?.DataBoundItem as TrackDto;
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
        _txtArtist.Text = metadata.ArtistName;
        _txtAlbum.Text = metadata.AlbumTitle;
        _numDuration.Value = Math.Min(_numDuration.Maximum, Math.Max(_numDuration.Minimum, metadata.DurationSeconds));
        _importedGenreName = metadata.GenreName;
    }

    public void TrySelectGenreByName(string? genreName)
    {
        _importedGenreName = genreName;
        if (string.IsNullOrWhiteSpace(genreName))
        {
            return;
        }

        for (var i = 0; i < _cmbGenre.Items.Count; i++)
        {
            if (_cmbGenre.Items[i] is GenreDto genre &&
                string.Equals(genre.Name, genreName, StringComparison.OrdinalIgnoreCase))
            {
                _cmbGenre.SelectedIndex = i;
                return;
            }
        }
    }

    public void SetGenres(IReadOnlyList<GenreDto> genres)
    {
        _genresSource.DataSource = genres;
        _cmbGenre.DataSource = _genresSource;
        _cmbGenre.DisplayMember = "Name";
        _cmbGenre.ValueMember = "Id";
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
        _usersSource.DataSource = users;
    }

    public void SetDeezerResults(IReadOnlyList<DeezerTrackDto> tracks)
    {
        _deezerResults.Clear();
        _deezerResults.AddRange(tracks);
        _deezerSource.DataSource = _deezerResults.ToList();
        _gridDeezer.DataSource = _deezerSource;
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
        _txtGenre.Clear();
        _txtCategory.Clear();
        _txtTrackTitle.Clear();
        _txtArtist.Clear();
        _txtAlbum.Clear();
        _txtAudioFilePath.Clear();
        _numDuration.Value = 180;
        _importedGenreName = null;
    }

    private Control BuildManageLayout()
    {
        var root = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 4,
            RowCount = 8
        };

        top.Controls.Add(new Label { Text = "Новый жанр" }, 0, 0);
        _txtGenre = new TextBox();
        top.Controls.Add(_txtGenre, 1, 0);
        var btnAddGenre = new Button { Text = "Добавить жанр" };
        top.Controls.Add(btnAddGenre, 2, 0);

        top.Controls.Add(new Label { Text = "Новая категория" }, 0, 1);
        _txtCategory = new TextBox();
        top.Controls.Add(_txtCategory, 1, 1);
        var btnAddCategory = new Button { Text = "Добавить категорию" };
        top.Controls.Add(btnAddCategory, 2, 1);

        top.Controls.Add(new Label { Text = "Аудиофайл" }, 0, 2);
        _txtAudioFilePath = new TextBox { ReadOnly = true };
        top.Controls.Add(_txtAudioFilePath, 1, 2);
        var btnPickAudio = new Button { Text = "Выбрать mp3" };
        top.Controls.Add(btnPickAudio, 2, 2);

        top.Controls.Add(new Label { Text = "Трек" }, 0, 3);
        _txtTrackTitle = new TextBox();
        top.Controls.Add(_txtTrackTitle, 1, 3);

        top.Controls.Add(new Label { Text = "Исполнитель" }, 0, 4);
        _txtArtist = new TextBox();
        top.Controls.Add(_txtArtist, 1, 4);

        top.Controls.Add(new Label { Text = "Альбом" }, 2, 3);
        _txtAlbum = new TextBox();
        top.Controls.Add(_txtAlbum, 3, 3);

        top.Controls.Add(new Label { Text = "Длительность, сек" }, 2, 4);
        _numDuration = new NumericUpDown { Maximum = 10000, Minimum = 0, Value = 180 };
        top.Controls.Add(_numDuration, 3, 4);

        top.Controls.Add(new Label { Text = "Жанр" }, 0, 5);
        _cmbGenre = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        top.Controls.Add(_cmbGenre, 1, 5);

        top.Controls.Add(new Label { Text = "Категория" }, 2, 5);
        _cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        top.Controls.Add(_cmbCategory, 3, 5);

        top.Controls.Add(new Label
        {
            Text = "Если у mp3 есть теги, поля заполнятся автоматически. Жанр будет выбран или создан при сохранении.",
            AutoSize = true
        }, 0, 6);

        var btnAddTrack = new Button { Text = "Сохранить трек" };
        top.Controls.Add(btnAddTrack, 1, 7);

        var lowerGrid = CreateGrid(_tracksSource);

        root.Panel1.Controls.Add(top);
        root.Panel2.Controls.Add(lowerGrid);

        btnAddGenre.Click += async (_, _) => await _presenter.AddGenreAsync();
        btnAddCategory.Click += async (_, _) => await _presenter.AddCategoryAsync();
        btnPickAudio.Click += async (_, _) => await _presenter.ImportAudioFileAsync();
        btnAddTrack.Click += async (_, _) => await _presenter.AddManualTrackAsync();

        return root;
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

        btnSearch.Click += async (_, _) => await _presenter.SearchTracksAsync();
        btnReset.Click += async (_, _) =>
        {
            _txtTrackSearchTitle.Clear();
            _txtTrackSearchArtist.Clear();
            _txtTrackSearchAlbum.Clear();
            _txtTrackSearchGenre.Clear();
            await _presenter.SearchTracksAsync();
        };
        btnPlaySelected.Click += async (_, _) => await _presenter.PlaySelectedTrackAsync();
        return panel;
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

        btnSearch.Click += async (_, _) => await _presenter.SearchDeezerAsync();
        btnImport.Click += async (_, _) => await _presenter.ImportDeezerTracksAsync();
        btnPlayPreview.Click += async (_, _) => await _presenter.PlaySelectedDeezerTrackAsync();
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
}
