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
                    new() { Username = "admin", Password = "admin123", SecretWord = "admin", Role = UserRole.Admin },
                    new() { Username = "user", Password = "user123", SecretWord = "user", Role = UserRole.User }
                };

            foreach (var seedUser in users)
            {
                await userRepository.AddAsync(new User
                {
                    Username = seedUser.Username,
                    PasswordHash = hasher.Hash(seedUser.Password),
                    SecretWordHash = hasher.Hash(string.IsNullOrWhiteSpace(seedUser.SecretWord) ? seedUser.Username : seedUser.SecretWord),
                    Role = seedUser.Role
                }, cancellationToken);
            }
        }
        else
        {
            var hasher = new BCryptPasswordHasher();
            foreach (var existingUser in existingUsers.Where(x => string.IsNullOrWhiteSpace(x.SecretWordHash)))
            {
                existingUser.SecretWordHash = hasher.Hash(existingUser.Username);
                await userRepository.UpdateAsync(existingUser, cancellationToken);
            }
        }

        var existingGenres = await genreRepository.GetAllAsync(cancellationToken);
        if (existingGenres.Count == 0)
        {
            var genres = seedData.Genres.Count > 0
                ? seedData.Genres
                : ["Поп", "Рок", "Хип-хоп", "Джаз"];

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
                : ["Топ 100", "Отдых", "Работа"];

            foreach (var categoryName in categories)
            {
                await categoryRepository.AddAsync(new Category { Name = categoryName }, cancellationToken);
            }
        }
    }
}
