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
    string DeezerQuery { get; }
    DeezerTrackDto? SelectedDeezerTrack { get; }
    void SetGenres(IReadOnlyList<GenreDto> genres);
    void SetCategories(IReadOnlyList<CategoryDto> categories);
    void SetTracks(IReadOnlyList<TrackDto> tracks);
    void SetUsers(IReadOnlyList<UserSessionDto> users);
    void SetDeezerResults(IReadOnlyList<DeezerTrackDto> tracks);
    void ShowMessage(string message, string title = "Music App");
    void ClearEntryFields();
}
