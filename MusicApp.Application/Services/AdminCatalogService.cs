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
    private readonly IMusicImportClient _musicImportClient;
    private readonly IAudioMetadataReader _audioMetadataReader;
    private readonly IFileStorageService _fileStorageService;

    public AdminCatalogService(
        IGenreRepository genreRepository,
        ICategoryRepository categoryRepository,
        ITrackRepository trackRepository,
        IArtistRepository artistRepository,
        IAlbumRepository albumRepository,
        IUserRepository userRepository,
        IMusicImportClient musicImportClient,
        IAudioMetadataReader audioMetadataReader,
        IFileStorageService fileStorageService)
    {
        _genreRepository = genreRepository;
        _categoryRepository = categoryRepository;
        _trackRepository = trackRepository;
        _artistRepository = artistRepository;
        _albumRepository = albumRepository;
        _userRepository = userRepository;
        _musicImportClient = musicImportClient;
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
        if (!genres.Any())
        {
            return OperationResult.Fail("Выберите жанр или импортируйте тег жанра из mp3.");
        }

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
            request.DeezerId,
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
        else if (!string.IsNullOrWhiteSpace(request.PreviewUrl))
        {
            playbackSource = await _fileStorageService.SaveFromUrlAsync(request.PreviewUrl, cancellationToken);
        }

        var track = new Track
        {
            Title = request.Title.Trim(),
            AlbumId = album?.Id,
            CategoryId = category?.Id,
            DurationSeconds = request.DurationSeconds,
            DeezerId = request.DeezerId,
            PreviewUrl = playbackSource,
            SourceType = request.SourceType,
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

    public Task<IReadOnlyList<DeezerTrackDto>> SearchDeezerAsync(string query, CancellationToken cancellationToken = default)
    {
        return _musicImportClient.SearchAsync(query, cancellationToken);
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
