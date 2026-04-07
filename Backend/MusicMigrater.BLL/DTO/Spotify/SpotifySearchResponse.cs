namespace MusicMigrater.BLL.DTO.Spotify;

public record SpotifySearchResponse(SpotifySearchTracks Tracks);

public record SpotifySearchTracks(List<SpotifySearchTrackItem> Items);

public record SpotifySearchTrackItem(
    string Id,
    string Name
);
