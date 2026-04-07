using MusicMigrater.Domain.Enums;

namespace MusicMigrater.BLL.DTO.Audio;

public record AudioModel(
    int Id,
    string ExternalId,
    MusicProviderEnum MusicProviderEnum,
    string Artist,
    string Title,
    int Duration,
    DateTime CreatedAt);
