"use client";

import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Box,
  Link,
  TextField,
  Button,
  CircularProgress,
  Alert,
} from "@mui/material";
import { extractVkTokenFromUrl } from "@/utils/vkHelper";
import { api } from "@/services/apiClient";

interface VkAuthModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function VkAuthModal({ open, onClose, onSuccess }: VkAuthModalProps) {
  const [vkUrl, setVkUrl] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClose = () => {
    setVkUrl("");
    setError(null);
    onClose();
  };

  const handleSubmit = async () => {
    setError(null);

    const tokenData = extractVkTokenFromUrl(vkUrl);

    if (!tokenData) {
      setError(
        "Не удалось найти access_token в ссылке. Проверьте правильность скопированного URL.",
      );
      return;
    }

    setIsLoading(true);

    try {
      const result = await api.post("/settings/vk/saveToken", {
        token: tokenData.token,
        expiresIn: tokenData.expiresIn,
      });

      if (result.isSuccess) {
        setVkUrl("");
        onSuccess();
      } else {
        setError(result.error || "Ошибка при сохранении токена на сервере.");
      }
    } catch {
      setError("Сетевая ошибка. Проверьте подключение к серверу.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: "bold" }}>
        Аутентификация ВКонтакте
      </DialogTitle>
      <DialogContent>
        <Typography variant="body1" gutterBottom>
          Для аутентификации в ВК вам потребуется:
        </Typography>
        <Box component="ol" sx={{ pl: 2, "& li": { mb: 1 } }}>
          <li>
            <Typography variant="body2">
              Убедиться, что в ВК ваши <b>Аудиозаписи открыты</b>.
            </Typography>
          </li>
          <li>
            <Typography variant="body2">
              Перейти по этой ссылке:{" "}
              <Link
                href="https://oauth.vk.com/authorize?client_id=2685278&scope=all&response_type=token&redirect_uri=https://oauth.vk.com/blank.html"
                target="_blank"
                rel="noopener"
              >
                Получить доступ
              </Link>
            </Typography>
          </li>
          <li>
            <Typography variant="body2">Войти под своим аккаунтом.</Typography>
          </li>
          <li>
            <Typography variant="body2">
              Скопировать полностью адресную строку и ввести в поле ниже.
            </Typography>
          </li>
        </Box>

        <TextField
          fullWidth
          label="URL из адресной строки"
          variant="outlined"
          placeholder="https://oauth.vk.com/blank.html#access_token=..."
          value={vkUrl}
          onChange={(e) => {
            setVkUrl(e.target.value);
            setError(null);
          }}
          disabled={isLoading}
          sx={{ mt: 2 }}
        />

        {error && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {error}
          </Alert>
        )}
      </DialogContent>
      <DialogActions sx={{ p: 3 }}>
        <Button onClick={handleClose} color="inherit" disabled={isLoading}>
          Отмена
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={!vkUrl.includes("access_token") || isLoading}
          startIcon={
            isLoading ? <CircularProgress size={20} color="inherit" /> : null
          }
        >
          {isLoading ? "Подключение..." : "Подключить"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
