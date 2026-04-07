using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Infrastructure.External;

public class DeezerClient : IMusicImportClient
{
    private readonly HttpClient _httpClient;

    public DeezerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeezerTrackDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<DeezerTrackDto>();
        }

        var response = await _httpClient.GetFromJsonAsync<DeezerSearchResponse>(
            $"search?q={Uri.EscapeDataString(query.Trim())}",
            cancellationToken);

        var tracks = response?.Data?
            .Select(item => new DeezerTrackDto
            {
                DeezerId = item.Id.ToString(),
                Title = item.Title ?? string.Empty,
                ArtistName = item.Artist?.Name ?? string.Empty,
                AlbumTitle = item.Album?.Title ?? string.Empty,
                PreviewUrl = item.Preview,
                DurationSeconds = item.Duration
            })
            .ToList();

        if (tracks is null)
        {
            return Array.Empty<DeezerTrackDto>();
        }

        return tracks;
    }

    private sealed class DeezerSearchResponse
    {
        [JsonPropertyName("data")]
        public List<DeezerTrackItem>? Data { get; init; }
    }

    private sealed class DeezerTrackItem
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("preview")]
        public string? Preview { get; init; }

        [JsonPropertyName("duration")]
        public int Duration { get; init; }

        [JsonPropertyName("artist")]
        public DeezerArtistItem? Artist { get; init; }

        [JsonPropertyName("album")]
        public DeezerAlbumItem? Album { get; init; }
    }

    private sealed class DeezerArtistItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed class DeezerAlbumItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; }
    }
}
