import { format, startOfMonth } from "date-fns";
import { useState } from "react";
import { Avatar, Chip, LinearProgress, Stack, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { SkeletonCard } from "../../components/Skeletons";
import { useResourceUtilization } from "../../hooks/useResourceUtilization";
import { useAuthStore } from "../../store/authStore";
import type { ResourceUtilizationRow } from "../../types";
import { isForbiddenError } from "../../utils/apiErrors";
import { DateRangeFields } from "./DateRangeFields";
import { formatHours, formatPercent } from "./reportsMeta";
import { ProjectPickerField } from "./ProjectPickerField";

// Thresholds chosen to read at a glance: comfortably allocated, under-utilized, over-allocated.
// Colors follow the app's existing status convention (success/warning/error).
function utilizationColor(percent: number | null): "success" | "warning" | "error" | "default" {
  if (percent === null) return "default";
  if (percent > 100) return "error";
  if (percent < 70) return "warning";
  return "success";
}

export function ResourceUtilizationPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";
  const [projectId, setProjectId] = useState<string>("");
  const [from, setFrom] = useState(format(startOfMonth(new Date()), "yyyy-MM-dd"));
  const [to, setTo] = useState(format(new Date(), "yyyy-MM-dd"));

  const { data, isLoading, error } = useResourceUtilization(
    organizationId
      ? { organizationId, projectId: projectId || undefined, from, to }
      : undefined
  );

  const columns: DataTableColumn<ResourceUtilizationRow>[] = [
    {
      key: "userName",
      label: "Team member",
      render: (row) => (
        <Stack direction="row" spacing={1.5} alignItems="center">
          <Avatar sx={{ width: 28, height: 28, fontSize: 12, bgcolor: "primary.main" }}>
            {row.userName
              .split(" ")
              .map((p) => p[0])
              .slice(0, 2)
              .join("")
              .toUpperCase()}
          </Avatar>
          <Typography variant="body2">{row.userName}</Typography>
        </Stack>
      ),
    },
    {
      key: "estimatedHours",
      label: "Estimated (h)",
      align: "right",
      render: (row) => formatHours(row.estimatedHours),
    },
    {
      key: "loggedHours",
      label: "Logged (h)",
      align: "right",
      render: (row) => formatHours(row.loggedHours),
    },
    {
      key: "taskCount",
      label: "Tasks",
      align: "right",
      render: (row) => row.taskCount,
    },
    {
      key: "utilizationPercent",
      label: "Utilization",
      render: (row) => {
        const color = utilizationColor(row.utilizationPercent);
        if (row.utilizationPercent === null) {
          return (
            <Chip size="small" label="—" variant="outlined" title="No estimate to compare against" />
          );
        }
        return (
          <Stack sx={{ minWidth: 160 }} spacing={0.5}>
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="caption" color={`${color}.main`} fontWeight={600}>
                {formatPercent(row.utilizationPercent)}
              </Typography>
            </Stack>
            <LinearProgress
              variant="determinate"
              value={Math.min(100, Math.max(0, row.utilizationPercent))}
              color={color === "default" ? "primary" : color}
              sx={{ height: 6, borderRadius: 3 }}
            />
          </Stack>
        );
      },
    },
  ];

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Resource Utilization
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Estimated vs. logged hours per team member across the organization.
          </Typography>
        </Stack>

        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap alignItems="center">
          <ProjectPickerField
            value={projectId}
            onChange={setProjectId}
            label="Project"
            clearLabel="All projects"
          />
          <DateRangeFields from={from} to={to} onFromChange={setFrom} onToChange={setTo} />
        </Stack>

        {isForbiddenError(error) ? (
          <AccessDeniedState />
        ) : isLoading ? (
          <SkeletonCard count={2} />
        ) : (
          <DataTable
            columns={columns}
            rows={data?.rows ?? []}
            getRowId={(row) => row.userId}
            totalCount={data?.rows.length ?? 0}
            page={0}
            pageSize={Math.max(data?.rows.length ?? 0, 10)}
            onPageChange={() => {}}
            onPageSizeChange={() => {}}
            emptyTitle="No utilization data"
            emptyDescription="Utilization appears once team members have estimated or logged hours in this range."
          />
        )}
      </Stack>
    </AppShell>
  );
}
