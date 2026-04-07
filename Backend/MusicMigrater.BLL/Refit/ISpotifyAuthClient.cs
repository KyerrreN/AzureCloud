using MusicMigrater.BLL.DTO.Spotify;
using Refit;

namespace MusicMigrater.BLL.Refit;

/// <summary>
/// Interface for sending requests to Spotify Auth API
/// </summary>
public interface ISpotifyAuthClient
{
    [Post("/api/token")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    Task<SpotifyTokenResponse> ExchangeCodeAsync(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data,
        [Header("Authorization")] string basicAuth);

    [Post("/api/token")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    Task<SpotifyTokenResponse> RefreshTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data,
        [Header("Authorization")] string basicAuth);
}
