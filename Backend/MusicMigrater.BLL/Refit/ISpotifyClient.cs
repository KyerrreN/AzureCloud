using MusicMigrater.BLL.DTO.Spotify;
using Refit;

namespace MusicMigrater.BLL.Refit;

/// <summary>
/// Interface for sending requests to Spotify API (uses handler for auth)
/// </summary>
public interface ISpotifyClient
{
    [Get("/me/tracks")]
    Task<SpotifyTracksResponse> GetMySpotifyTracksAsync(
        int limit = 10,
        int offset = 0,
        CancellationToken cancellationToken = default);

    [Get("/search")]
    Task<SpotifySearchResponse> SearchAsync(
        [Query] string q,
        [Query] string type = "track",
        [Query] int limit = 1,
        CancellationToken cancellationToken = default);

    [Put("/me/library")]
    Task AddToLikedAsync(
        [Query(CollectionFormat.Csv)] string[] uris,
        CancellationToken cancellationToken = default);
}
