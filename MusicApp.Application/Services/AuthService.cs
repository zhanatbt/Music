using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Common;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordValidator _passwordValidator;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IPasswordValidator passwordValidator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
    }

    public async Task<OperationResult<UserSessionDto>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return OperationResult<UserSessionDto>.Fail("Введите логин и пароль.");
        }

        var user = await _userRepository.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return OperationResult<UserSessionDto>.Fail("Неверный логин или пароль.");
        }

        return OperationResult<UserSessionDto>.Ok(new UserSessionDto
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        });
    }

    public async Task<OperationResult> RegisterAsync(string username, string password, string confirmPassword, string secretWord, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return OperationResult.Fail("Имя пользователя не может быть пустым.");
        }

        var passwordValidation = _passwordValidator.Validate(password);
        if (!passwordValidation.Success)
        {
            return passwordValidation;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return OperationResult.Fail("Пароли не совпадают.");
        }

        if (string.IsNullOrWhiteSpace(secretWord))
        {
            return OperationResult.Fail("Кодовое слово не может быть пустым.");
        }

        var normalized = username.Trim();
        var existing = await _userRepository.GetByUsernameAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            return OperationResult.Fail("Пользователь с таким логином уже существует.");
        }

        var user = new User
        {
            Username = normalized,
            PasswordHash = _passwordHasher.Hash(password),
            SecretWordHash = _passwordHasher.Hash(secretWord.Trim()),
            Role = UserRole.User
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return OperationResult.Ok("Регистрация выполнена.");
    }

    public async Task<OperationResult> RecoverPasswordAsync(
        string username,
        string secretWord,
        string newPassword,
        string confirmNewPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(secretWord))
        {
            return OperationResult.Fail("Введите логин и кодовое слово.");
        }

        var passwordValidation = _passwordValidator.Validate(newPassword);
        if (!passwordValidation.Success)
        {
            return passwordValidation;
        }

        if (!string.Equals(newPassword, confirmNewPassword, StringComparison.Ordinal))
        {
            return OperationResult.Fail("Новые пароли не совпадают.");
        }

        var user = await _userRepository.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Пользователь не найден.");
        }

        if (string.IsNullOrWhiteSpace(user.SecretWordHash) || !_passwordHasher.Verify(secretWord.Trim(), user.SecretWordHash))
        {
            return OperationResult.Fail("Неверное кодовое слово.");
        }

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);
        return OperationResult.Ok("Пароль успешно изменен.");
    }
}
