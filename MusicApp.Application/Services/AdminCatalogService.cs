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

    public AdminCatalogService(
        IGenreRepository genreRepository,
        ICategoryRepository categoryRepository,
        ITrackRepository trackRepository,
        IArtistRepository artistRepository,
        IAlbumRepository albumRepository,
        IUserRepository userRepository,
        IMusicImportClient musicImportClient,
        IAudioMetadataReader audioMetadataReader)
    {
        _genreRepository = genreRepository;
        _categoryRepository = categoryRepository;
        _trackRepository = trackRepository;
        _artistRepository = artistRepository;
        _albumRepository = albumRepository;
        _userRepository = userRepository;
        _musicImportClient = musicImportClient;
        _audioMetadataReader = audioMetadataReader;
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
        return tracks.Select(TrackMapper.ToDto).ToList();
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
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ArtistName))
        {
            return OperationResult.Fail("Для трека нужны название и исполнитель.");
        }

        var genre = await ResolveGenreAsync(request, cancellationToken);
        if (genre is null)
        {
            return OperationResult.Fail("Выберите жанр или импортируйте тег жанра из mp3.");
        }

        Category? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                return OperationResult.Fail("Выбранная категория не найдена.");
            }
        }

        var artist = await _artistRepository.GetByNameAsync(request.ArtistName.Trim(), cancellationToken);
        if (artist is null)
        {
            artist = new Artist { Name = request.ArtistName.Trim() };
            await _artistRepository.AddAsync(artist, cancellationToken);
        }

        Album? album = null;
        if (!string.IsNullOrWhiteSpace(request.AlbumTitle))
        {
            album = await _albumRepository.GetByTitleAndArtistAsync(request.AlbumTitle.Trim(), artist.Id, cancellationToken);
            if (album is null)
            {
                album = new Album { Title = request.AlbumTitle.Trim(), ArtistId = artist.Id };
                await _albumRepository.AddAsync(album, cancellationToken);
            }
        }

        var duplicateTrack = await _trackRepository.FindDuplicateAsync(
            request.Title,
            artist.Id,
            album?.Id,
            request.DeezerId,
            cancellationToken);

        if (duplicateTrack is not null)
        {
            return OperationResult.Fail("Такой трек уже есть в каталоге.");
        }

        var playbackSource = !string.IsNullOrWhiteSpace(request.AudioFilePath)
            ? request.AudioFilePath
            : request.PreviewUrl;

        var track = new Track
        {
            Title = request.Title.Trim(),
            ArtistId = artist.Id,
            AlbumId = album?.Id,
            GenreId = genre.Id,
            CategoryId = category?.Id,
            DurationSeconds = request.DurationSeconds,
            DeezerId = request.DeezerId,
            PreviewUrl = playbackSource,
            SourceType = request.SourceType
        };

        await _trackRepository.AddAsync(track, cancellationToken);
        return OperationResult.Ok("Трек добавлен.");
    }

    public Task<IReadOnlyList<DeezerTrackDto>> SearchDeezerAsync(string query, CancellationToken cancellationToken = default)
    {
        return _musicImportClient.SearchAsync(query, cancellationToken);
    }

    private async Task<Genre?> ResolveGenreAsync(TrackCreateDto request, CancellationToken cancellationToken)
    {
        if (request.GenreId > 0)
        {
            var selectedGenre = await _genreRepository.GetByIdAsync(request.GenreId, cancellationToken);
            if (selectedGenre is not null)
            {
                return selectedGenre;
            }
        }

        if (string.IsNullOrWhiteSpace(request.GenreName))
        {
            return null;
        }

        var normalized = request.GenreName.Trim();
        var existing = await _genreRepository.GetByNameAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var genre = new Genre { Name = normalized };
        await _genreRepository.AddAsync(genre, cancellationToken);
        return genre;
    }
}
