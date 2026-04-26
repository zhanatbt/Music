using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Infrastructure.External;

public class DeezerClient : IMusicImportClient
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<long, string> _genreCache = new();

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
                ArtistNames = string.IsNullOrWhiteSpace(item.Artist?.Name)
                    ? Array.Empty<string>()
                    : new[] { item.Artist!.Name! },
                AlbumTitle = item.Album?.Title ?? string.Empty,
                PreviewUrl = item.Preview,
                DurationSeconds = item.Duration
            })
            .ToList();

        if (tracks is null)
        {
            return Array.Empty<DeezerTrackDto>();
        }

        var detailTasks = tracks.Select(track => EnrichTrackAsync(track, cancellationToken)).ToArray();
        await Task.WhenAll(detailTasks);

        return tracks;
    }

    private async Task EnrichTrackAsync(DeezerTrackDto track, CancellationToken cancellationToken)
    {
        try
        {
            if (!long.TryParse(track.DeezerId, out var id))
            {
                return;
            }

            var details = await _httpClient.GetFromJsonAsync<DeezerTrackDetailsResponse>(
                $"track/{id}", cancellationToken);
            if (details is null)
            {
                return;
            }

            if (details.Contributors?.Any() == true)
            {
                var contributors = details.Contributors
                    .Select(c => c.Name?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();

                if (contributors.Count > 0)
                {
                    track.ArtistNames = contributors;
                    track.ArtistName = string.Join(", ", contributors);
                }
            }

            if (!string.IsNullOrWhiteSpace(details.GenreName))
            {
                track.GenreName = details.GenreName;
                track.GenreNames = new[] { details.GenreName.Trim() };
                return;
            }

            if (details.Genres?.Data?.Any() == true)
            {
                var genres = details.Genres.Data
                    .Select(g => g.Name?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();

                if (genres.Count > 0)
                {
                    track.GenreNames = genres;
                    track.GenreName = string.Join(", ", genres);
                    return;
                }
            }

            if (details.GenreId > 0)
            {
                var genreName = await GetGenreNameAsync(details.GenreId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(genreName))
                {
                    track.GenreName = genreName;
                    track.GenreNames = new[] { genreName };
                }
            }
        }
        catch
        {
            // Ignore individual track enrichment failures.
        }
    }

    private async Task<string?> GetGenreNameAsync(long genreId, CancellationToken cancellationToken)
    {
        if (genreId <= 0)
        {
            return null;
        }

        if (_genreCache.TryGetValue(genreId, out var cached))
        {
            return cached;
        }

        var response = await _httpClient.GetFromJsonAsync<DeezerGenreResponse>($"genre/{genreId}", cancellationToken);
        if (response is null || string.IsNullOrWhiteSpace(response.Name))
        {
            return null;
        }

        _genreCache[genreId] = response.Name;
        return response.Name;
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

    private sealed class DeezerTrackDetailsResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("genre_id")]
        public long GenreId { get; init; }

        [JsonPropertyName("genres")]
        public DeezerGenreCollection? Genres { get; init; }

        [JsonPropertyName("genre")]
        public DeezerGenreItem? Genre { get; init; }

        [JsonPropertyName("artist")]
        public DeezerArtistItem? Artist { get; init; }

        [JsonPropertyName("album")]
        public DeezerAlbumItem? Album { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("preview")]
        public string? Preview { get; init; }

        [JsonPropertyName("duration")]
        public int Duration { get; init; }

        [JsonPropertyName("genre_name")]
        public string? GenreName { get; init; }

        [JsonPropertyName("contributors")]
        public List<DeezerArtistItem>? Contributors { get; init; }
    }

    private sealed class DeezerGenreCollection
    {
        [JsonPropertyName("data")]
        public List<DeezerGenreItem>? Data { get; init; }
    }

    private sealed class DeezerGenreItem
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed class DeezerGenreResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
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
