using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MusicMigrater.BLL.DTO.Result;
using MusicMigrater.BLL.DTO.Sync;
using MusicMigrater.BLL.Synchronization;
using MusicMigrater.DTO;
using MusicMigrater.Filters;

namespace MusicMigrater.Endpoints;

public static class SyncEndpoints
{
    extension(WebApplication app)
    {
        public void MapSyncEndpoints()
        {
            var syncEndpointsGroup = app.MapGroup("/api/sync").AddEndpointFilter<ResultFilter>();

            syncEndpointsGroup.MapGet("/vkToSpotify", VkToSpotifySyncHandler);
            syncEndpointsGroup.MapGet("/getFailedLogs", GetFailedLogsHandler);

            syncEndpointsGroup.MapPost("/status", StatusHandler);
        }

        private static async Task<Result<FailedToSyncLogsDto>> GetFailedLogsHandler(
            ISpotifySyncService service,
            CancellationToken cancellationToken = default)
        {
            return await service.GetFailedLogsAsync(cancellationToken);
        }

        private static async Task<Result<string>> VkToSpotifySyncHandler(CancellationToken cancellationToken = default)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var processingJob = monitoringApi.ProcessingJobs(0, 1000)
                .FirstOrDefault(x => x.Value.Job.Type == typeof(ISpotifySyncService));

            if (processingJob.Key is not null)
            {
                return Result.Success(processingJob.Key);
            }

            var enqueuedJob = monitoringApi.EnqueuedJobs("default", 0, 1000)
                .FirstOrDefault(x => x.Value.Job.Type == typeof(ISpotifySyncService));

            if (enqueuedJob.Key is not null)
            {
                return Result.Success(enqueuedJob.Key);
            }

            var jobId = BackgroundJob.Enqueue<ISpotifySyncService>(
                service => service.RunSyncPipelineAsync(CancellationToken.None));

            return Result.Success(jobId);
        }

        private static async Task<Result<bool>> StatusHandler(
            [FromBody] JobStatusDto jobDto, 
            CancellationToken cancellationToken = default)
        {
            var connection = JobStorage.Current.GetConnection();
            var jobData = connection.GetJobData(jobDto.JobId);

            if (jobData is null)
            {
                return Result.Failed<bool>("Job is not found or has been deleted");
            }

            var isCompleted = jobData.State == "Succeeded" || jobData.State == "Failed";

            return isCompleted;
        }
    }
}
