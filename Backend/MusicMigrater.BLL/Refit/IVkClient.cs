using MusicMigrater.BLL.DTO.VkMusic;
using Refit;

namespace MusicMigrater.BLL.Refit;

/// <summary>
/// Refit interface to work with VK
/// </summary>
public interface IVkClient
{
    /// <summary>
    /// Get audio for current account
    /// </summary>
    /// <param name="token">Access token, used for sending request</param>
    /// <param name="count">Count of music (200 is max by documentation)</param>
    /// <param name="offset">Offset</param>
    /// <param name="version">Stable version that still has methods to retrieve audio (5.95 by default)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/method/audio.get")]
    Task<VkAudioResponseModel> GetMusicAsync(
        [AliasAs("access_token")] string token,
        [AliasAs("count")] int count = 10,
        [AliasAs("offset")] int offset = 0,
        [AliasAs("v")] string version = "5.95",
        CancellationToken cancellationToken = default);
}
