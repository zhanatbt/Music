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

    public MusicLibraryService(ITrackRepository trackRepository, IPlaylistRepository playlistRepository)
    {
        _trackRepository = trackRepository;
        _playlistRepository = playlistRepository;
    }

    public async Task<IReadOnlyList<TrackDto>> SearchTracksAsync(string? query, int? genreId = null, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        var tracks = await _trackRepository.SearchAsync(query, genreId, categoryId, cancellationToken);
        return tracks.Select(TrackMapper.ToDto).ToList();
    }

    public async Task<IReadOnlyList<PlaylistDto>> GetPlaylistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var playlists = await _playlistRepository.GetByUserIdAsync(userId, cancellationToken);
        return playlists.Select(LookupMapper.ToDto).ToList();
    }

    public async Task<OperationResult> CreatePlaylistAsync(int userId, string playlistName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(playlistName))
        {
            return OperationResult.Fail("Введите название плейлиста.");
        }

        var playlist = new Playlist
        {
            Name = playlistName.Trim(),
            UserId = userId
        };

        await _playlistRepository.AddAsync(playlist, cancellationToken);
        return OperationResult.Ok("Плейлист создан.");
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
}
