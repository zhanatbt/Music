using MusicApp.Application.Services;
using MusicApp.Application.Security;
using MusicApp.Domain.Common;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Security;
using MusicApp.Tests.Fakes;

namespace MusicApp.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_Should_Create_User_With_Hash()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        var service = new AuthService(repository, hasher, new PasswordValidator());

        var result = await service.RegisterAsync("alice", "Password!", "Password!");

        Assert.True(result.Success);
        var stored = await repository.GetByUsernameAsync("alice");
        Assert.NotNull(stored);
        Assert.NotEqual("Password!", stored!.PasswordHash);
        Assert.True(hasher.Verify("Password!", stored.PasswordHash));
    }

    [Fact]
    public async Task Register_Should_Fail_When_Password_Has_No_Special_Character()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        var service = new AuthService(repository, hasher, new PasswordValidator());

        var result = await service.RegisterAsync("alice", "Password", "Password");

        Assert.False(result.Success);
        Assert.Contains("Хотя бы один специальный символ.", result.Message);
    }

    [Fact]
    public async Task Register_Should_Return_All_Missing_Password_Requirements()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        var service = new AuthService(repository, hasher, new PasswordValidator());

        var result = await service.RegisterAsync("alice", "пар", "пар");

        Assert.False(result.Success);
        Assert.Contains("Минимум 6 символов.", result.Message);
        Assert.Contains("Хотя бы одна заглавная буква.", result.Message);
        Assert.Contains("Хотя бы один специальный символ.", result.Message);
    }

    [Fact]
    public async Task Register_Should_Accept_Cyrillic_Password()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        var service = new AuthService(repository, hasher, new PasswordValidator());

        var result = await service.RegisterAsync("ivan", "Пароль!", "Пароль!");

        Assert.True(result.Success);
        var stored = await repository.GetByUsernameAsync("ivan");
        Assert.NotNull(stored);
        Assert.True(hasher.Verify("Пароль!", stored!.PasswordHash));
    }

    [Fact]
    public async Task Login_Should_Return_Admin_Session()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        await repository.AddAsync(new User
        {
            Username = "admin",
            PasswordHash = hasher.Hash("Admin123!"),
            Role = UserRole.Admin
        });

        var service = new AuthService(repository, hasher, new PasswordValidator());
        var result = await service.LoginAsync("admin", "Admin123!");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(UserRole.Admin, result.Data!.Role);
    }
}
