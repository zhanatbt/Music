using MusicApp.Application.Services;

namespace MusicApp.UI.Forms;

public class RecoverPasswordForm : Form
{
    private readonly AuthService _authService;
    private readonly TextBox _txtUsername;
    private readonly TextBox _txtSecretWord;
    private readonly TextBox _txtNewPassword;
    private readonly TextBox _txtConfirmPassword;

    public RecoverPasswordForm(AuthService authService, string? initialUsername = null)
    {
        _authService = authService;

        Text = "Восстановление пароля";
        Width = 430;
        Height = 360;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 11
        };

        layout.Controls.Add(new Label { Text = "Логин" }, 0, 0);
        _txtUsername = new TextBox { Text = initialUsername ?? string.Empty };
        layout.Controls.Add(_txtUsername, 0, 1);

        layout.Controls.Add(new Label { Text = "Кодовое слово" }, 0, 2);
        _txtSecretWord = new TextBox();
        layout.Controls.Add(_txtSecretWord, 0, 3);

        layout.Controls.Add(new Label { Text = "Новый пароль" }, 0, 4);
        _txtNewPassword = new TextBox { UseSystemPasswordChar = true };
        layout.Controls.Add(_txtNewPassword, 0, 5);

        layout.Controls.Add(new Label { Text = "Подтвердите новый пароль" }, 0, 6);
        _txtConfirmPassword = new TextBox { UseSystemPasswordChar = true };
        layout.Controls.Add(_txtConfirmPassword, 0, 7);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var btnSave = new Button { Text = "Сменить пароль", Width = 140 };
        var btnCancel = new Button { Text = "Отмена", Width = 120 };
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);
        layout.Controls.Add(buttons, 0, 9);

        Controls.Add(layout);

        btnSave.Click += async (_, _) => await RecoverAsync();
        btnCancel.Click += (_, _) => Close();
    }

    private async Task RecoverAsync()
    {
        var result = await _authService.RecoverPasswordAsync(
            _txtUsername.Text,
            _txtSecretWord.Text,
            _txtNewPassword.Text,
            _txtConfirmPassword.Text);

        MessageBox.Show(this, result.Message, result.Success ? "Успех" : "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);

        if (result.Success)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
