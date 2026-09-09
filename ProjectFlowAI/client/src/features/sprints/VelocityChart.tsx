import { useTheme } from "@mui/material/styles";
import { Paper, Stack, Typography } from "@mui/material";
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
import { SkeletonCard } from "../../components/Skeletons";
import { useVelocity } from "../../hooks/useVelocity";

interface VelocityChartProps {
  projectId: string;
}

export function VelocityChart({ projectId }: VelocityChartProps) {
  const theme = useTheme();
  const { data, isLoading } = useVelocity(projectId);

  if (isLoading) return <SkeletonCard count={1} />;

  const sprints = data?.sprints ?? [];
  if (sprints.length === 0) {
    return (
      <EmptyState
        title="No velocity data yet"
        description="Complete at least one sprint to see committed vs. completed points."
      />
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="subtitle1" fontWeight={700}>
          Velocity
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Committed vs. completed story points per sprint.
        </Typography>
        <div style={{ width: "100%", height: 320 }}>
          <ResponsiveContainer>
            <BarChart data={sprints} barGap={4} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} vertical={false} />
              <XAxis
                dataKey="sprintName"
                stroke={theme.palette.text.secondary}
                tick={{ fontSize: 12 }}
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
                dataKey="committedPoints"
                name="Committed"
                fill={theme.palette.primary.main}
                radius={[4, 4, 0, 0]}
              />
              <Bar
                dataKey="completedPoints"
                name="Completed"
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
