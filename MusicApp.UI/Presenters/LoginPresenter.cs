using MusicApp.Application.Services;
using MusicApp.Domain.Common;

namespace MusicApp.UI.Presenters;

public class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly AuthService _authService;

    public LoginPresenter(ILoginView view, AuthService authService)
    {
        _view = view;
        _authService = authService;
    }

    public async Task LoginAsync()
    {
        var result = await _authService.LoginAsync(_view.Username, _view.Password);
        if (!result.Success || result.Data is null)
        {
            _view.ShowMessage(result.Message, "Ошибка входа");
            return;
        }

        if (result.Data.Role == UserRole.Admin)
        {
            _view.OpenAdmin(result.Data);
        }
        else
        {
            _view.OpenUser(result.Data);
        }
    }

    public void Register()
    {
        _view.OpenRegistration();
    }
}
