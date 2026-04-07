using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.UserSettings;
using MusicMigrater.BLL.DTO.VkMusic;
using MusicMigrater.BLL.Services.Settings;
using MusicMigrater.BLL.Services.Spotify.Auth;
using MusicMigrater.Domain.Constants;
using MusicMigrater.DTO;
using MusicMigrater.Filters;

namespace MusicMigrater.Endpoints;

public static class UserSettingsEndpoints
{
    extension(WebApplication app)
    {
        public void MapUserSettingsEndpoints()
        {
            var userSettingsEndpoints = app
                .MapGroup("/api/settings")
                .AddEndpointFilter<ResultFilter>();

            // VK
            userSettingsEndpoints.MapGet("/vk/isAuthenticated", IsAuthenticatedInVkHandler);

            userSettingsEndpoints.MapPost("/vk/saveToken", SaveVkAccessTokenHandler);

            // Spotify
            userSettingsEndpoints.MapPost("/spotify/saveToken", SaveSpotifyTokensHandler);

            // General
            userSettingsEndpoints.MapGet("/checkAuth", CheckAuthHandler);
        }

        private static async Task<Result<bool>> IsAuthenticatedInVkHandler(
            IUserSettingsService userSettingsService,
            CancellationToken cancellationToken = default)
        {
            return await userSettingsService.IsRegisteredByProviderAsync(UserSettingsConstants.VkUserTokenCode, cancellationToken);
        }

        private static async Task<Result<bool>> SaveVkAccessTokenHandler(
            [FromBody] VkTokenDto vkTokenDto,
            IUserSettingsService userSettingsService,
            IValidator<VkTokenDto> validator,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(vkTokenDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Result.Failed<bool>(validationResult.Errors.FirstOrDefault()!.ErrorMessage);
            }

            return await userSettingsService.SaveVkAuthDataAsync(vkTokenDto, cancellationToken);
        }

        private static async Task<Result<bool>> SaveSpotifyTokensHandler(
            [FromBody] SpotifyExchangeCodeDto dto,
            ISpotifyAuthService service,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(dto.Code))
            {
                return Result.Failed<bool>($"{nameof(dto.Code)} is required");
            }

            return await service.ExchangeCodeAndSaveTokenAsync(dto.Code, cancellationToken);
        }

        private static async Task<Result<IsAuthenticatedInServicesDto>> CheckAuthHandler(
            IUserSettingsService service,
            CancellationToken cancellationToken = default)
        {
            return await service.CheckServicesAuthentication(cancellationToken);
        }
    }
}
