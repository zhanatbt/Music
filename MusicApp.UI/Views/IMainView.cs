using MusicApp.Application.DTOs;
using MusicApp.Domain.Common;

namespace MusicApp.UI.Presenters;

public interface IMainView
{
    string TitleFilter { get; }
    string ArtistFilter { get; }
    string AlbumFilter { get; }
    string GenreFilter { get; }
    int? SelectedTrackId { get; }
    IReadOnlyList<int> SelectedTrackIds { get; }
    int? SelectedPlaylistId { get; }
    int? SelectedPlaylistTrackId { get; }
    IReadOnlyList<int> SelectedPlaylistTrackIds { get; }
    string NewPlaylistName { get; }
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetPlaylists(IReadOnlyList<PlaylistDto> playlists);
    void SetPlaylistTracks(IReadOnlyList<TrackDto> tracks);
    void AddPlaylist(PlaylistDto playlist);
    void SelectPlaylistByName(string playlistName);
    void ClearNewPlaylistName();
    void PlayPreview(string previewUrl, string trackTitle);
    void ShowMessage(string message, string title = "Music App");
    PlaybackMode CurrentMode { get; }
    void PlayTrack(TrackDto track);
    event Action? TrackFinished;
    void SetFilterGenres(IReadOnlyList<GenreDto> genres);
    void SetFilterArtists(IReadOnlyList<ArtistDto> artists);
    void SetFilterAlbums(IReadOnlyList<AlbumDto> albums);
    void SetFilterTitles(IReadOnlyList<string> titles);
}