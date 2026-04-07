export interface SyncLogDto {
  title: string;
  artist: string;
  targetProvider: string;
  resultDetails: string;
  attemptedAt: string;
}

export interface FailedToSyncLogsDto {
  vkTotalCount: number;
  succesfullyMigratedCount: number;
  failedLogsList: SyncLogDto[];
}
