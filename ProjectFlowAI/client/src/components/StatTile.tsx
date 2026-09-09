import { Card, CardContent, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";

interface StatTileProps {
  label: string;
  value: ReactNode;
  icon?: ReactNode;
  color?: "primary" | "success" | "warning" | "error" | "text.primary";
  helperText?: string;
}

/** The single stat-tile building block reused across the dashboard and every report
 * page, so KPI rows read as one system rather than each page reinventing spacing. */
export function StatTile({ label, value, icon, color = "primary", helperText }: StatTileProps) {
  const valueColor = color === "text.primary" ? "text.primary" : `${color}.main`;
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Typography variant="body2" color="text.secondary">
            {label}
          </Typography>
          {icon}
        </Stack>
        <Typography variant="h4" fontWeight={700} color={valueColor} sx={{ mt: 0.5 }}>
          {value}
        </Typography>
        {helperText && (
          <Typography variant="caption" color="text.secondary">
            {helperText}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}
