using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Common;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Configuration;
using MusicApp.Infrastructure.Security;

namespace MusicApp.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(
        AppDbContext context,
        IUserRepository userRepository,
        IGenreRepository genreRepository,
        ICategoryRepository categoryRepository,
        SeedDataConfiguration? seedData = null,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        seedData ??= new SeedDataConfiguration();

        var existingUsers = await userRepository.GetAllAsync(cancellationToken);
        if (existingUsers.Count == 0)
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
                await userRepository.AddAsync(new User
                {
                    Username = seedUser.Username,
                    PasswordHash = hasher.Hash(seedUser.Password),
                    Role = seedUser.Role
                }, cancellationToken);
            }
        }

        var existingGenres = await genreRepository.GetAllAsync(cancellationToken);
        if (existingGenres.Count == 0)
        {
            var genres = seedData.Genres.Count > 0
                ? seedData.Genres
                : ["Pop", "Rock", "Hip-Hop", "Electronic"];

            foreach (var genreName in genres)
            {
                await genreRepository.AddAsync(new Genre { Name = genreName }, cancellationToken);
            }
        }

        var existingCategories = await categoryRepository.GetAllAsync(cancellationToken);
        if (existingCategories.Count == 0)
        {
            var categories = seedData.Categories.Count > 0
                ? seedData.Categories
                : ["Top Hits", "Chill", "Workout"];

            foreach (var categoryName in categories)
            {
                await categoryRepository.AddAsync(new Category { Name = categoryName }, cancellationToken);
            }
        }
    }
}
