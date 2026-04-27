namespace MusicApp.UI.Forms;

partial class AdminForm
{
    private System.ComponentModel.IContainer? components = null;
    private TabControl _tabControl = null!;
    private TabPage _manageTab = null!;
    private TabPage _tracksTab = null!;
    private TabPage _usersTab = null!;
    private TabPage _genresTab = null!;
    private TabPage _artistsTab = null!;
    private TabPage _deezerTab = null!;
    private Panel _playerHost = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _tabControl = new TabControl();
        _manageTab = new TabPage();
        _tracksTab = new TabPage();
        _usersTab = new TabPage();
        _genresTab = new TabPage();
        _artistsTab = new TabPage();
        _deezerTab = new TabPage();
        _playerHost = new Panel();
        SuspendLayout();

        _tabControl.Dock = DockStyle.Fill;
        _tabControl.Name = "_tabControl";
        _tabControl.TabIndex = 0;

        _manageTab.Name = "_manageTab";
        _manageTab.Padding = new Padding(3);
        _manageTab.Text = "Управление каталогом";
        _manageTab.UseVisualStyleBackColor = true;
        _manageTab.Controls.Add(BuildManageLayout());

        _tracksTab.Name = "_tracksTab";
        _tracksTab.Padding = new Padding(3);
        _tracksTab.Text = "Треки";
        _tracksTab.UseVisualStyleBackColor = true;
        _tracksTab.Controls.Add(BuildTracksLayout());

        _usersTab.Name = "_usersTab";
        _usersTab.Padding = new Padding(3);
        _usersTab.Text = "Пользователи";
        _usersTab.UseVisualStyleBackColor = true;
        _usersTab.Controls.Add(CreateGrid(_usersSource));

        _genresTab.Name = "_genresTab";
        _genresTab.Padding = new Padding(3);
        _genresTab.Text = "Genres";
        _genresTab.UseVisualStyleBackColor = true;
        _genresTab.Controls.Add(BuildGenreCatalogLayout());

        _artistsTab.Name = "_artistsTab";
        _artistsTab.Padding = new Padding(3);
        _artistsTab.Text = "Artists";
        _artistsTab.UseVisualStyleBackColor = true;
        _artistsTab.Controls.Add(BuildArtistCatalogLayout());

        _deezerTab.Name = "_deezerTab";
        _deezerTab.Padding = new Padding(3);
        _deezerTab.Text = "Импорт Deezer";
        _deezerTab.UseVisualStyleBackColor = true;
        _deezerTab.Controls.Add(BuildDeezerLayout());

        _tabControl.Controls.Add(_manageTab);
        _tabControl.Controls.Add(_tracksTab);
        _tabControl.Controls.Add(_usersTab);
        _tabControl.Controls.Add(_genresTab);
        _tabControl.Controls.Add(_artistsTab);
        _tabControl.Controls.Add(_deezerTab);

        _playerHost = (Panel)BuildPlayerPanel();

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1240, 820);
        Controls.Add(_tabControl);
        Controls.Add(_playerHost);
        Name = "AdminForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Music App - Admin Panel";
        ResumeLayout(false);
    }
}
