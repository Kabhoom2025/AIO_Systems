import { Box, Paper, Stack, Typography } from "@mui/material";
import { motion } from "framer-motion";
import type { ReactNode } from "react";

interface AuthLayoutProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  maxWidth?: number;
}

export function AuthLayout({ title, subtitle, children, maxWidth = 420 }: AuthLayoutProps) {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        bgcolor: "background.default",
        px: 2,
        py: 4,
      }}
    >
      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.25 }}
        style={{ width: "100%", maxWidth }}
      >
        <Stack spacing={3} alignItems="center" sx={{ mb: 3 }}>
          <Typography variant="h5" fontWeight={700} color="primary.main">
            ProjectFlow AI
          </Typography>
        </Stack>
        <Paper variant="outlined" sx={{ p: 4, borderRadius: 3 }}>
          <Stack spacing={0.5} sx={{ mb: 3 }}>
            <Typography variant="h5" fontWeight={700}>
              {title}
            </Typography>
            {subtitle && (
              <Typography variant="body2" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Stack>
          {children}
        </Paper>
      </motion.div>
    </Box>
  );
}
