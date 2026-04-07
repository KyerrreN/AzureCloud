"use client";

import { FailedToSyncLogsDto } from "@/dto/syncLogDtos";
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider,
} from "@mui/material";

export function SyncResults({ stats }: { stats: FailedToSyncLogsDto }) {
  return (
    <Box sx={{ width: "100%", mt: 4, textAlign: "left" }}>
      <Paper
        variant="outlined"
        sx={{
          p: 3,
          borderRadius: 3,
          mb: 4,
          backgroundColor: "rgba(29, 185, 84, 0.05)",
        }}
      >
        <Typography variant="h6" fontWeight="bold" gutterBottom>
          Итоги миграции:
        </Typography>
        <Box sx={{ display: "flex", justifyContent: "space-between", mt: 2 }}>
          <Typography variant="body1">Всего треков в ВК:</Typography>
          <Typography variant="body1" fontWeight="bold">
            {stats.vkTotalCount}
          </Typography>
        </Box>
        <Divider sx={{ my: 1 }} />
        <Box sx={{ display: "flex", justifyContent: "space-between" }}>
          <Typography variant="body1" color="success.main" fontWeight="bold">
            Успешно перенесено:
          </Typography>
          <Typography variant="body1" color="success.main" fontWeight="bold">
            {stats.succesfullyMigratedCount}
          </Typography>
        </Box>
      </Paper>

      <Typography sx={{ mb: 2, color: "text.secondary", fontSize: "0.9rem" }}>
        Если вы видите ошибку Too many requests, повторите синхронизацию через
        24 часа
      </Typography>

      {stats.failedLogsList.length > 0 && (
        <Box sx={{ mt: 4 }}>
          <Typography
            variant="h6"
            color="error.main"
            fontWeight="bold"
            gutterBottom
          >
            Не удалось перенести ({stats.failedLogsList.length})
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Эти треки не были найдены в Spotify или произошла ошибка доступа.
          </Typography>

          <TableContainer
            component={Paper}
            variant="outlined"
            sx={{ maxHeight: 400, borderRadius: 3 }}
          >
            <Table stickyHeader size="small">
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: "bold" }}>Артист</TableCell>
                  <TableCell sx={{ fontWeight: "bold" }}>Трек</TableCell>
                  <TableCell sx={{ fontWeight: "bold" }}>Причина</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {stats.failedLogsList.map((log, index) => (
                  <TableRow key={index} hover>
                    <TableCell>{log.artist}</TableCell>
                    <TableCell>{log.title}</TableCell>
                    <TableCell
                      sx={{ color: "error.main", fontSize: "0.85rem" }}
                    >
                      {log.resultDetails}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}
    </Box>
  );
}
