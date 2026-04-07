using MusicApp.Domain.Common;

namespace MusicApp.Infrastructure.Configuration;

public class SeedDataConfiguration
{
    public List<SeedUserConfiguration> Users { get; set; } = [];
    public List<string> Genres { get; set; } = [];
    public List<string> Categories { get; set; } = [];
}
