using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicMigrater.BLL.Handlers;
using MusicMigrater.BLL.Mapping;
using MusicMigrater.BLL.Options;
using MusicMigrater.BLL.Refit;
using MusicMigrater.BLL.Services.Settings;
using MusicMigrater.BLL.Services.Spotify.Auth;
using MusicMigrater.BLL.Services.Spotify.Music;
using MusicMigrater.BLL.Services.VK.MusicService;
using MusicMigrater.BLL.Synchronization;
using MusicMigrater.BLL.Utils;
using Refit;
using System.Text.Json;

namespace MusicMigrater.BLL.DI;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterBllLayer(IConfiguration configuration)
        {
            // mapping config
            MapsterConfig.Configure();

            // options
            var vkSettings = configuration.GetRequiredSection(VkOptions.SectionName);
            services.Configure<VkOptions>(vkSettings);
            var vkOptions = vkSettings.Get<VkOptions>()
                ?? throw new InvalidOperationException("VK Settings are missing in configuration");

            var spotifySettings = configuration.GetRequiredSection(SpotifyUrlOptions.SectionName);
            services.Configure<SpotifyUrlOptions>(spotifySettings);
            var spotifyOptions = spotifySettings.Get<SpotifyUrlOptions>()
                ?? throw new InvalidOperationException("Spotify settings are missing in configuration");

            // services
            services.AddScoped<IVkMusicService, VkMusicService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<ISpotifyMusicService, SpotifyMusicService>();
            services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
            services.AddScoped<IMusicImportHelper, MusicImportHelper>();
            services.AddScoped<ISpotifySyncService, SpotifySyncService>();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var vkRefitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
            };

            var spotifyRefitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true
                })
            };

            services.AddRefitClient<IVkClient>(vkRefitSettings)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(vkOptions.Uri);
                    c.DefaultRequestHeaders.Add("User-Agent", vkOptions.UserAgent);
                });

            services.AddRefitClient<ISpotifyAuthClient>(spotifyRefitSettings)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(spotifyOptions.AuthUrl);
                });

            services.AddRefitClient<ISpotifyClient>(spotifyRefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(spotifyOptions.ClientUrl))
                .AddHttpMessageHandler<SpotifyAuthHandler>();

            return services;
        }
    }
}
