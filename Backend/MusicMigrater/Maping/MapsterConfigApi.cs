using Mapster;
using MusicMigrater.BLL.DTO.Audio;
using MusicMigrater.DTO;

namespace MusicMigrater.Maping;

public static class MapsterConfigApi
{
    public static void Configure()
    {
        TypeAdapterConfig.GlobalSettings.Default
            .EnumMappingStrategy(EnumMappingStrategy.ByName);

        TypeAdapterConfig<AudioModel, AudioDto>
            .NewConfig()
            .Map(dest => dest.MusicProviderEnum, src => src.MusicProviderEnum.ToString());
    }
}
