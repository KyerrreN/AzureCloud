"use client";

import { getSpotifyAuthUrl } from "@/utils/spotifyHelper";
import { Box, Button, Paper, Typography } from "@mui/material";
import SpotifyIcon from "@mui/icons-material/LibraryMusic";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";

interface SpotifyCardProps {
  isAuthenticated: boolean;
}

export function SpotifyCard({ isAuthenticated }: SpotifyCardProps) {
  const handleConnect = () => {
    window.location.href = getSpotifyAuthUrl();
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
          borderColor: "#1DB954",
          boxShadow: "0 8px 24px rgba(29, 185, 84, 0.15)",
        },
      }}
    >
      <SpotifyIcon sx={{ fontSize: 80, color: "#1DB954", mb: 3 }} />
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Spotify
      </Typography>
      <Typography
        variant="body1"
        color="text.secondary"
        align="center"
        sx={{ mb: 4 }}
      >
        Куда переносим. Предоставьте доступ, чтобы мы могли добавлять треки в
        вашу медиатеку.
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
            variant="contained"
            size="large"
            onClick={handleConnect}
            sx={{
              backgroundColor: "#1DB954",
              color: "white",
              px: 4,
              py: 1.5,
              borderRadius: 10,
              fontWeight: "bold",
              "&:hover": { backgroundColor: "#17a44a" },
            }}
          >
            Аутентифицироваться
          </Button>
        ) : (
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
        )}
      </Box>
    </Paper>
  );
}
