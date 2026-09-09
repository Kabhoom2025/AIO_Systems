import { useTheme } from "@mui/material/styles";
import { Paper, Stack, Typography } from "@mui/material";
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { useBurndown } from "../../hooks/useBurndown";

interface BurndownChartProps {
  sprintId: string;
}

export function BurndownChart({ sprintId }: BurndownChartProps) {
  const theme = useTheme();
  const { data, isLoading } = useBurndown(sprintId);

  if (isLoading) return <SkeletonCard count={1} />;

  const days = data?.days ?? [];
  if (days.length === 0) {
    return <EmptyState title="No burndown data yet" description="Data appears once the sprint starts." />;
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="subtitle1" fontWeight={700}>
          Burndown
        </Typography>
        <div style={{ width: "100%", height: 300 }}>
          <ResponsiveContainer>
            <LineChart data={days} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} vertical={false} />
              <XAxis
                dataKey="date"
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
              <Line
                type="monotone"
                dataKey="idealRemainingPoints"
                name="Ideal"
                stroke={theme.palette.text.disabled}
                strokeDasharray="5 4"
                strokeWidth={2}
                dot={false}
              />
              <Line
                type="monotone"
                dataKey="remainingPoints"
                name="Actual"
                stroke={theme.palette.primary.main}
                strokeWidth={2}
                dot={{ r: 3 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </Stack>
    </Paper>
  );
}
