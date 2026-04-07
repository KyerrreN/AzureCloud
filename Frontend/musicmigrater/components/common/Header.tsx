"use client";

import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Box,
  Container,
} from "@mui/material";
import SyncIcon from "@mui/icons-material/SyncAlt";
import Link from "next/link";

export function Header() {
  return (
    <AppBar
      position="static"
      color="transparent"
      elevation={0}
      sx={{ borderBottom: "1px solid", borderColor: "divider" }}
    >
      <Container maxWidth="lg">
        <Toolbar disableGutters>
          <SyncIcon sx={{ color: "primary.main", mr: 1, fontSize: 28 }} />

          <Typography
            variant="h6"
            component={Link}
            href="/"
            sx={{
              flexGrow: 1,
              fontWeight: "bold",
              letterSpacing: 1,
              textDecoration: "none",
              color: "inherit",
            }}
          >
            MusicMigrater
          </Typography>

          <Box sx={{ display: "flex", gap: 2 }}>
            <Button
              component={Link}
              href="/"
              color="inherit"
              sx={{ fontWeight: 500 }}
            >
              Главная
            </Button>
            <Button
              component={Link}
              href="/dashboard"
              color="inherit"
              sx={{ fontWeight: 500 }}
            >
              Дашборд
            </Button>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
}
