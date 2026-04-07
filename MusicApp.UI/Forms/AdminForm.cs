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
    private NumericUpDown _numDuration = null!;
    private ComboBox _cmbGenre = null!;
    private ComboBox _cmbCategory = null!;
    private TextBox _txtDeezerQuery = null!;
    private DataGridView _gridDeezer = null!;

    public AdminForm(AdminCatalogService catalogService, UserSessionDto session)
    {
        _presenter = new AdminPresenter(this, catalogService);

        Text = $"Music App - Admin Panel ({session.Username})";
        Width = 1200;
        Height = 760;
        StartPosition = FormStartPosition.CenterParent;

        var tabControl = new TabControl { Dock = DockStyle.Fill };

        var manageTab = new TabPage("Управление каталогом");
        manageTab.Controls.Add(BuildManageLayout());

        var tracksTab = new TabPage("Треки");
        var tracksGrid = CreateGrid(_tracksSource);
        tracksTab.Controls.Add(tracksGrid);

        var usersTab = new TabPage("Пользователи");
        var usersGrid = CreateGrid(_usersSource);
        usersTab.Controls.Add(usersGrid);

        var deezerTab = new TabPage("Импорт Deezer");
        deezerTab.Controls.Add(BuildDeezerLayout());

        tabControl.TabPages.Add(manageTab);
        tabControl.TabPages.Add(tracksTab);
        tabControl.TabPages.Add(usersTab);
        tabControl.TabPages.Add(deezerTab);

        Controls.Add(tabControl);

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
    public string DeezerQuery => _txtDeezerQuery.Text;

    public DeezerTrackDto? SelectedDeezerTrack
    {
        get
        {
            if (_gridDeezer.CurrentRow?.DataBoundItem is DeezerTrackDto track)
            {
                return track;
            }

            return null;
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
    }

    public void SetUsers(IReadOnlyList<UserSessionDto> users)
    {
        _usersSource.DataSource = users;
    }

    public void SetDeezerResults(IReadOnlyList<DeezerTrackDto> tracks)
    {
        _deezerSource.DataSource = tracks;
        _gridDeezer.DataSource = _deezerSource;
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
        _numDuration.Value = 0;
    }

    private Control BuildManageLayout()
    {
        var root = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250 };

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 4,
            RowCount = 6
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

        top.Controls.Add(new Label { Text = "Трек" }, 0, 2);
        _txtTrackTitle = new TextBox();
        top.Controls.Add(_txtTrackTitle, 1, 2);

        top.Controls.Add(new Label { Text = "Исполнитель" }, 0, 3);
        _txtArtist = new TextBox();
        top.Controls.Add(_txtArtist, 1, 3);

        top.Controls.Add(new Label { Text = "Альбом" }, 2, 2);
        _txtAlbum = new TextBox();
        top.Controls.Add(_txtAlbum, 3, 2);

        top.Controls.Add(new Label { Text = "Длительность, сек" }, 2, 3);
        _numDuration = new NumericUpDown { Maximum = 10000, Minimum = 0, Value = 180 };
        top.Controls.Add(_numDuration, 3, 3);

        top.Controls.Add(new Label { Text = "Жанр" }, 0, 4);
        _cmbGenre = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        top.Controls.Add(_cmbGenre, 1, 4);

        top.Controls.Add(new Label { Text = "Категория" }, 2, 4);
        _cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        top.Controls.Add(_cmbCategory, 3, 4);

        var btnAddTrack = new Button { Text = "Добавить трек вручную" };
        top.Controls.Add(btnAddTrack, 1, 5);

        var lowerGrid = CreateGrid(_tracksSource);

        root.Panel1.Controls.Add(top);
        root.Panel2.Controls.Add(lowerGrid);

        btnAddGenre.Click += async (_, _) => await _presenter.AddGenreAsync();
        btnAddCategory.Click += async (_, _) => await _presenter.AddCategoryAsync();
        btnAddTrack.Click += async (_, _) => await _presenter.AddManualTrackAsync();

        return root;
    }

    private Control BuildDeezerLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        _txtDeezerQuery = new TextBox { Left = 12, Top = 12, Width = 420 };
        var btnSearch = new Button { Text = "Поиск Deezer", Left = 440, Top = 10, Width = 120 };
        var btnImport = new Button { Text = "Импортировать", Left = 570, Top = 10, Width = 130 };

        _gridDeezer = new DataGridView
        {
            Top = 48,
            Left = 12,
            Width = 1120,
            Height = 580,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            DataSource = _deezerSource
        };

        panel.Controls.Add(_txtDeezerQuery);
        panel.Controls.Add(btnSearch);
        panel.Controls.Add(btnImport);
        panel.Controls.Add(_gridDeezer);

        btnSearch.Click += async (_, _) => await _presenter.SearchDeezerAsync();
        btnImport.Click += async (_, _) => await _presenter.ImportSelectedDeezerTrackAsync();

        return panel;
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
