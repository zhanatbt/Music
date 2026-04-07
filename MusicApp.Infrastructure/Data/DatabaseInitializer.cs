using MusicApp.Domain.Common;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Configuration;
using MusicApp.Infrastructure.Security;

namespace MusicApp.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(
        AppDbContext context,
        SeedDataConfiguration? seedData = null,
        CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);
        seedData ??= new SeedDataConfiguration();

        if (!context.Users.Any())
        {
            var hasher = new BCryptPasswordHasher();

            var users = seedData.Users.Count > 0
                ? seedData.Users
                : new List<SeedUserConfiguration>
                {
                    new() { Username = "admin", Password = "admin123", Role = UserRole.Admin },
                    new() { Username = "user", Password = "user123", Role = UserRole.User }
                };

            foreach (var seedUser in users)
            {
                context.Users.Add(new User
                {
                    Username = seedUser.Username,
                    PasswordHash = hasher.Hash(seedUser.Password),
                    Role = seedUser.Role
                });
            }
        }

        if (!context.Genres.Any())
        {
            var genres = seedData.Genres.Count > 0
                ? seedData.Genres
                : ["Pop", "Rock", "Hip-Hop", "Electronic"];

            context.Genres.AddRange(genres.Select(name => new Genre { Name = name }));
        }

        if (!context.Categories.Any())
        {
            var categories = seedData.Categories.Count > 0
                ? seedData.Categories
                : ["Top Hits", "Chill", "Workout"];

            context.Categories.AddRange(categories.Select(name => new Category { Name = name }));
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
