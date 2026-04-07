import { DashboardClient } from "@/components/dashboard/DashboardClient";
import { Box, Container, Paper, Typography } from "@mui/material";

export const metadata = {
  title: "Дашборд | MusicMigrater",
};

export default function DashboardPage() {
  return (
    <Container maxWidth="md">
      <Box sx={{ py: 8 }}>
        <Typography
          variant="h3"
          component="h1"
          align="center"
          fontWeight="bold"
          gutterBottom
        >
          Дашборд
        </Typography>

        <Paper
          elevation={3}
          sx={{
            p: 6,
            mt: 4,
            borderRadius: 4,
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            textAlign: "center",
            minHeight: "400px",
            justifyContent: "center",
          }}
        >
          <DashboardClient />
        </Paper>
      </Box>
    </Container>
  );
}
