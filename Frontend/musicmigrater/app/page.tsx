import { Box, Container, Typography } from "@mui/material";
import { HomeClient } from "@/components/home/HomeClient";

export const metadata = {
  title: "Главная | MusicMigrater",
};

export default function Home() {
  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 8 }}>
        <Typography
          variant="h2"
          component="h1"
          align="center"
          fontWeight="bold"
          gutterBottom
        >
          MusicMigrater
        </Typography>

        <HomeClient />

        <Box sx={{ mt: 6, p: 2, textAlign: "center" }}>
          <Typography variant="caption" color="text.secondary">
            Ваши данные авторизации сохраняются локально в вашей базе данных.
            <br />
            Мы не передаем ваши токены третьим лицам и не храним ваши пароли.
          </Typography>
        </Box>
      </Box>
    </Container>
  );
}
