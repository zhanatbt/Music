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

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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

    public async Task<OperationResult> RegisterAsync(string username, string password, string confirmPassword, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return OperationResult.Fail("Имя пользователя не может быть пустым.");
        }

        if (password.Length < 6)
        {
            return OperationResult.Fail("Пароль должен содержать минимум 6 символов.");
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return OperationResult.Fail("Пароли не совпадают.");
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
            Role = isAdmin ? UserRole.Admin : UserRole.User
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return OperationResult.Ok("Регистрация выполнена.");
    }
}
