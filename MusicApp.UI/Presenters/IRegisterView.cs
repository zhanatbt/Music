namespace MusicApp.UI.Presenters;

public interface IRegisterView
{
    string Username { get; }
    string Password { get; }
    string ConfirmPassword { get; }
    void ShowMessage(string message, string title = "Music App");
    void CloseView();
}
