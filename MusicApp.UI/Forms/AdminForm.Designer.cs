namespace MusicApp.UI.Forms;

partial class AdminForm
{
    private System.ComponentModel.IContainer? components = null;
    private TabControl _tabControl = null!;
    private TabPage _manageTab = null!;
    private TabPage _tracksTab = null!;
    private TabPage _usersTab = null!;
    private TabPage _categoriesTab = null!;
    private TabPage _genresTab = null!;
    private TabPage _artistsTab = null!;
    private TabPage _albumsTab = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _tabControl = new TabControl();
        _manageTab = new TabPage();
        _tracksTab = new TabPage();
        _usersTab = new TabPage();
        _categoriesTab = new TabPage();
        _genresTab = new TabPage();
        _artistsTab = new TabPage();
        _albumsTab = new TabPage();
        SuspendLayout();

        _tabControl.Dock = DockStyle.Fill;

        _manageTab.Text = "Управление каталогом";
        _manageTab.Padding = new Padding(3);
        _manageTab.Controls.Add(BuildManageLayout());

        _tracksTab.Text = "Треки";
        _tracksTab.Padding = new Padding(3);
        _tracksTab.Controls.Add(BuildTracksLayout());

        _usersTab.Text = "Пользователи";
        _usersTab.Padding = new Padding(3);
        _usersTab.Controls.Add(BuildUsersLayout());

        _categoriesTab.Text = "Категории";
        _categoriesTab.Padding = new Padding(3);
        _categoriesTab.Controls.Add(BuildCategoryCatalogLayout());

        _genresTab.Text = "Жанры";
        _genresTab.Padding = new Padding(3);
        _genresTab.Controls.Add(BuildGenreCatalogLayout());

        _artistsTab.Text = "Исполнители";
        _artistsTab.Padding = new Padding(3);
        _artistsTab.Controls.Add(BuildArtistCatalogLayout());

        _albumsTab.Text = "Альбомы";
        _albumsTab.Padding = new Padding(3);
        _albumsTab.Controls.Add(BuildAlbumsLayout());

        _tabControl.Controls.AddRange([_manageTab, _tracksTab, _usersTab, _categoriesTab, _genresTab, _artistsTab, _albumsTab]);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 860);
        Controls.Add(_tabControl);
        Name = "AdminForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Music App — Admin Panel";
        ResumeLayout(false);
    }
}