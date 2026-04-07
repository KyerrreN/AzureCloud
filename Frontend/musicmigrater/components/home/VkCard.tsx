"use client";

import {
  Box,
  Button,
  CircularProgress,
  Paper,
  Typography,
} from "@mui/material";
import VkIcon from "@mui/icons-material/CloudDownload";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import { useState } from "react";
import { api } from "@/services/apiClient";

interface VkCardProps {
  isAuthenticated: boolean;
  isImported: boolean;
  onConnectClick: () => void;
  onImportSuccess: (count: number) => void;
}

export function VkCard({
  isAuthenticated,
  isImported,
  onConnectClick,
  onImportSuccess,
}: VkCardProps) {
  const [isImporting, setIsImporting] = useState(false);
  const [importedCount, setImportedCount] = useState<number | null>(null);

  const handleImport = async () => {
    setIsImporting(true);
    try {
      const result = await api.post<number>("/music/vk/save");
      if (result.isSuccess && result.value !== null) {
        setImportedCount(result.value);
        onImportSuccess(result.value);
      } else {
        console.error("Ошибка при импорте треков:", result.error);
      }
    } catch (err) {
      console.error("Сетевая ошибка при импорте:", err);
    } finally {
      setIsImporting(false);
    }
  };

  return (
    <Paper
      elevation={3}
      sx={{
        p: 5,
        height: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        borderRadius: 4,
        border: "2px solid transparent",
        transition: "0.3s",
        "&:hover": {
          borderColor: "#2787F5",
          boxShadow: "0 8px 24px rgba(39, 135, 245, 0.15)",
        },
      }}
    >
      <VkIcon sx={{ fontSize: 80, color: "#2787F5", mb: 3 }} />
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        ВКонтакте
      </Typography>
      <Typography
        variant="body1"
        color="text.secondary"
        align="center"
        sx={{ mb: 4 }}
      >
        Источник вашей музыки. Подключите аккаунт, чтобы мы могли прочитать ваши
        аудиозаписи.
      </Typography>

      <Box
        sx={{
          mt: "auto",
          display: "flex",
          justifyContent: "center",
          width: "100%",
        }}
      >
        {!isAuthenticated ? (
          <Button
            variant="outlined"
            size="large"
            onClick={onConnectClick}
            sx={{
              color: "#2787F5",
              borderColor: "#2787F5",
              borderWidth: 2,
              px: 4,
              py: 1.5,
              borderRadius: 10,
              fontWeight: "bold",
              "&:hover": {
                borderWidth: 2,
                backgroundColor: "rgba(39, 135, 245, 0.08)",
              },
            }}
          >
            Аутентифицироваться
          </Button>
        ) : !isImported ? (
          <Button
            variant="contained"
            size="large"
            onClick={handleImport}
            disabled={isImporting}
            startIcon={
              isImporting ? (
                <CircularProgress size={20} color="inherit" />
              ) : null
            }
            sx={{
              backgroundColor: "#2787F5",
              color: "white",
              px: 4,
              py: 1.5,
              borderRadius: 10,
              fontWeight: "bold",
              "&:hover": { backgroundColor: "#1e68c2" },
            }}
          >
            {isImporting ? "Загрузка..." : "Импортировать треки"}
          </Button>
        ) : (
          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              gap: 0.5,
            }}
          >
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 1,
                color: "success.main",
              }}
            >
              <CheckCircleIcon />
              <Typography fontWeight="bold">Готово</Typography>
            </Box>
            {importedCount !== null && (
              <Typography variant="caption" color="text.secondary">
                Успешно сохранено: {importedCount} треков
              </Typography>
            )}
          </Box>
        )}
      </Box>
    </Paper>
  );
}
