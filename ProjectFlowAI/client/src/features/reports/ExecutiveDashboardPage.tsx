import FolderOutlinedIcon from "@mui/icons-material/FolderOutlined";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Chip, LinearProgress, Link, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { SkeletonCard } from "../../components/Skeletons";
import { StatTile } from "../../components/StatTile";
import { useExecutiveDashboard } from "../../hooks/useExecutiveDashboard";
import { useAuthStore } from "../../store/authStore";
import type { ProjectHealthSummary } from "../../types";
import { isForbiddenError } from "../../utils/apiErrors";
import { HealthChip } from "./reportsMeta";

export function ExecutiveDashboardPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId;
  const navigate = useNavigate();
  const { data, isLoading, error } = useExecutiveDashboard(organizationId);

  const columns: DataTableColumn<ProjectHealthSummary>[] = [
    {
      key: "projectName",
      label: "Project",
      render: (row) => (
        <Link
          component="button"
          underline="hover"
          onClick={() => navigate(`/reports/project-health?projectId=${row.projectId}`)}
        >
          {row.projectName}
        </Link>
      ),
    },
    { key: "health", label: "Health", render: (row) => <HealthChip health={row.health} /> },
    {
      key: "progressPercent",
      label: "Progress",
      render: (row) => (
        <Stack sx={{ minWidth: 140 }} spacing={0.5}>
          <LinearProgress
            variant="determinate"
            value={Math.min(100, Math.max(0, row.progressPercent))}
            sx={{ height: 6, borderRadius: 3 }}
          />
          <Typography variant="caption" color="text.secondary">
            {Math.round(row.progressPercent)}%
          </Typography>
        </Stack>
      ),
    },
    {
      key: "overdueCount",
      label: "Overdue",
      align: "right",
      render: (row) => (
        <Chip
          size="small"
          label={row.overdueCount}
          color={row.overdueCount > 0 ? "error" : "default"}
          variant={row.overdueCount > 0 ? "filled" : "outlined"}
        />
      ),
    },
    {
      key: "activeSprintName",
      label: "Active sprint",
      render: (row) => row.activeSprintName ?? "—",
    },
  ];

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Executive Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Organization-wide project health and delivery KPIs.
          </Typography>
        </Stack>

        {isForbiddenError(error) ? (
          <AccessDeniedState />
        ) : isLoading ? (
          <SkeletonCard count={3} />
        ) : (
          <>
            <Stack
              direction={{ xs: "column", sm: "row" }}
              spacing={2}
              sx={{ "& > *": { flex: 1 } }}
            >
              <StatTile
                label="Total projects"
                value={data?.totalProjects ?? 0}
                icon={<FolderOutlinedIcon color="action" />}
              />
              <StatTile
                label="Active projects"
                value={data?.activeProjects ?? 0}
                color="text.primary"
              />
              <StatTile
                label="Total work items"
                value={data?.totalWorkItems ?? 0}
                color="text.primary"
              />
              <StatTile
                label="Completed this month"
                value={data?.completedThisMonth ?? 0}
                color="success"
                icon={<CheckCircleOutlineIcon color="success" />}
              />
              <StatTile
                label="Overdue"
                value={data?.overdueCount ?? 0}
                color={data && data.overdueCount > 0 ? "error" : "text.primary"}
                icon={<ErrorOutlineIcon color={data && data.overdueCount > 0 ? "error" : "disabled"} />}
              />
              <StatTile
                label="At-risk projects"
                value={data?.atRiskProjectCount ?? 0}
                color={data && data.atRiskProjectCount > 0 ? "warning" : "text.primary"}
                icon={
                  <WarningAmberOutlinedIcon
                    color={data && data.atRiskProjectCount > 0 ? "warning" : "disabled"}
                  />
                }
              />
            </Stack>

            <Stack spacing={1}>
              <Typography variant="subtitle1" fontWeight={700}>
                Project health
              </Typography>
              <DataTable
                columns={columns}
                rows={data?.projectHealthSummaries ?? []}
                getRowId={(row) => row.projectId}
                totalCount={data?.projectHealthSummaries.length ?? 0}
                page={0}
                pageSize={Math.max(data?.projectHealthSummaries.length ?? 0, 10)}
                onPageChange={() => {}}
                onPageSizeChange={() => {}}
                emptyTitle="No projects yet"
                emptyDescription="Project health appears here once projects have work items."
              />
            </Stack>
          </>
        )}
      </Stack>
    </AppShell>
  );
}
