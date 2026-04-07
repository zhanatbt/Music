using Microsoft.Extensions.Configuration;
using MusicApp.Application.Services;
using MusicApp.Application.Security;
using MusicApp.Infrastructure.Configuration;
using MusicApp.Infrastructure.Data;
using MusicApp.Infrastructure.External;
using MusicApp.Infrastructure.Repositories;
using MusicApp.Infrastructure.Security;
using MusicApp.UI.Forms;

namespace MusicApp.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = BuildConfiguration();
        var appConfiguration = configuration.Get<AppConfiguration>() ?? new AppConfiguration();
        var connectionString = appConfiguration.ConnectionStrings.DefaultConnection;

        using var context = AppDbContextFactory.Create(connectionString);
        DatabaseInitializer.SeedAsync(context, appConfiguration.SeedData).GetAwaiter().GetResult();

        var userRepository = new UserRepository(context);
        var genreRepository = new GenreRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var artistRepository = new ArtistRepository(context);
        var albumRepository = new AlbumRepository(context);
        var trackRepository = new TrackRepository(context);
        var playlistRepository = new PlaylistRepository(context);

        var passwordHasher = new BCryptPasswordHasher();
        var passwordValidator = new PasswordValidator();
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.deezer.com/")
        };

        var authService = new AuthService(userRepository, passwordHasher, passwordValidator);
        var adminCatalogService = new AdminCatalogService(
            genreRepository,
            categoryRepository,
            trackRepository,
            artistRepository,
            albumRepository,
            userRepository,
            new DeezerClient(httpClient),
            new TagLibAudioMetadataReader());
        var musicLibraryService = new MusicLibraryService(trackRepository, playlistRepository);

        System.Windows.Forms.Application.Run(new LoginForm(authService, adminCatalogService, musicLibraryService));
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }
}
