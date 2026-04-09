using Microsoft.Extensions.Logging;

namespace MusicMigrater.Domain.Logging;

public static partial class HighPerformanceLogging
{
    // INFORMATION
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Starting sending batch requests to {provider}")]
    public static partial void LogStartBatch(this ILogger logger, string provider);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "{provider} Batch processed. Added: {addedCount}. Total: {totalCount}")]
    public static partial void LogDataBatch(this ILogger logger, int addedCount, int totalCount, string provider);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Success: import from {provider} is complete.")]
    public static partial void LogEndDataBatch(this ILogger logger, string provider);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "HTTP {method} {path} started.")]
    public static partial void LogRequestStarted(this ILogger logger, string method, string path);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "HTTP {method} {path} finished in {elapsedMs}ms with status {statusCode}.")]
    public static partial void LogRequestFinished(this ILogger logger, string method, string path, double elapsedMs, int statusCode);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "{service} token expired. Refreshing...")]
    public static partial void LogTokenRefreshingInformation(this ILogger logger, string service);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "{service} token refreshed succesfully")]
    public static partial void LogTokenRefreshedSuccessfully(this ILogger logger, string service);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "{service} token saved successfully")]
    public static partial void LogTokenSavedSuccessfully(this ILogger logger, string service);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Information,
        Message = "Succesfully retrieved tracks for current user from {service}. Total: {total}")]
    public static partial void LogSuccessfullyRetrievedTracksFromExternalService(this ILogger logger, string service, int total);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "There are no more track to sync. Everything is up to date")]
    public static partial void LogNoMoreTracksToSync(this ILogger logger);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Start migration of tracks to spotify for {count} tracks")]
    public static partial void LogStartMigration(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Information,
        Message = "Succesfully found track on spotify. Title: {title}, Artist: {artist}")]
    public static partial void LogFoundTrackOnSpotify(this ILogger logger, string title, string artist);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Information,
        Message = "Couldn't find track on spotify. Title: {title}, Artist: {artist}")]
    public static partial void LogCouldntFindTrackOnSpotify(this ILogger logger, string title, string artist);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Information,
        Message = "Track migration has completed successfully")]
    public static partial void LogTrackMigrationSuccess(this ILogger logger);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Information,
        Message = "Initiated sleep after {requestCount}. Duration: {sleepDurationInSeconds} seconds")]
    public static partial void LogInitiateSyncSleep(this ILogger logger, int requestCount, int sleepDurationInSeconds);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Information,
        Message = "Starting applying database migrations...")]
    public static partial void LogStartApplyingMigrations(this ILogger logger);

    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Information,
        Message = "Finished applying database migrations.")]
    public static partial void LogFinishApplyingMigrations(this ILogger logger);

    // WARNINGS
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "User is already authenticated")]
    public static partial void LogUserAlreadyAuthenticatedWarning(this ILogger logger);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Invalid request parameters: {message}")]
    public static partial void LogInvalidParametersWarning(this ILogger logger, string message);

    // ERRORS
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Error,
        Message = "VK Token is missing, application is in inconsistent state")]
    public static partial void LogVkTokenMissingError(this ILogger logger);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Error,
        Message = "Error occured while processing logic. Message: {message}")]
    public static partial void LogRequestError(this ILogger logger, string message, Exception? exception);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Token is invalid, couldn't authenticate user")]
    public static partial void LogTokenError(this ILogger logger);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Error,
        Message = "Couldn't reach {service}")]
    public static partial void LogCouldntReachServiceError(this ILogger logger, string service, Exception? exception);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Error,
        Message = "Error while saving changes to the database")]
    public static partial void LogSaveChangesError(this ILogger logger, Exception? exception);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Error,
        Message = "Unauthorized in {service}")]
    public static partial void LogUnauthorizedInServiceError(this ILogger logger, string service);

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Error,
        Message = "Unexpected error occured")]
    public static partial void LogUnexpectedError(this ILogger logger, Exception? exception);

    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Error,
        Message = "Error while reaching {service}, StatusCode: {statusCode}, Content: {content}")]
    public static partial void LogErrorSendingRequestToExternalService(this ILogger logger, string service, int statusCode, string? content, Exception? ex);

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Error,
        Message = "Failed to save batch of {count} tracks to {service}")]
    public static partial void LogFailedToSaveBatchWhileMigrating(this ILogger logger, int count, string service, Exception? ex);

    // CRITICAL
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Critical,
        Message = "Hit rate limit. Aborting")]
    public static partial void LogHitRateLimiter(this ILogger logger);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Critical,
        Message = "Error while applying migrations on startup")]
    public static partial void LogFailedToApplyMigrationsOnStartup(this ILogger logger, Exception ex);
}
