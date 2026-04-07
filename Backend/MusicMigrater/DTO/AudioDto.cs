namespace MusicMigrater.DTO;

public record AudioDto(
    int Id,
    string ExternalId,
    string MusicProviderEnum,
    string Artist,
    string Title,
    int Duration,
    DateTime CreatedAt);
