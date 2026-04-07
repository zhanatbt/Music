using MusicApp.Application.Services;
using MusicApp.UI.Presenters;

namespace MusicApp.UI.Forms;

public class RegisterForm : Form, IRegisterView
{
    private readonly RegisterPresenter _presenter;
    private readonly TextBox _txtUsername;
    private readonly TextBox _txtPassword;
    private readonly TextBox _txtConfirmPassword;

    public RegisterForm(AuthService authService)
    {
        _presenter = new RegisterPresenter(this, authService);

        Text = "Регистрация";
        Width = 400;
        Height = 300;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 9
        };

        layout.Controls.Add(new Label { Text = "Логин" }, 0, 0);
        _txtUsername = new TextBox();
        layout.Controls.Add(_txtUsername, 0, 1);

        layout.Controls.Add(new Label { Text = "Пароль" }, 0, 2);
        _txtPassword = new TextBox { UseSystemPasswordChar = true };
        layout.Controls.Add(_txtPassword, 0, 3);

        layout.Controls.Add(new Label { Text = "Подтвердите пароль" }, 0, 4);
        _txtConfirmPassword = new TextBox { UseSystemPasswordChar = true };
        layout.Controls.Add(_txtConfirmPassword, 0, 5);


        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var btnSave = new Button { Text = "Создать", Width = 120 };
        var btnCancel = new Button { Text = "Отмена", Width = 120 };
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);
        layout.Controls.Add(buttons, 0, 7);

        Controls.Add(layout);

        btnSave.Click += async (_, _) => await _presenter.RegisterAsync();
        btnCancel.Click += (_, _) => Close();
    }

    public string Username => _txtUsername.Text;
    public string Password => _txtPassword.Text;
    public string ConfirmPassword => _txtConfirmPassword.Text;

    public void ShowMessage(string message, string title = "Music App")
    {
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void CloseView()
    {
        DialogResult = DialogResult.OK;
        Close();
    }
}
