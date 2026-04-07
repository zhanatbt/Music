using MusicApp.Application.Services;
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
        var service = new AuthService(repository, hasher);

        var result = await service.RegisterAsync("alice", "password1", "password1");

        Assert.True(result.Success);
        var stored = await repository.GetByUsernameAsync("alice");
        Assert.NotNull(stored);
        Assert.NotEqual("password1", stored!.PasswordHash);
        Assert.True(hasher.Verify("password1", stored.PasswordHash));
    }

    [Fact]
    public async Task Login_Should_Return_Admin_Session()
    {
        var repository = new FakeUserRepository();
        var hasher = new BCryptPasswordHasher();
        await repository.AddAsync(new User
        {
            Username = "admin",
            PasswordHash = hasher.Hash("admin123"),
            Role = UserRole.Admin
        });

        var service = new AuthService(repository, hasher);
        var result = await service.LoginAsync("admin", "admin123");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(UserRole.Admin, result.Data!.Role);
    }
}
