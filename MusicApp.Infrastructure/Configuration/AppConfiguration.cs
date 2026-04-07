namespace MusicApp.Infrastructure.Configuration;

public class AppConfiguration
{
    public ConnectionStringsConfiguration ConnectionStrings { get; set; } = new();
    public SeedDataConfiguration SeedData { get; set; } = new();
}
