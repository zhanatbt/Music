using Microsoft.Extensions.Configuration;
using MusicApp.Application.Services;
using MusicApp.Application.Security;
using MusicApp.Infrastructure.Configuration;
using MusicApp.Infrastructure.Data;
using MusicApp.Infrastructure.External;
using MusicApp.Infrastructure.Repositories;
using MusicApp.Infrastructure.Security;
using MusicApp.Infrastructure.Storage;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? appConfiguration.ConnectionStrings.DefaultConnection;

        using var context = AppDbContextFactory.Create(connectionString);
        var genreRepository = new GenreRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var userRepository = new UserRepository(context);
        DatabaseInitializer.SeedAsync(
            context,
            userRepository,
            genreRepository,
            categoryRepository,
            appConfiguration.SeedData).GetAwaiter().GetResult();

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

        var fileStorageService = new LocalFileStorageService(httpClient, AppContext.BaseDirectory);
        var authService = new AuthService(userRepository, passwordHasher, passwordValidator);
        var adminCatalogService = new AdminCatalogService(
            genreRepository,
            categoryRepository,
            trackRepository,
            artistRepository,
            albumRepository,
            userRepository,
            playlistRepository,
            new DeezerClient(httpClient),
            new TagLibAudioMetadataReader(),
            fileStorageService);
        var musicLibraryService = new MusicLibraryService(trackRepository, playlistRepository, fileStorageService);

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
