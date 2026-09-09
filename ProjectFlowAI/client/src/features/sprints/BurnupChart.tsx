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
import { useBurnup } from "../../hooks/useBurnup";

interface BurnupChartProps {
  sprintId: string;
}

export function BurnupChart({ sprintId }: BurnupChartProps) {
  const theme = useTheme();
  const { data, isLoading } = useBurnup(sprintId);

  if (isLoading) return <SkeletonCard count={1} />;

  const days = data?.days ?? [];
  if (days.length === 0) {
    return <EmptyState title="No burnup data yet" description="Data appears once the sprint starts." />;
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="subtitle1" fontWeight={700}>
          Burnup
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
                dataKey="totalScopePoints"
                name="Total scope"
                stroke={theme.palette.text.disabled}
                strokeDasharray="5 4"
                strokeWidth={2}
                dot={false}
              />
              <Line
                type="monotone"
                dataKey="completedPoints"
                name="Completed"
                stroke={theme.palette.secondary.main}
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
