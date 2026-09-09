import InboxOutlinedIcon from "@mui/icons-material/InboxOutlined";
import { Box, Button, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export function EmptyState({ icon, title, description, actionLabel, onAction }: EmptyStateProps) {
  return (
    <Stack
      alignItems="center"
      justifyContent="center"
      spacing={1.5}
      sx={{
        py: 8,
        px: 3,
        textAlign: "center",
        color: "text.secondary",
      }}
    >
      <Box sx={{ fontSize: 48, color: "text.disabled", display: "flex" }}>
        {icon ?? <InboxOutlinedIcon sx={{ fontSize: 48 }} />}
      </Box>
      <Typography variant="h6" color="text.primary">
        {title}
      </Typography>
      {description && (
        <Typography variant="body2" sx={{ maxWidth: 400 }}>
          {description}
        </Typography>
      )}
      {actionLabel && onAction && (
        <Button variant="contained" onClick={onAction} sx={{ mt: 1 }}>
          {actionLabel}
        </Button>
      )}
    </Stack>
  );
}
