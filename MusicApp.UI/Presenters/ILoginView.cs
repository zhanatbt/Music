using MusicApp.Application.DTOs;

namespace MusicApp.UI.Presenters;

public interface ILoginView
{
    string Username { get; }
    string Password { get; }
    void ShowMessage(string message, string title = "Music App");
    void OpenRegistration();
    void OpenAdmin(UserSessionDto session);
    void OpenUser(UserSessionDto session);
}
