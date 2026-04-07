namespace MusicApp.Infrastructure.Configuration;

public class ConnectionStringsConfiguration
{
    public string DefaultConnection { get; set; } =
        "Server=(localdb)\\mssqllocaldb;Database=MusicAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
}
