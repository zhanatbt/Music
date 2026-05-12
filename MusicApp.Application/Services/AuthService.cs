using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IPasswordValidator passwordValidator)
{
    public async Task<OperationResult<UserSessionDto>> LoginAsync(string username, string password,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return OperationResult<UserSessionDto>.Fail("Введите логин и пароль.");

        var user = await userRepository.GetByUsernameAsync(username.Trim(), ct);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            return OperationResult<UserSessionDto>.Fail("Неверный логин или пароль.");

        if (user.IsBlocked)
            return OperationResult<UserSessionDto>.Fail("Ваш аккаунт заблокирован. Обратитесь к администратору.");

        return OperationResult<UserSessionDto>.Ok(new UserSessionDto
        {
            UserId = user.Id, Username = user.Username, Role = user.Role
        });
    }

    public async Task<OperationResult> RegisterAsync(string username, string password, string confirmPassword,
        string secretWord, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return OperationResult.Fail("Введите логин.");
        if (password != confirmPassword) return OperationResult.Fail("Пароли не совпадают.");

        var passwordValidation = passwordValidator.Validate(password);
        if (!passwordValidation.Success) return passwordValidation;

        if (await userRepository.GetByUsernameAsync(username.Trim(), ct) is not null)
            return OperationResult.Fail("Пользователь с таким логином уже существует.");

        await userRepository.AddAsync(new User
        {
            Username = username.Trim(),
            PasswordHash = passwordHasher.Hash(password),
            SecretWordHash = passwordHasher.Hash(string.IsNullOrWhiteSpace(secretWord) ? username : secretWord)
        }, ct);

        return OperationResult.Ok("Регистрация прошла успешно.");
    }

    public async Task<OperationResult> RecoverPasswordAsync(string username, string secretWord, string newPassword,
        string confirmPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return OperationResult.Fail("Введите логин.");

        var user = await userRepository.GetByUsernameAsync(username.Trim(), ct);
        if (user is null) return OperationResult.Fail("Пользователь не найден.");

        if (!passwordHasher.Verify(secretWord, user.SecretWordHash))
            return OperationResult.Fail("Неверное секретное слово.");

        if (newPassword != confirmPassword) return OperationResult.Fail("Пароли не совпадают.");

        var passwordValidation = passwordValidator.Validate(newPassword);
        if (!passwordValidation.Success) return passwordValidation;

        user.PasswordHash = passwordHasher.Hash(newPassword);
        await userRepository.UpdateAsync(user, ct);
        return OperationResult.Ok("Пароль успешно изменён.");
    }
}