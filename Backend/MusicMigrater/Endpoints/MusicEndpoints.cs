using Mapster;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.Spotify;
using MusicMigrater.BLL.Services.Spotify.Music;
using MusicMigrater.BLL.Services.VK.MusicService;
using MusicMigrater.Domain.Enums;
using MusicMigrater.DTO;
using MusicMigrater.Filters;

namespace MusicMigrater.Endpoints;

public static class MusicEndpoints
{
    extension(WebApplication app)
    {
        public void MapMusicEndpoints()
        {
            var musicEndpointsGroup = app
                .MapGroup("/api/music")
                .AddEndpointFilter<ResultFilter>();
            // VK
            musicEndpointsGroup.MapGet("/vk/count", GetVkMusicCountHandler).AddEndpointFilter<VkTokenFilter>();
            musicEndpointsGroup.MapGet("/audio/get", GetAudioFromLocalDbHandler);

            musicEndpointsGroup.MapPost("/vk/save", SaveMusicHandler).AddEndpointFilter<VkTokenFilter>();

            // Spotify
            musicEndpointsGroup.MapGet("/spotify/getMyTrack", GetSpotifyMyTracksHandler);
            musicEndpointsGroup.MapPost("/spotify/save", SaveSpotifyMusicToLocalDbHandler);
        }

        private static async Task<Result<int>> GetVkMusicCountHandler(
            IVkMusicService musicService,
            CancellationToken cancellationToken = default)
        {
            return await musicService.GetMusicCountAsync(cancellationToken);
        }

        private static async Task<Result<int>> SaveMusicHandler(
            IVkMusicService musicService,
            CancellationToken cancellationToken = default)
        {
            return await musicService.AddMusicToLocalDbAsync(cancellationToken);
        }

        private static async Task<Result<List<AudioDto>>> GetAudioFromLocalDbHandler(
            IVkMusicService musicService,
            MusicProviderEnum musicProviderEnum,
            int offset = 0, 
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await musicService.GetAudioFromLocalDbAsync(offset, count, musicProviderEnum, cancellationToken);

            return result.Adapt<Result<List<AudioDto>>>();
        }

        private static async Task<Result<SpotifyTracksResponse>> GetSpotifyMyTracksHandler(
            ISpotifyMusicService musicService,
            CancellationToken cancellationToken = default)
        {
            return await musicService.GetMySpotifyTracksAsync(cancellationToken: cancellationToken);
        }

        private static async Task<Result<int>> SaveSpotifyMusicToLocalDbHandler(
            ISpotifyMusicService musicService,
            CancellationToken cancellationToken = default)
        {
            return await musicService.SaveSpotifyTracksInLocalDbAsync(cancellationToken);
        }
    }
}
