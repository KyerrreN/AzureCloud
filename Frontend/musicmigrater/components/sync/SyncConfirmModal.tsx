"use client";

import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Button,
  CircularProgress,
  Alert,
} from "@mui/material";
import { api } from "@/services/apiClient";

interface SyncConfirmModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function SyncConfirmModal({
  open,
  onClose,
  onSuccess,
}: SyncConfirmModalProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = () => {
    if (isLoading) return;
    setError(null);
    onClose();
  };

  const handleConfirm = async () => {
    setError(null);
    setIsLoading(true);

    try {
      const result = await api.get<string>("/sync/vkToSpotify");

      if (result.isSuccess && result.value) {
        localStorage.setItem("syncJobId", result.value);
        onSuccess();
      } else {
        setError(result.error || "Не удалось запустить процесс синхронизации.");
      }
    } catch (err) {
      console.error("Ошибка старта синхронизации:", err);
      setError("Сетевая ошибка. Проверьте подключение к серверу.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: "bold" }}>
        Подтверждение синхронизации
      </DialogTitle>
      <DialogContent>
        <Typography variant="body1" gutterBottom>
          Эта операция займет долгое время (примерно 30 минут на 1000 треков).
        </Typography>
        <Typography variant="body1" gutterBottom>
          Если в Dashboard вы увидите последнюю запись с ошибкой 429 - Too many
          requests, повторите попытку через 24 часа
        </Typography>
        <Typography variant="body1" gutterBottom>
          Вы увидите результаты в дашборде. <b>Не выключайте компьютер.</b>
        </Typography>
        <Typography variant="body1" sx={{ mt: 2, fontWeight: "bold" }}>
          Приступаем?
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {error}
          </Alert>
        )}
      </DialogContent>
      <DialogActions sx={{ p: 3 }}>
        <Button onClick={handleClose} color="inherit" disabled={isLoading}>
          Нет
        </Button>
        <Button
          onClick={handleConfirm}
          variant="contained"
          color="primary"
          disabled={isLoading}
          startIcon={
            isLoading ? <CircularProgress size={20} color="inherit" /> : null
          }
        >
          {isLoading ? "Запуск..." : "Да"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
