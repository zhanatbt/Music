using Microsoft.EntityFrameworkCore;

namespace MusicApp.Infrastructure.Data;

public static class AppDbContextFactory
{
    public static AppDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
