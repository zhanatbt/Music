using MusicApp.Application.Services;

namespace MusicApp.UI.Presenters;

public class RegisterPresenter
{
    private readonly IRegisterView _view;
    private readonly AuthService _authService;

    public RegisterPresenter(IRegisterView view, AuthService authService)
    {
        _view = view;
        _authService = authService;
    }

    public async Task RegisterAsync()
    {
        var result = await _authService.RegisterAsync(
            _view.Username,
            _view.Password,
            _view.ConfirmPassword,
            _view.RegisterAsAdmin);

        _view.ShowMessage(result.Message, result.Success ? "Успех" : "Ошибка");

        if (result.Success)
        {
            _view.CloseView();
        }
    }
}
