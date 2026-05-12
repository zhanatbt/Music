using MusicApp.Application.DTOs;
using MusicApp.Application.Services;
using MusicApp.UI.Presenters;

namespace MusicApp.UI.Forms;

public class LoginForm : Form, ILoginView
{
    private readonly AuthService _authService;
    private readonly AdminCatalogService _adminCatalogService;
    private readonly MusicLibraryService _musicLibraryService;
    private readonly LoginPresenter _presenter;

    private readonly TextBox _txtUsername;
    private readonly TextBox _txtPassword;

    public LoginForm(AuthService authService, AdminCatalogService adminCatalogService, MusicLibraryService musicLibraryService)
    {
        _authService = authService;
        _adminCatalogService = adminCatalogService;
        _musicLibraryService = musicLibraryService;
        _presenter = new LoginPresenter(this, authService);

        Text = "Music App";
        Width = 460;
        Height = 400;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = AppleMusicTheme.Background;
        ForeColor = AppleMusicTheme.TextPrimary;

        // ── Logo / title ──
        var logo = new Label
        {
            Text = "♫",
            Font = new Font("Segoe UI", 42f, FontStyle.Bold),
            ForeColor = AppleMusicTheme.Accent,
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = "Music App",
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = AppleMusicTheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 38,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        // ── Form panel ──
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 10, 40, 20),
            ColumnCount = 1,
            RowCount = 7,
            BackColor = AppleMusicTheme.Background
        };
        for (var i = 0; i < 7; i++)
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        panel.Controls.Add(MkLabel("Логин"), 0, 0);
        _txtUsername = new TextBox { Dock = DockStyle.Fill, BackColor = AppleMusicTheme.Surface, ForeColor = AppleMusicTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(_txtUsername, 0, 1);

        panel.Controls.Add(MkLabel("Пароль"), 0, 2);
        _txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, BackColor = AppleMusicTheme.Surface, ForeColor = AppleMusicTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(_txtPassword, 0, 3);

        var btnLogin = new Button
        {
            Text = "Войти",
            Dock = DockStyle.Fill,
            Height = 36,
            BackColor = AppleMusicTheme.Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        panel.Controls.Add(btnLogin, 0, 4);

        var altButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            BackColor = AppleMusicTheme.Background
        };
        var btnRegister = new Button { Text = "Регистрация", Width = 150, Height = 30, BackColor = AppleMusicTheme.SurfaceAlt, ForeColor = AppleMusicTheme.TextPrimary, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnRecover = new Button { Text = "Забыли пароль?", Width = 160, Height = 30, BackColor = AppleMusicTheme.SurfaceAlt, ForeColor = AppleMusicTheme.TextSecondary, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRecover.FlatAppearance.BorderSize = 0;
        altButtons.Controls.AddRange([btnRegister, btnRecover]);
        panel.Controls.Add(altButtons, 0, 5);

        var hint = new Label
        {
            Text = "demo: admin / admin123   •   user / user123",
            AutoSize = true,
            ForeColor = AppleMusicTheme.TextSecondary,
            Font = new Font("Segoe UI", 8f)
        };
        panel.Controls.Add(hint, 0, 6);

        Controls.Add(panel);
        Controls.Add(title);
        Controls.Add(logo);

        btnLogin.Click += async (_, _) => await _presenter.LoginAsync();
        btnRegister.Click += (_, _) => _presenter.Register();
        btnRecover.Click += (_, _) => _presenter.RecoverPassword();

        // allow Enter key to login
        AcceptButton = btnLogin;
    }

    public string Username => _txtUsername.Text;
    public string Password => _txtPassword.Text;

    public void ShowMessage(string message, string title = "Music App")
        => MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void OpenRegistration()
    {
        using var form = new RegisterForm(_authService);
        form.ShowDialog(this);
    }

    public void OpenPasswordRecovery()
    {
        using var form = new RecoverPasswordForm(_authService, _txtUsername.Text);
        form.ShowDialog(this);
    }

    public void OpenAdmin(UserSessionDto session)
    {
        Hide();
        using var form = new AdminForm(_adminCatalogService, session);
        form.ShowDialog(this);
        Show();
    }

    public void OpenUser(UserSessionDto session)
    {
        Hide();
        using var form = new MainForm(_musicLibraryService, session);
        form.ShowDialog(this);
        Show();
    }

    private static Label MkLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = AppleMusicTheme.TextSecondary,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", 9f)
    };
}