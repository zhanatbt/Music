using MusicApp.Domain.Common;

namespace MusicApp.Application.DTOs;

public class UserAdminDto
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsBlocked { get; init; }
}