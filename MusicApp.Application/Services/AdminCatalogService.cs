using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Mappers;
using MusicApp.Domain.Common;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Services;

public class AdminCatalogService(
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
    public async Task<IReadOnlyList<GenreDto>> GetGenresAsync(CancellationToken ct = default)
        => (await genreRepository.GetAllAsync(ct)).Select(LookupMapper.ToDto).ToList();

    public async Task<OperationResult> AddGenreAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return OperationResult.Fail("Название жанра пустое.");
        var normalized = name.Trim();
        if (await genreRepository.GetByNameAsync(normalized, ct) is not null)
            return OperationResult.Fail("Такой жанр уже существует.");
        await genreRepository.AddAsync(new Genre { Name = normalized }, ct);
        return OperationResult.Ok("Жанр добавлен.");
    }

    public async Task<OperationResult> UpdateGenreAsync(int id, string newName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newName)) return OperationResult.Fail("Название не может быть пустым.");
        var genre = await genreRepository.GetByIdAsync(id, ct);
        if (genre is null) return OperationResult.Fail("Жанр не найден.");
        var normalized = newName.Trim();
        var existing = await genreRepository.GetByNameAsync(normalized, ct);
        if (existing is not null && existing.Id != id)
            return OperationResult.Fail("Жанр с таким названием уже существует.");
        genre.Name = normalized;
        await genreRepository.UpdateAsync(genre, ct);
        return OperationResult.Ok("Жанр переименован.");
    }

    public async Task<OperationResult> DeleteGenreAsync(int genreId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await genreRepository.DeleteByIdAsync(genreId, ct);
            return deleted ? OperationResult.Ok("Жанр удален.") : OperationResult.Fail("Жанр не найден.");
        }
        catch
        {
            return OperationResult.Fail("Жанр нельзя удалить: он используется в треках.");
        }
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
        => (await categoryRepository.GetAllAsync(ct)).Select(LookupMapper.ToDto).ToList();

    public async Task<OperationResult> AddCategoryAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return OperationResult.Fail("Название категории пустое.");
        var normalized = name.Trim();
        if (await categoryRepository.GetByNameAsync(normalized, ct) is not null)
            return OperationResult.Fail("Такая категория уже существует.");

        await categoryRepository.AddAsync(new Category { Name = normalized }, ct);
        return OperationResult.Ok("Категория добавлена.");
    }

    public async Task<OperationResult> UpdateCategoryAsync(int id, string newName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newName)) return OperationResult.Fail("Название не может быть пустым.");
        var category = await categoryRepository.GetByIdAsync(id, ct);
        if (category is null) return OperationResult.Fail("Категория не найдена.");
        var normalized = newName.Trim();
        var existing = await categoryRepository.GetByNameAsync(normalized, ct);
        if (existing is not null && existing.Id != id)
            return OperationResult.Fail("Категория с таким названием уже существует.");
        category.Name = normalized;
        await categoryRepository.UpdateAsync(category, ct);
        return OperationResult.Ok("Категория переименована.");
    }

    public async Task<OperationResult> DeleteCategoryAsync(int categoryId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await categoryRepository.DeleteByIdAsync(categoryId, ct);
            return deleted ? OperationResult.Ok("Категория удалена.") : OperationResult.Fail("Категория не найдена.");
        }
        catch
        {
            return OperationResult.Fail("Категорию нельзя удалить: она используется в треках.");
        }
    }

    public async Task<IReadOnlyList<ArtistDto>> GetArtistsAsync(CancellationToken ct = default)
        => (await artistRepository.GetAllAsync(ct)).Select(LookupMapper.ToDto).ToList();

    public async Task<OperationResult> AddArtistAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return OperationResult.Fail("Имя исполнителя пустое.");
        var normalized = name.Trim();
        if (await artistRepository.GetByNameAsync(normalized, ct) is not null)
            return OperationResult.Fail("Такой исполнитель уже существует.");
        await artistRepository.AddAsync(new Artist { Name = normalized }, ct);
        return OperationResult.Ok("Исполнитель добавлен.");
    }

    public async Task<OperationResult> UpdateArtistAsync(int id, string newName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newName)) return OperationResult.Fail("Имя не может быть пустым.");
        var artists = await artistRepository.GetAllAsync(ct);
        var artist = artists.FirstOrDefault(a => a.Id == id);
        if (artist is null) return OperationResult.Fail("Исполнитель не найден.");
        var normalized = newName.Trim();
        var existing = await artistRepository.GetByNameAsync(normalized, ct);
        if (existing is not null && existing.Id != id)
            return OperationResult.Fail("Исполнитель с таким именем уже существует.");
        artist.Name = normalized;
        await artistRepository.UpdateAsync(artist, ct);
        return OperationResult.Ok("Исполнитель переименован.");
    }

    public async Task<OperationResult> DeleteArtistAsync(int artistId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await artistRepository.DeleteByIdAsync(artistId, ct);
            return deleted ? OperationResult.Ok("Исполнитель удален.") : OperationResult.Fail("Исполнитель не найден.");
        }
        catch
        {
            return OperationResult.Fail("Исполнителя нельзя удалить: он используется в треках или альбомах.");
        }
    }

    public async Task<IReadOnlyList<AlbumDto>> GetAlbumsAsync(CancellationToken ct = default)
        => (await albumRepository.GetAllWithDetailsAsync(ct)).Select(LookupMapper.ToDto).ToList();

    public async Task<IReadOnlyList<TrackDto>> GetAlbumTracksAsync(int albumId, CancellationToken ct = default)
    {
        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return [];
        var tracks = album.TrackAlbums.Where(ta => ta.Track is not null).Select(ta => ta.Track!).ToList();
        return NormalizeTrackPreviewUrls(tracks);
    }

    public async Task<OperationResult> AddAlbumAsync(string title, IReadOnlyList<string> artistNames,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title)) return OperationResult.Fail("Введите название альбома.");
        if (!artistNames.Any()) return OperationResult.Fail("Выберите хотя бы одного исполнителя.");

        var artists = await ResolveArtistsAsync(artistNames, ct);
        if (artists.Count == 0) return OperationResult.Fail("Исполнители не найдены.");

        var primaryArtist = artists[0];
        var normalized = title.Trim();

        if (await albumRepository.GetByTitleAndArtistAsync(normalized, primaryArtist.Id, ct) is not null)
            return OperationResult.Fail("Альбом с таким названием и исполнителем уже существует.");

        var album = new Album
        {
            Title = normalized,
            ArtistId = primaryArtist.Id,
            AlbumArtists = artists.Select(a => new AlbumArtist { ArtistId = a.Id }).ToList()
        };
        await albumRepository.AddAsync(album, ct);
        return OperationResult.Ok("Альбом добавлен.");
    }

    public async Task<OperationResult> UpdateAlbumAsync(int albumId, string newTitle, IReadOnlyList<string> artistNames,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newTitle)) return OperationResult.Fail("Введите название альбома.");
        if (!artistNames.Any()) return OperationResult.Fail("Выберите хотя бы одного исполнителя.");

        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return OperationResult.Fail("Альбом не найден.");

        var artists = await ResolveArtistsAsync(artistNames, ct);
        if (artists.Count == 0) return OperationResult.Fail("Исполнители не найдены.");

        var normalized = newTitle.Trim();
        var primaryArtist = artists[0];

        var duplicate = await albumRepository.GetByTitleAndArtistAsync(normalized, primaryArtist.Id, ct);
        if (duplicate is not null && duplicate.Id != albumId)
            return OperationResult.Fail("Альбом с таким названием и исполнителем уже существует.");

        album.Title = normalized;
        album.ArtistId = primaryArtist.Id;

        var targetIds = artists.Select(a => a.Id).ToHashSet();
        var toRemove = album.AlbumArtists.Where(x => !targetIds.Contains(x.ArtistId)).ToList();
        foreach (var item in toRemove) album.AlbumArtists.Remove(item);
        var existingIds = album.AlbumArtists.Select(x => x.ArtistId).ToHashSet();
        foreach (var a in artists.Where(x => !existingIds.Contains(x.Id)))
            album.AlbumArtists.Add(new AlbumArtist { AlbumId = albumId, ArtistId = a.Id });

        await albumRepository.UpdateAsync(album, ct);
        return OperationResult.Ok("Альбом обновлён.");
    }

    public async Task<OperationResult> DeleteAlbumAsync(int albumId, CancellationToken ct = default)
    {
        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return OperationResult.Fail("Альбом не найден.");
        if (album.TrackAlbums.Count != 0)
            return OperationResult.Fail("Нельзя удалить альбом: сначала уберите все треки из альбома.");
        var deleted = await albumRepository.DeleteByIdAsync(albumId, ct);
        return deleted ? OperationResult.Ok("Альбом удалён.") : OperationResult.Fail("Альбом не найден.");
    }

    public async Task<OperationResult> AddTrackToAlbumAsync(int albumId, int trackId, CancellationToken ct = default)
    {
        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return OperationResult.Fail("Альбом не найден.");
        var track = await trackRepository.GetByIdAsync(trackId, ct);
        if (track is null) return OperationResult.Fail("Трек не найден.");
        if (album.TrackAlbums.Any(ta => ta.TrackId == trackId))
            return OperationResult.Fail("Этот трек уже есть в альбоме.");
        await albumRepository.AddTrackAsync(albumId, trackId, ct);
        return OperationResult.Ok("Трек добавлен в альбом.");
    }

    public async Task<OperationResult> RemoveTrackFromAlbumAsync(int albumId, int trackId,
        CancellationToken ct = default)
    {
        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return OperationResult.Fail("Альбом не найден.");
        if (album.TrackAlbums.All(ta => ta.TrackId != trackId))
            return OperationResult.Fail("Трек не принадлежит этому альбому.");
        await albumRepository.RemoveTrackAsync(albumId, trackId, ct);
        return OperationResult.Ok("Трек удалён из альбома.");
    }

    public async Task<IReadOnlyList<TrackDto>> GetTracksAsync(CancellationToken ct = default)
        => NormalizeTrackPreviewUrls(await trackRepository.GetAllAsync(ct));

    public async Task<IReadOnlyList<TrackDto>> SearchTracksAsync(
        string? album = null, string? genre = null, string? title = null, string? artist = null,
        int? genreId = null, int? categoryId = null, CancellationToken ct = default)
    {
        var tracks = await trackRepository.SearchAsync(null, genreId, categoryId, album, genre, title, artist, ct);
        return NormalizeTrackPreviewUrls(tracks);
    }

    public async Task<AudioMetadataDto> ReadAudioMetadataAsync(string filePath, CancellationToken ct = default)
        => await audioMetadataReader.ReadAsync(filePath, ct);

    public async Task<OperationResult> AddTrackAsync(TrackCreateDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return OperationResult.Fail("Для трека нужно название.");

        var artistNames = GetDistinctNames(request.ArtistNames, request.ArtistName);
        if (artistNames.Count == 0) return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");

        var artists = await ResolveArtistsAsync(artistNames, ct);
        if (artists.Count == 0) return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");

        var genres = await ResolveGenresAsync(request, ct);
        var category = await ResolveCategoryAsync(request, ct);
        if (category is null && request.CategoryId.HasValue)
            return OperationResult.Fail("Выбранная категория не найдена.");

        var primaryArtist = artists[0];

        var duplicate = await trackRepository.FindDuplicateAsync(request.Title, primaryArtist.Id, null, ct);
        if (duplicate is not null) return OperationResult.Fail("Такой трек уже есть в каталоге.");

        string? playbackSource = null;
        if (!string.IsNullOrWhiteSpace(request.AudioFilePath))
            playbackSource = await fileStorageService.SaveAsync(request.AudioFilePath, ct);

        var track = new Track
        {
            Title = request.Title.Trim(),
            CategoryId = category?.Id,
            DurationSeconds = request.DurationSeconds,
            AudioFilePath = playbackSource,
            TrackArtists = artists.Select(a => new TrackArtist { ArtistId = a.Id }).ToList(),
            TrackGenres = genres.Select(g => new TrackGenre { GenreId = g.Id }).ToList()
        };

        await trackRepository.AddAsync(track, ct);

        var albumErrors = new List<string>();
        foreach (var albumId in request.AlbumIds.Distinct())
        {
            var validationError = await ValidateTrackArtistsMatchAlbumAsync(albumId, artists, ct);
            if (validationError is not null)
            {
                albumErrors.Add(validationError);
                continue;
            }

            await albumRepository.AddTrackAsync(albumId, track.Id, ct);
        }

        var message = "Трек добавлен.";
        if (albumErrors.Count > 0)
            message += $" Не добавлен в некоторые альбомы: {string.Join("; ", albumErrors)}";

        return OperationResult.Ok(message);
    }

    public async Task<OperationResult> UpdateTrackAsync(int trackId, TrackCreateDto request,
        CancellationToken ct = default)
    {
        var track = await trackRepository.GetByIdAsync(trackId, ct);
        if (track is null) return OperationResult.Fail("Трек не найден.");
        if (string.IsNullOrWhiteSpace(request.Title)) return OperationResult.Fail("Для трека нужно название.");

        var artistNames = GetDistinctNames(request.ArtistNames, request.ArtistName);
        if (artistNames.Count == 0) return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");

        var artists = await ResolveArtistsAsync(artistNames, ct);
        if (artists.Count == 0) return OperationResult.Fail("Для трека нужен хотя бы один исполнитель.");

        var genres = await ResolveGenresAsync(request, ct);
        var category = await ResolveCategoryAsync(request, ct);
        if (category is null && request.CategoryId.HasValue)
            return OperationResult.Fail("Выбранная категория не найдена.");

        var primaryArtist = artists[0];

        var duplicate = await trackRepository.FindDuplicateAsync(request.Title, primaryArtist.Id, track.Id, ct);
        if (duplicate is not null) return OperationResult.Fail("Такой трек уже есть в каталоге.");

        if (!string.IsNullOrWhiteSpace(request.AudioFilePath))
            track.AudioFilePath = await fileStorageService.SaveAsync(request.AudioFilePath, ct);

        track.Title = request.Title.Trim();
        track.CategoryId = category?.Id;
        track.Category = category;
        track.DurationSeconds = request.DurationSeconds;
        SyncTrackArtists(track, artists);
        SyncTrackGenres(track, genres);

        var currentAlbumIds = track.TrackAlbums.Select(ta => ta.AlbumId).ToHashSet();
        var targetAlbumIds = request.AlbumIds.ToHashSet();

        foreach (var aId in currentAlbumIds.Except(targetAlbumIds))
            await albumRepository.RemoveTrackAsync(aId, track.Id, ct);

        var albumErrors = new List<string>();
        foreach (var aId in targetAlbumIds.Except(currentAlbumIds))
        {
            var validationError = await ValidateTrackArtistsMatchAlbumAsync(aId, artists, ct);
            if (validationError is not null)
            {
                albumErrors.Add(validationError);
                continue;
            }

            await albumRepository.AddTrackAsync(aId, track.Id, ct);
        }

        await trackRepository.UpdateAsync(track, ct);

        var message = "Трек обновлен.";
        if (albumErrors.Count > 0)
            message += $" Не добавлен в некоторые альбомы: {string.Join("; ", albumErrors)}";

        return OperationResult.Ok(message);
    }

    public async Task<OperationResult> DeleteTrackAsync(int trackId, CancellationToken ct = default)
    {
        var track = await trackRepository.GetByIdAsync(trackId, ct);
        if (track is null) return OperationResult.Fail("Трек не найден.");

        if (track.PlaylistTracks.Count != 0)
            return OperationResult.Fail("Нельзя удалить трек: он находится в плейлистах пользователей.");

        await trackRepository.DeleteAsync(track, ct);
        return OperationResult.Ok("Трек удален.");
    }

    public async Task<IReadOnlyList<UserAdminDto>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return users.Select(u => new UserAdminDto
        {
            UserId = u.Id,
            Username = u.Username,
            Role = u.Role,
            IsBlocked = u.IsBlocked
        }).ToList();
    }

    public async Task<OperationResult> BlockUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null) return OperationResult.Fail("Пользователь не найден.");
        if (user.Role == UserRole.Admin) return OperationResult.Fail("Нельзя заблокировать администратора.");
        if (user.IsBlocked) return OperationResult.Fail("Пользователь уже заблокирован.");
        user.IsBlocked = true;
        await userRepository.UpdateAsync(user, ct);
        return OperationResult.Ok($"Пользователь «{user.Username}» заблокирован.");
    }

    public async Task<OperationResult> UnblockUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null) return OperationResult.Fail("Пользователь не найден.");
        if (!user.IsBlocked) return OperationResult.Fail("Пользователь не заблокирован.");
        user.IsBlocked = false;
        await userRepository.UpdateAsync(user, ct);
        return OperationResult.Ok($"Пользователь «{user.Username}» разблокирован.");
    }

    public async Task<IReadOnlyList<PlaylistDto>> GetUserPlaylistsAsync(int userId, CancellationToken ct = default)
        => (await playlistRepository.GetByUserIdAsync(userId, ct)).Select(LookupMapper.ToDto).ToList();

    public async Task<IReadOnlyList<TrackDto>> GetPlaylistTracksAsync(int playlistId, CancellationToken ct = default)
        => NormalizeTrackPreviewUrls(await playlistRepository.GetTracksByPlaylistIdAsync(playlistId, ct));

    private IReadOnlyList<TrackDto> NormalizeTrackPreviewUrls(IReadOnlyList<Track> tracks)
    {
        var dtos = tracks.Select(TrackMapper.ToDto).ToList();
        foreach (var dto in dtos.Where(dto => !string.IsNullOrWhiteSpace(dto.PreviewUrl)))
            dto.PreviewUrl = fileStorageService.GetAbsolutePath(dto.PreviewUrl);
        return dtos;
    }

    private async Task<Category?> ResolveCategoryAsync(TrackCreateDto request, CancellationToken ct)
    {
        if (request.CategoryId.HasValue)
            return await categoryRepository.GetByIdAsync(request.CategoryId.Value, ct);

        if (string.IsNullOrWhiteSpace(request.CategoryName)) return null;

        var normalized = request.CategoryName.Trim();
        var existing = await categoryRepository.GetByNameAsync(normalized, ct);
        if (existing is not null) return existing;
        var category = new Category { Name = normalized };
        await categoryRepository.AddAsync(category, ct);
        return category;
    }

    private async Task<List<Artist>> ResolveArtistsAsync(IEnumerable<string> artistNames, CancellationToken ct)
    {
        var artists = new List<Artist>();
        foreach (var name in artistNames)
        {
            var artist = await artistRepository.GetByNameAsync(name, ct);
            if (artist is null)
            {
                artist = new Artist { Name = name };
                await artistRepository.AddAsync(artist, ct);
            }

            artists.Add(artist);
        }

        return artists.GroupBy(a => a.Id).Select(g => g.First()).ToList();
    }

    private async Task<List<Genre>> ResolveGenresAsync(TrackCreateDto request, CancellationToken ct)
    {
        var genres = new List<Genre>();
        if (request.GenreId > 0)
        {
            var g = await genreRepository.GetByIdAsync(request.GenreId, ct);
            if (g is not null) genres.Add(g);
        }

        foreach (var name in GetDistinctNames(request.GenreNames, request.GenreName))
        {
            var existing = await genreRepository.GetByNameAsync(name, ct);
            if (existing is not null)
            {
                genres.Add(existing);
                continue;
            }

            var genre = new Genre { Name = name };
            await genreRepository.AddAsync(genre, ct);
            genres.Add(genre);
        }

        return genres.GroupBy(g => g.Id).Select(g => g.First()).ToList();
    }

    private static void SyncTrackArtists(Track track, IReadOnlyCollection<Artist> artists)
    {
        var targetIds = artists.Select(x => x.Id).ToHashSet();
        foreach (var item in track.TrackArtists.Where(x => !targetIds.Contains(x.ArtistId)).ToList())
            track.TrackArtists.Remove(item);

        var existingIds = track.TrackArtists.Select(x => x.ArtistId).ToHashSet();
        foreach (var a in artists.Where(x => !existingIds.Contains(x.Id)))
            track.TrackArtists.Add(new TrackArtist { TrackId = track.Id, ArtistId = a.Id });
    }

    private static void SyncTrackGenres(Track track, IReadOnlyCollection<Genre> genres)
    {
        var targetIds = genres.Select(x => x.Id).ToHashSet();
        foreach (var item in track.TrackGenres.Where(x => !targetIds.Contains(x.GenreId)).ToList())
            track.TrackGenres.Remove(item);
        var existingIds = track.TrackGenres.Select(x => x.GenreId).ToHashSet();
        foreach (var g in genres.Where(x => !existingIds.Contains(x.Id)))
            track.TrackGenres.Add(new TrackGenre { TrackId = track.Id, GenreId = g.Id });
    }

    private static List<string> GetDistinctNames(IEnumerable<string>? values, string? fallback)
    {
        var result = new List<string>();
        if (values is not null) result.AddRange(values);
        if (!string.IsNullOrWhiteSpace(fallback))
            result.AddRange(fallback.Split([',', ';', '|', '/'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return result.Select(n => n.Trim()).Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task<string?> ValidateTrackArtistsMatchAlbumAsync(
        int albumId, IReadOnlyCollection<Artist> trackArtists, CancellationToken ct)
    {
        var album = await albumRepository.GetByIdWithDetailsAsync(albumId, ct);
        if (album is null) return $"Альбом {albumId} не найден.";

        var albumArtistIds = album.AlbumArtists.Select(aa => aa.ArtistId).ToHashSet();
        var trackArtistIds = trackArtists.Select(a => a.Id).ToHashSet();

        var hasMatch = trackArtistIds.Any(id => albumArtistIds.Contains(id));
        if (hasMatch) return null;
        var albumArtistNames = album.AlbumArtists
            .Where(aa => aa.Artist is not null)
            .Select(aa => aa.Artist!.Name);
        return $"«{album.Title}» — артисты альбома: {string.Join(", ", albumArtistNames)}.";
    }
}