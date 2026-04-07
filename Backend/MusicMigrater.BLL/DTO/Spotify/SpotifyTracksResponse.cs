namespace MusicMigrater.BLL.DTO.Spotify;

public record SpotifyTracksResponse(
    int Total,
    List<SpotifySavedItemResponse> Items);

public record SpotifySavedItemResponse(
    string AddedAt,
    SpotifySingleTrackResponse Track);

public record SpotifySingleTrackResponse(
    string Id,
    string Name,
    string Type,
    int DurationMs,
    List<SpotifyArtistsResponse> Artists);

public record SpotifyArtistsResponse(
    string Name,
    string Id);
