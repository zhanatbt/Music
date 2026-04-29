using MusicApp.Domain.Common;

namespace MusicApp.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string SecretWordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
