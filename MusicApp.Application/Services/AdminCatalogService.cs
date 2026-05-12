using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Mappers;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Services;

public class AdminCatalogService
{
    private readonly IGenreRepository _genreRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IAudioMetadataReader _audioMetadataReader;
    private readonly IFileStorageService _fileStorageService;

    public AdminCatalogService(
        IGenreRepository genreRepository,
        ICategoryRepository categoryRepository,
        ITrackRepository trackRepository,
        IArtistRepository artistRepository,
        IAlbumRepository albumRepository,
        IUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IAudioMetadataReader audioMetadataReader,
        IFileStorageService fileStorageService)
    {
        _genreRepository = genreRepository;
        _categoryRepository = categoryRepository;
        _trackRepository = trackRepository;
        _artistRepository = artistRepository;
        _albumRepository = albumRepository;
        _userRepository = userRepository;
        _playlistRepository = playlistRepository;
        _audioMetadataReader = audioMetadataReader;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<GenreDto>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _genreRepository.GetAllAsync(cancellationToken);
        return genres.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<TrackDto>> GetTracksAsync(CancellationToken cancellationToken = default)
    {
        var tracks = await _trackRepository.GetAllAsync(cancellationToken);
        return NormalizeTrackPreviewUrls(tracks);
    }

    public async Task<IReadOnlyList<TrackDto>> SearchTracksAsync(
        string? album = null,
        string? genre = null,
        string? title = null,
        string? artist = null,
        int? genreId = null,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var tracks = await _trackRepository.SearchAsync(
            query: null,
            genreId: genreId,
            categoryId: categoryId,
            album: album,
            genre: genre,
            title: title,
            artist: artist,
            cancellationToken: cancellationToken);
        return NormalizeTrackPreviewUrls(tracks);
    }

    private IReadOnlyList<TrackDto> NormalizeTrackPreviewUrls(IReadOnlyList<Track> tracks)
    {
        var trackDtos = tracks.Select(TrackMapper.ToDto).ToList();
        foreach (var trackDto in trackDtos)
        {
            if (!string.IsNullOrWhiteSpace(trackDto.PreviewUrl))
            {
                trackDto.PreviewUrl = _fileStorageService.GetAbsolutePath(trackDto.PreviewUrl);
            }
        }

        return trackDtos;
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .Select(u => new UserSessionDto { UserId = u.Id, Username = u.Username, Role = u.Role })
            .ToList();
    }

    public async Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var playlists = await _playlistRepository.GetByUserIdAsync(userId, cancellationToken);
        return playlists.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<TrackDto>> GetPlaylistTracksAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var tracks = await _playlistRepository.GetTracksByPlaylistIdAsync(playlistId, cancellationToken);
        return NormalizeTrackPreviewUrls(tracks);
    }

    public async Task<IReadOnlyList<ArtistDto>> GetArtistsAsync(CancellationToken cancellationToken = default)
    {
        var artists = await _artistRepository.GetAllAsync(cancellationToken);
        return artists.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<OperationResult> AddArtistAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult.Fail("Имя исполнителя пустое.");
        }

        var normalized = name.Trim();
        if (await _artistRepository.GetByNameAsync(normalized, cancellationToken) is not null)
        {
            return OperationResult.Fail("Такой исполнитель уже существует.");
        }

        await _artistRepository.AddAsync(new Artist { Name = normalized }, cancellationToken);
        return OperationResult.Ok("Исполнитель добавлен.");
    }

    public async Task<OperationResult> DeleteGenreAsync(int genreId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _genreRepository.DeleteByIdAsync(genreId, cancellationToken);
            if (!deleted)
            {
                return OperationResult.Fail("Жанр не найден.");
            }

            return OperationResult.Ok("Жанр удален.");
        }
        catch
        {
            return OperationResult.Fail("Жанр нельзя удалить: он используется в треках.");
        }
    }

    public async Task<OperationResult> DeleteArtistAsync(int artistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _artistRepository.DeleteByIdAsync(artistId, cancellationToken);
            if (!deleted)
            {
                return OperationResult.Fail("Исполнитель не найден.");
            }

            return OperationResult.Ok("Исполнитель удален.");
        }
        catch
        {
            return OperationResult.Fail("Исполнителя нельзя удалить: он используется в треках или альбомах.");
        }
    }

    public async Task<OperationResult> AddGenreAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult.Fail("Название жанра пустое.");
        }

        var normalized = name.Trim();
        if (await _genreRepository.GetByNameAsync(normalized, cancellationToken) is not null)
        {
            return OperationResult.Fail("Такой жанр уже существует.");
        }

        await _genreRepository.AddAsync(new Genre { Name = normalized }, cancellationToken);
        return OperationResult.Ok("Жанр добавлен.");
    }

    public async Task<OperationResult> AddCategoryAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult.Fail("Название категории пустое.");
        }

        var normalized = name.Trim();
        if (await _categoryRepository.GetByNameAsync(normalized, cancellationToken) is not null)
        {
            return OperationResult.Fail("Такая категория уже существует.");
        }

        await _categoryRepository.AddAsync(new Category { Name = normalized }, cancellationToken);
        return OperationResult.Ok("Категория добавлена.");
    }

    public async Task<OperationResult> DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _categoryRepository.DeleteByIdAsync(categoryId, cancellationToken);
            if (!deleted)
            {
                return OperationResult.Fail("Категория не найдена.");
            }

            return OperationResult.Ok("Категория удалена.");
        }
        catch
        {
            return OperationResult.Fail("Категорию нельзя удалить: она используется в треках.");
        }
    }

    public async Task<AudioMetadataDto> ReadAudioMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await _audioMetadataReader.ReadAsync(filePath, cancellationToken);
    }

    public async Task<OperationResult> AddTrackAsync(TrackCreateDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return OperationResult.Fail("Для трека нужно название.");
        }

        var artistNames = GetDistinctNames(request.ArtistNames, request.ArtistName);
        if (!artistNames.Any())
        {
            return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");
        }

        var artists = await ResolveArtistsAsync(artistNames, cancellationToken);
        if (!artists.Any())
        {
            return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");
        }

        var genres = await ResolveGenresAsync(request, cancellationToken);

        var category = await ResolveCategoryAsync(request, cancellationToken);
        if (category is null && request.CategoryId.HasValue)
        {
            return OperationResult.Fail("Выбранная категория не найдена.");
        }

        var primaryArtist = artists[0];

        Album? album = null;
        if (!string.IsNullOrWhiteSpace(request.AlbumTitle))
        {
            var normalizedAlbumTitle = request.AlbumTitle.Trim();
            album = await _albumRepository.GetByTitleAndArtistAsync(normalizedAlbumTitle, primaryArtist.Id, cancellationToken);
            if (album is null)
            {
                album = new Album { Title = normalizedAlbumTitle, ArtistId = primaryArtist.Id };
                await _albumRepository.AddAsync(album, cancellationToken);
            }
        }

        var duplicateTrack = await _trackRepository.FindDuplicateAsync(
            request.Title,
            primaryArtist.Id,
            album?.Id,
            null,
            cancellationToken);

        if (duplicateTrack is not null)
        {
            return OperationResult.Fail("Такой трек уже есть в каталоге.");
        }

        string? playbackSource = null;

        if (!string.IsNullOrWhiteSpace(request.AudioFilePath))
        {
            playbackSource = await _fileStorageService.SaveAsync(request.AudioFilePath, cancellationToken);
        }

        var track = new Track
        {
            Title = request.Title.Trim(),
            AlbumId = album?.Id,
            CategoryId = category?.Id,
            DurationSeconds = request.DurationSeconds,
            PreviewUrl = playbackSource,
            TrackArtists = artists
                .Select(artist => new TrackArtist { ArtistId = artist.Id })
                .ToList(),
            TrackGenres = genres
                .Select(genre => new TrackGenre { GenreId = genre.Id })
                .ToList()
        };

        await _trackRepository.AddAsync(track, cancellationToken);
        return OperationResult.Ok("Трек добавлен.");
    }

    public async Task<OperationResult> UpdateTrackAsync(int trackId, TrackCreateDto request, CancellationToken cancellationToken = default)
    {
        var track = await _trackRepository.GetByIdAsync(trackId, cancellationToken);
        if (track is null)
        {
            return OperationResult.Fail("Трек не найден.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return OperationResult.Fail("Для трека нужно название.");
        }

        var artistNames = GetDistinctNames(request.ArtistNames, request.ArtistName);
        if (!artistNames.Any())
        {
            return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");
        }

        var artists = await ResolveArtistsAsync(artistNames, cancellationToken);
        if (!artists.Any())
        {
            return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");
        }

        var genres = await ResolveGenresAsync(request, cancellationToken);

        var category = await ResolveCategoryAsync(request, cancellationToken);
        if (category is null && request.CategoryId.HasValue)
        {
            return OperationResult.Fail("Выбранная категория не найдена.");
        }

        var primaryArtist = artists[0];
        Album? album = null;
        if (!string.IsNullOrWhiteSpace(request.AlbumTitle))
        {
            var normalizedAlbumTitle = request.AlbumTitle.Trim();
            album = await _albumRepository.GetByTitleAndArtistAsync(normalizedAlbumTitle, primaryArtist.Id, cancellationToken);
            if (album is null)
            {
                album = new Album { Title = normalizedAlbumTitle, ArtistId = primaryArtist.Id };
                await _albumRepository.AddAsync(album, cancellationToken);
            }
        }

        var duplicateTrack = await _trackRepository.FindDuplicateAsync(
            request.Title,
            primaryArtist.Id,
            album?.Id,
            track.Id,
            cancellationToken);

        if (duplicateTrack is not null)
        {
            return OperationResult.Fail("Такой трек уже есть в каталоге.");
        }

        if (!string.IsNullOrWhiteSpace(request.AudioFilePath))
        {
            track.PreviewUrl = await _fileStorageService.SaveAsync(request.AudioFilePath, cancellationToken);
        }

        track.Title = request.Title.Trim();
        track.AlbumId = album?.Id;
        track.Album = album;
        track.CategoryId = category?.Id;
        track.Category = category;
        track.DurationSeconds = request.DurationSeconds;
        SyncTrackArtists(track, artists);
        SyncTrackGenres(track, genres);

        await _trackRepository.UpdateAsync(track, cancellationToken);
        return OperationResult.Ok("Трек обновлен.");
    }

    public async Task<OperationResult> DeleteTrackAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var track = await _trackRepository.GetByIdAsync(trackId, cancellationToken);
        if (track is null)
        {
            return OperationResult.Fail("Трек не найден.");
        }

        await _trackRepository.DeleteAsync(track, cancellationToken);
        return OperationResult.Ok("Трек удален.");
    }

    private async Task<Category?> ResolveCategoryAsync(TrackCreateDto request, CancellationToken cancellationToken)
    {
        if (request.CategoryId.HasValue)
        {
            return await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return null;
        }

        var normalized = request.CategoryName.Trim();
        var existing = await _categoryRepository.GetByNameAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var category = new Category { Name = normalized };
        await _categoryRepository.AddAsync(category, cancellationToken);
        return category;
    }

    private async Task<List<Artist>> ResolveArtistsAsync(IEnumerable<string> artistNames, CancellationToken cancellationToken)
    {
        var artists = new List<Artist>();
        foreach (var artistName in artistNames)
        {
            var artist = await _artistRepository.GetByNameAsync(artistName, cancellationToken);
            if (artist is null)
            {
                artist = new Artist { Name = artistName };
                await _artistRepository.AddAsync(artist, cancellationToken);
            }

            artists.Add(artist);
        }

        return artists
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .ToList();
    }

    private async Task<List<Genre>> ResolveGenresAsync(TrackCreateDto request, CancellationToken cancellationToken)
    {
        var genres = new List<Genre>();

        if (request.GenreId > 0)
        {
            var selectedGenre = await _genreRepository.GetByIdAsync(request.GenreId, cancellationToken);
            if (selectedGenre is not null)
            {
                genres.Add(selectedGenre);
            }
        }

        var genreNames = GetDistinctNames(request.GenreNames, request.GenreName);
        foreach (var genreName in genreNames)
        {
            var existing = await _genreRepository.GetByNameAsync(genreName, cancellationToken);
            if (existing is not null)
            {
                genres.Add(existing);
                continue;
            }

            var genre = new Genre { Name = genreName };
            await _genreRepository.AddAsync(genre, cancellationToken);
            genres.Add(genre);
        }

        return genres
            .GroupBy(g => g.Id)
            .Select(g => g.First())
            .ToList();
    }

    private static void SyncTrackArtists(Track track, IReadOnlyCollection<Artist> artists)
    {
        var targetIds = artists.Select(x => x.Id).ToHashSet();
        var toRemove = track.TrackArtists.Where(x => !targetIds.Contains(x.ArtistId)).ToList();
        foreach (var item in toRemove)
        {
            track.TrackArtists.Remove(item);
        }

        var existingIds = track.TrackArtists.Select(x => x.ArtistId).ToHashSet();
        foreach (var artist in artists.Where(x => !existingIds.Contains(x.Id)))
        {
            track.TrackArtists.Add(new TrackArtist { TrackId = track.Id, ArtistId = artist.Id });
        }
    }

    private static void SyncTrackGenres(Track track, IReadOnlyCollection<Genre> genres)
    {
        var targetIds = genres.Select(x => x.Id).ToHashSet();
        var toRemove = track.TrackGenres.Where(x => !targetIds.Contains(x.GenreId)).ToList();
        foreach (var item in toRemove)
        {
            track.TrackGenres.Remove(item);
        }

        var existingIds = track.TrackGenres.Select(x => x.GenreId).ToHashSet();
        foreach (var genre in genres.Where(x => !existingIds.Contains(x.Id)))
        {
            track.TrackGenres.Add(new TrackGenre { TrackId = track.Id, GenreId = genre.Id });
        }
    }

    private static List<string> GetDistinctNames(IEnumerable<string>? values, string? fallbackValue)
    {
        var result = new List<string>();

        if (values is not null)
        {
            result.AddRange(values);
        }

        if (!string.IsNullOrWhiteSpace(fallbackValue))
        {
            result.AddRange(SplitNames(fallbackValue));
        }

        return result
            .Select(static name => name.Trim())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<string> SplitNames(string rawValue)
    {
        return rawValue.Split([',', ';', '|', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
