using Mapster;
using MusicMigrater.BLL.DTO.Audio;
using MusicMigrater.BLL.DTO.Spotify;
using MusicMigrater.BLL.DTO.VkMusic;
using MusicMigrater.DAL.Entities;
using MusicMigrater.Domain.Enums;

namespace MusicMigrater.BLL.Mapping;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<VkAudioItemModel, AudioEntity>.NewConfig()
            .Map(dest => dest.ExternalId, src => src.Id.ToString())
            .Map(dest => dest.Provider, _ => MusicProviderEnum.Vk)
            .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow)
            .Ignore(dest => dest.Id);

        TypeAdapterConfig<AudioEntity, AudioModel>.NewConfig()
            .Map(dest => dest.MusicProviderEnum, src => src.Provider);

        TypeAdapterConfig<SpotifySingleTrackResponse, AudioEntity>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Map(dest => dest.ExternalId, src => src.Id)
            .Map(dest => dest.Artist, src => string.Join(", ", src.Artists.Select(a => a.Name)))
            .Map(dest => dest.Title, src => src.Name)
            .Map(dest => dest.Provider, src => MusicProviderEnum.Spotify)
            .Map(dest => dest.Duration, src => src.DurationMs / 1000)
            .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow);
    }
}
