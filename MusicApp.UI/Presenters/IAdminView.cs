using MusicApp.Application.DTOs;

namespace MusicApp.UI.Presenters;

public interface IAdminView
{
    string GenreName { get; }
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
    string DeezerQuery { get; }
    string? ImportedAudioFilePath { get; }
    string? ImportedGenreName { get; }
    TrackDto? SelectedTrack { get; }
    DeezerTrackDto? SelectedDeezerTrack { get; }
    string? PickAudioFile();
    void ApplyAudioMetadata(AudioMetadataDto metadata);
    void TrySelectGenreByName(string? genreName);
    void SetGenres(IReadOnlyList<GenreDto> genres);
    void SetCategories(IReadOnlyList<CategoryDto> categories);
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetUsers(IReadOnlyList<UserSessionDto> users);
    void SetDeezerResults(IReadOnlyList<DeezerTrackDto> tracks);
    void PlayPreview(string previewUrl, string trackTitle);
    void ShowMessage(string message, string title = "Music App");
    void ClearEntryFields();
}
