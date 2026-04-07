namespace MusicMigrater.BLL.DTO.UserSettings;

public record IsAuthenticatedInServicesDto(
    bool IsAuthenticatedInVk, 
    bool IsAuthenticatedInSpotify,
    bool IsImportedMusicFromVk);
