using MusicApp.Application.DTOs;

namespace MusicApp.UI.Presenters;

public interface IMainView
{
    string SearchText { get; }
    int? SelectedTrackId { get; }
    int? SelectedPlaylistId { get; }
    string NewPlaylistName { get; }
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetPlaylists(IReadOnlyList<PlaylistDto> playlists);
    void PlayPreview(string previewUrl, string trackTitle);
    void ShowMessage(string message, string title = "Music App");
}
