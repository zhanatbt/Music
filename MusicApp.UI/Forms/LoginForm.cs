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
    private readonly Button _btnLogin;
    private readonly Button _btnRegister;
    private readonly Button _btnRecoverPassword;

    public LoginForm(AuthService authService, AdminCatalogService adminCatalogService, MusicLibraryService musicLibraryService)
    {
        _authService = authService;
        _adminCatalogService = adminCatalogService;
        _musicLibraryService = musicLibraryService;
        _presenter = new LoginPresenter(this, authService);

        Text = "Music App - Login";
        Width = 560;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;

        var title = new Label
        {
            Text = "Авторизация",
            Dock = DockStyle.Top,
            Height = 44,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13, FontStyle.Bold)
        };

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 7
        };

        panel.RowStyles.Clear();
        for (var i = 0; i < 7; i++)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        }

        panel.Controls.Add(new Label { Text = "Логин", AutoSize = true }, 0, 0);
        _txtUsername = new TextBox { Dock = DockStyle.Fill };
        panel.Controls.Add(_txtUsername, 0, 1);

        panel.Controls.Add(new Label { Text = "Пароль", AutoSize = true }, 0, 2);
        _txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
        panel.Controls.Add(_txtPassword, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true
        };
        _btnLogin = new Button { Text = "Войти", Width = 140, Height = 34 };
        _btnRegister = new Button { Text = "Регистрация", Width = 140, Height = 34 };
        _btnRecoverPassword = new Button { Text = "Забыли пароль?", Width = 170, Height = 34 };
        buttons.Controls.Add(_btnLogin);
        buttons.Controls.Add(_btnRegister);
        buttons.Controls.Add(_btnRecoverPassword);
        panel.Controls.Add(buttons, 0, 4);

        var hint = new Label
        {
            Text = "Тестовые учётные записи: admin/admin123, user/user123",
            AutoSize = true
        };
        panel.Controls.Add(hint, 0, 5);

        Controls.Add(panel);
        Controls.Add(title);

        _btnLogin.Click += async (_, _) => await _presenter.LoginAsync();
        _btnRegister.Click += (_, _) => _presenter.Register();
        _btnRecoverPassword.Click += (_, _) => _presenter.RecoverPassword();
    }

    public string Username => _txtUsername.Text;
    public string Password => _txtPassword.Text;

    public void ShowMessage(string message, string title = "Music App")
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

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
}
