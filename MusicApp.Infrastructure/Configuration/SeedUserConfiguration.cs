using MusicApp.Domain.Common;

namespace MusicApp.Infrastructure.Configuration;

public class SeedUserConfiguration
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecretWord { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}
