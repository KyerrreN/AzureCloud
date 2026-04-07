namespace MusicMigrater.BLL.DTO.Spotify;

public record SpotifyTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    string Scope);
