using MusicApp.Application.DTOs;

namespace MusicApp.UI.Views;

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
    int? EditingTrackId { get; }
    string? ImportedAudioFilePath { get; }
    TrackDto? SelectedTrack { get; }

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
    UserAdminDto? SelectedUser { get; }
    PlaylistDto? SelectedUserPlaylist { get; }
    string NewAlbumTitle { get; }
    IReadOnlyList<string> SelectedAlbumArtistNames { get; }
    int? SelectedAlbumId { get; }
    int? SelectedAlbumTrackId { get; }
    int? EditingAlbumId { get; }
    int? SelectedTrackForAlbumId { get; }
    void SetGenres(IReadOnlyList<GenreDto> genres);
    void SetArtists(IReadOnlyList<ArtistDto> artists);
    void SetCategories(IReadOnlyList<CategoryDto> categories);
    void SetCategoryLookupItems(IReadOnlyList<CategoryDto> categories);
    void SetGenreLookupItems(IReadOnlyList<GenreDto> genres);
    void SetArtistLookupItems(IReadOnlyList<ArtistDto> artists);
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetUsers(IReadOnlyList<UserAdminDto> users);
    void SetUserPlaylists(IReadOnlyList<PlaylistDto> playlists);
    void SetSelectedUserPlaylistTracks(IReadOnlyList<TrackDto> tracks);
    void SetAlbums(IReadOnlyList<AlbumDto> albums);
    void SetAlbumTracks(IReadOnlyList<TrackDto> tracks);
    void SetAlbumArtists(IReadOnlyList<ArtistDto> artists);
    string? PickAudioFile();
    void ApplyAudioMetadata(AudioMetadataDto metadata);
    void TrySelectGenreByName(string? genreName);
    void LoadTrackIntoEditor(TrackDto track);
    void ClearEntryFields();
    void ClearNewCategoryInput();
    void ClearNewGenreInput();
    void ClearNewArtistInput();
    void ClearAlbumFields();
    void LoadAlbumIntoEditor(AlbumDto album);
    void PlayPreview(string previewUrl, string trackTitle);
    void ShowMessage(string message, string title = "Music App");
}