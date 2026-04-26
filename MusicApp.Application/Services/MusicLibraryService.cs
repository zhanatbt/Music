using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Mappers;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Services;

public class MusicLibraryService
{
    private readonly ITrackRepository _trackRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IFileStorageService _fileStorageService;

    public MusicLibraryService(
        ITrackRepository trackRepository,
        IPlaylistRepository playlistRepository,
        IFileStorageService fileStorageService)
    {
        _trackRepository = trackRepository;
        _playlistRepository = playlistRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<TrackDto>> SearchTracksAsync(
        string? query = null,
        string? album = null,
        string? genre = null,
        string? title = null,
        string? artist = null,
        int? genreId = null,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var tracks = await _trackRepository.SearchAsync(
            query,
            genreId,
            categoryId,
            album,
            genre,
            title,
            artist,
            cancellationToken);

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

    public async Task<IReadOnlyList<PlaylistDto>> GetPlaylistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var playlists = await _playlistRepository.GetByUserIdAsync(userId, cancellationToken);
        return playlists.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<TrackDto>> GetPlaylistTracksAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var tracks = await _playlistRepository.GetTracksByPlaylistIdAsync(playlistId, cancellationToken);
        return NormalizeTrackPreviewUrls(tracks);
    }

    public async Task<OperationResult<PlaylistDto>> CreatePlaylistAsync(int userId, string playlistName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(playlistName))
        {
            return OperationResult<PlaylistDto>.Fail("Введите название плейлиста.");
        }

        var normalizedName = playlistName.Trim();
        var existing = await _playlistRepository.GetByUserIdAndNameAsync(userId, normalizedName, cancellationToken);
        if (existing is not null)
        {
            return OperationResult<PlaylistDto>.Fail("Плейлист с таким названием уже существует.");
        }

        var playlist = new Playlist
        {
            Name = normalizedName,
            UserId = userId
        };

        await _playlistRepository.AddAsync(playlist, cancellationToken);

        return OperationResult<PlaylistDto>.Ok(new PlaylistDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            TrackCount = 0
        }, "Плейлист создан.");
    }

    public async Task<OperationResult> AddTrackToPlaylistAsync(int playlistId, int trackId, CancellationToken cancellationToken = default)
    {
        var track = await _trackRepository.GetByIdAsync(trackId, cancellationToken);
        if (track is null)
        {
            return OperationResult.Fail("Трек не найден.");
        }

        var playlist = await _playlistRepository.GetByIdAsync(playlistId, cancellationToken);
        if (playlist is null)
        {
            return OperationResult.Fail("Плейлист не найден.");
        }

        await _playlistRepository.AddTrackAsync(playlistId, trackId, cancellationToken);
        return OperationResult.Ok("Трек добавлен в плейлист.");
    }

    public async Task<OperationResult> RemoveTrackFromPlaylistAsync(int playlistId, int trackId, CancellationToken cancellationToken = default)
    {
        var playlist = await _playlistRepository.GetByIdAsync(playlistId, cancellationToken);
        if (playlist is null)
        {
            return OperationResult.Fail("Плейлист не найден.");
        }

        var trackExistsInPlaylist = playlist.PlaylistTracks.Any(x => x.TrackId == trackId);
        if (!trackExistsInPlaylist)
        {
            return OperationResult.Fail("Выбранный трек не найден в плейлисте.");
        }

        await _playlistRepository.RemoveTrackAsync(playlistId, trackId, cancellationToken);
        return OperationResult.Ok("Трек удалён из плейлиста.");
    }
}
