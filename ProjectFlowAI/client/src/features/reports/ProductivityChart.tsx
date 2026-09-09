import { useTheme } from "@mui/material/styles";
import { Paper, Stack } from "@mui/material";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { EmptyState } from "../../components/EmptyState";
import type { ProductivityBucket } from "../../types";

interface ProductivityChartProps {
  buckets: ProductivityBucket[];
}

/** Follows the same bar-chart convention as the existing VelocityChart (Phase 3):
 * shared axis, primary/secondary series colors, legend since there are 2 series. */
export function ProductivityChart({ buckets }: ProductivityChartProps) {
  const theme = useTheme();

  if (buckets.length === 0) {
    return (
      <EmptyState
        title="No completed work in range"
        description="Completed items and story points will chart here once work is done in this period."
      />
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <div style={{ width: "100%", height: 320 }}>
          <ResponsiveContainer>
            <BarChart data={buckets} barGap={4} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} vertical={false} />
              <XAxis
                dataKey="periodStart"
                stroke={theme.palette.text.secondary}
                tick={{ fontSize: 11 }}
                tickFormatter={(v: string) => v.slice(5, 10)}
              />
              <YAxis stroke={theme.palette.text.secondary} tick={{ fontSize: 12 }} allowDecimals={false} />
              <Tooltip
                contentStyle={{
                  backgroundColor: theme.palette.background.paper,
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 8,
                }}
              />
              <Legend />
              <Bar
                dataKey="completedCount"
                name="Completed items"
                fill={theme.palette.primary.main}
                radius={[4, 4, 0, 0]}
              />
              <Bar
                dataKey="completedPoints"
                name="Completed points"
                fill={theme.palette.secondary.main}
                radius={[4, 4, 0, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Stack>
    </Paper>
  );
}
