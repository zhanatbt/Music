using MusicApp.Application.DTOs;

namespace MusicApp.UI.Presenters;

public interface IAdminView
{
    string GenreName { get; }
    IReadOnlyList<string> SelectedGenreNames { get; }
    IReadOnlyList<string> SelectedArtistNames { get; }
    string CategoryName { get; }
    string TrackTitle { get; }
    string ArtistName { get; }
    string AlbumTitle { get; }
    int DurationSeconds { get; }
    int? SelectedGenreId { get; }
    int? SelectedCategoryId { get; }
    string TrackSearchTitle { get; }
    string TrackSearchArtist { get; }
    string TrackSearchAlbum { get; }
    string TrackSearchGenre { get; }
    string CategoryLookupSearch { get; }
    string GenreLookupSearch { get; }
    string ArtistLookupSearch { get; }
    string NewCategoryName { get; }
    string NewGenreName { get; }
    string NewArtistName { get; }
    int? SelectedCategoryLookupId { get; }
    int? SelectedGenreLookupId { get; }
    int? SelectedArtistLookupId { get; }
    int? EditingTrackId { get; }
    string? ImportedAudioFilePath { get; }
    TrackDto? SelectedTrack { get; }
    UserSessionDto? SelectedUser { get; }
    PlaylistDto? SelectedUserPlaylist { get; }
    string? PickAudioFile();
    void ApplyAudioMetadata(AudioMetadataDto metadata);
    void TrySelectGenreByName(string? genreName);
    void SetGenres(IReadOnlyList<GenreDto> genres);
    void SetArtists(IReadOnlyList<ArtistDto> artists);
    void SetCategoryLookupItems(IReadOnlyList<CategoryDto> categories);
    void SetGenreLookupItems(IReadOnlyList<GenreDto> genres);
    void SetArtistLookupItems(IReadOnlyList<ArtistDto> artists);
    void SetCategories(IReadOnlyList<CategoryDto> categories);
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetUsers(IReadOnlyList<UserSessionDto> users);
    void SetUserPlaylists(IReadOnlyList<PlaylistDto> playlists);
    void SetSelectedUserPlaylistTracks(IReadOnlyList<TrackDto> tracks);
    void LoadTrackIntoEditor(TrackDto track);
    void ClearNewCategoryInput();
    void ClearNewGenreInput();
    void ClearNewArtistInput();
    void PlayPreview(string previewUrl, string trackTitle);
    void ShowMessage(string message, string title = "Music App");
    void ClearEntryFields();
}
