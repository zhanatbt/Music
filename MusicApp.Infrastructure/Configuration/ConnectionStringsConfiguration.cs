namespace MusicApp.Infrastructure.Configuration;

public class ConnectionStringsConfiguration
{
    public string DefaultConnection { get; set; } =
        "Server=localhost\\SQLEXPRESS;Database=MusicPlayer;TrustServerCertificate=True;MultipleActiveResultSets=True;Trusted_Connection=True;";
}
