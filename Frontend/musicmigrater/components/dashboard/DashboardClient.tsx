"use client";

import { useEffect, useState } from "react";
import { Typography, CircularProgress, Button } from "@mui/material";
import { useRouter } from "next/navigation";
import { api } from "@/services/apiClient";
import { SyncResults } from "./SyncResults";
import { FailedToSyncLogsDto } from "@/dto/syncLogDtos";

export function DashboardClient() {
  const router = useRouter();

  const [isLoading, setIsLoading] = useState(true);
  const [hasJob, setHasJob] = useState(false);
  const [isFinished, setIsFinished] = useState<boolean | null>(null);
  const [syncStats, setSyncStats] = useState<FailedToSyncLogsDto | null>(null);

  useEffect(() => {
    const checkStatus = async (jobId: string) => {
      try {
        const result = await api.post<boolean>("/sync/status", { jobId });

        if (result.isSuccess) {
          setIsFinished(result.value);
        } else {
          const errorMsg = (result.error || "").toLowerCase();
          if (errorMsg.includes("not found") || errorMsg.includes("400")) {
            localStorage.removeItem("syncJobId");
            setIsFinished(true);
          } else {
            console.error("Ошибка API при проверке статуса:", result.error);
          }
        }
      } catch (err: unknown) {
        let errorMessage = "";

        if (err instanceof Error) {
          errorMessage = err.message.toLowerCase();
        } else if (typeof err === "string") {
          errorMessage = err.toLowerCase();
        }

        if (errorMessage.includes("not found")) {
          localStorage.removeItem("syncJobId");
          setIsFinished(true);
        } else {
          console.error("Сетевая ошибка при проверке статуса:", err);
        }
      }
    };

    const loadInitialData = async () => {
      const jobId = localStorage.getItem("syncJobId");

      if (jobId) {
        setHasJob(true);
        await checkStatus(jobId);
        setIsLoading(false);
      } else {
        try {
          const result = await api.get<FailedToSyncLogsDto>(
            "/sync/getFailedLogs",
          );
          if (
            result.isSuccess &&
            result.value &&
            result.value.vkTotalCount > 0
          ) {
            setHasJob(true);
            setIsFinished(true);
            setSyncStats(result.value);
          } else {
            setHasJob(false);
          }
        } catch (err) {
          console.error("Ошибка при получении старых логов:", err);
          setHasJob(false);
        } finally {
          setIsLoading(false);
        }
      }
    };

    loadInitialData();

    const interval = setInterval(() => {
      const currentJobId = localStorage.getItem("syncJobId");
      if (currentJobId && isFinished === false) {
        checkStatus(currentJobId);
      }
    }, 15000);

    return () => clearInterval(interval);
  }, [isFinished]);

  useEffect(() => {
    if (isFinished === true && !syncStats) {
      const fetchLogs = async () => {
        try {
          const result = await api.get<FailedToSyncLogsDto>(
            "/sync/getFailedLogs",
          );
          if (result.isSuccess && result.value) {
            setSyncStats(result.value);
          }
        } catch (err) {
          console.error("Ошибка при загрузке логов:", err);
        }
      };
      fetchLogs();
    }
  }, [isFinished, syncStats]);

  if (isLoading) return <CircularProgress />;

  if (!hasJob)
    return (
      <>
        <Typography variant="h5" color="text.secondary" gutterBottom>
          Синхронизация не была запущена.
        </Typography>
        <Typography variant="body1" sx={{ mb: 4, maxWidth: 500 }}>
          Запустите, чтобы увидеть результаты.
        </Typography>
        <Button
          variant="contained"
          size="large"
          onClick={() => router.push("/")}
          sx={{ borderRadius: 10, px: 4, fontWeight: "bold" }}
        >
          Вернуться на главную
        </Button>
      </>
    );

  if (isFinished === false)
    return (
      <>
        <CircularProgress
          size={60}
          thickness={4}
          sx={{ mb: 4, color: "primary.main" }}
        />
        <Typography variant="h4" fontWeight="bold" gutterBottom>
          Синхронизация еще в процессе...
        </Typography>
        <Typography
          variant="body1"
          color="text.secondary"
          sx={{ maxWidth: 600 }}
        >
          Ожидайте результата. Это может занять некоторое время. Вы можете
          оставить эту вкладку открытой — статус обновится автоматически.
        </Typography>
      </>
    );

  if (isFinished === true)
    return (
      <>
        <Typography variant="h1" sx={{ fontSize: 80, mb: 2 }}>
          🎉
        </Typography>
        <Typography
          variant="h4"
          fontWeight="bold"
          color="success.main"
          gutterBottom
        >
          Синхронизация завершена!
        </Typography>

        {syncStats ? (
          <SyncResults stats={syncStats} />
        ) : (
          <CircularProgress sx={{ mt: 4, mb: 2 }} />
        )}
      </>
    );

  return <Typography color="error">Ошибка получения статуса.</Typography>;
}
