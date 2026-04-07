"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Box, Button, CircularProgress, Grid, Typography } from "@mui/material";
import { api } from "@/services/apiClient";
import { AuthStatus } from "@/dto/authStatus";
import { SpotifyCard } from "./SpotifyCard";
import { VkCard } from "./VkCard";
import { VkAuthModal } from "../auth/VkAuthModal";
import { SyncConfirmModal } from "../sync/SyncConfirmModal";

export function HomeClient() {
  const router = useRouter();
  const [status, setStatus] = useState<AuthStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [isVkModalOpen, setIsVkModalOpen] = useState(false);
  const [isSyncModalOpen, setIsSyncModalOpen] = useState(false);

  useEffect(() => {
    const fetchStatus = async () => {
      try {
        const result = await api.get<AuthStatus>("/settings/checkAuth");
        if (result.isSuccess && result.value) setStatus(result.value);
      } finally {
        setIsLoading(false);
      }
    };
    fetchStatus();
  }, []);

  if (isLoading) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "50vh",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  const safeStatus = status || {
    isAuthenticatedInVk: false,
    isAuthenticatedInSpotify: false,
    isImportedMusicFromVk: false,
  };

  const isAllDone =
    safeStatus.isAuthenticatedInVk &&
    safeStatus.isAuthenticatedInSpotify &&
    safeStatus.isImportedMusicFromVk;

  const handleSyncSuccess = () => {
    setIsSyncModalOpen(false);
    router.push("/dashboard");
  };

  return (
    <>
      <Typography
        variant="h6"
        align="center"
        color="text.secondary"
        sx={{ mb: 6 }}
      >
        {isAllDone
          ? "Все источники настроены. Можно переходить к синхронизации!"
          : "Выберите источники для миграции вашей медиатеки"}
      </Typography>

      <Grid container spacing={4} alignItems="stretch" justifyContent="center">
        <Grid size={{ xs: 12, md: 6 }}>
          <VkCard
            isAuthenticated={safeStatus.isAuthenticatedInVk}
            isImported={safeStatus.isImportedMusicFromVk}
            onConnectClick={() => setIsVkModalOpen(true)}
            onImportSuccess={() =>
              setStatus((prev) =>
                prev ? { ...prev, isImportedMusicFromVk: true } : null,
              )
            }
          />
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <SpotifyCard isAuthenticated={safeStatus.isAuthenticatedInSpotify} />
        </Grid>
      </Grid>

      {isAllDone && (
        <Box sx={{ mt: 6, display: "flex", justifyContent: "center" }}>
          <Button
            variant="contained"
            size="large"
            color="primary"
            onClick={() => setIsSyncModalOpen(true)}
            sx={{
              px: 6,
              py: 2,
              borderRadius: 10,
              fontSize: "1.1rem",
              fontWeight: "bold",
            }}
          >
            Перенести треки
          </Button>
        </Box>
      )}

      <VkAuthModal
        open={isVkModalOpen}
        onClose={() => setIsVkModalOpen(false)}
        onSuccess={() => {
          setIsVkModalOpen(false);
          setStatus((prev) =>
            prev ? { ...prev, isAuthenticatedInVk: true } : null,
          );
        }}
      />

      <SyncConfirmModal
        open={isSyncModalOpen}
        onClose={() => setIsSyncModalOpen(false)}
        onSuccess={handleSyncSuccess}
      />
    </>
  );
}
