import { format, startOfMonth } from "date-fns";
import { useState } from "react";
import { Avatar, Stack, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { StatTile } from "../../components/StatTile";
import { useCostAnalysis } from "../../hooks/useCostAnalysis";
import type { CostAnalysisByUser } from "../../types";
import { isForbiddenError } from "../../utils/apiErrors";
import { DateRangeFields } from "./DateRangeFields";
import { formatCurrency, formatHours } from "./reportsMeta";
import { ProjectPickerField } from "./ProjectPickerField";

interface CostAnalysisContentProps {
  projectId: string | undefined;
  onProjectChange?: (projectId: string) => void;
}

export function CostAnalysisContent({ projectId, onProjectChange }: CostAnalysisContentProps) {
  const [from, setFrom] = useState(format(startOfMonth(new Date()), "yyyy-MM-dd"));
  const [to, setTo] = useState(format(new Date(), "yyyy-MM-dd"));

  const { data, isLoading, error } = useCostAnalysis(
    projectId ? { projectId, from, to } : undefined
  );

  const hasRateConfigured = data?.hourlyRateConfigured ?? true;

  const columns: DataTableColumn<CostAnalysisByUser>[] = [
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
      key: "billableHours",
      label: "Billable hours",
      align: "right",
      render: (row) => formatHours(row.billableHours),
    },
    {
      key: "cost",
      label: "Cost",
      align: "right",
      render: (row) => formatCurrency(row.cost, hasRateConfigured),
    },
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap alignItems="center">
        {onProjectChange && (
          <ProjectPickerField value={projectId ?? ""} onChange={onProjectChange} />
        )}
        <DateRangeFields from={from} to={to} onFromChange={setFrom} onToChange={setTo} />
      </Stack>

      {!projectId ? (
        <EmptyState title="Select a project" description="Pick a project to see its cost analysis." />
      ) : isForbiddenError(error) ? (
        <AccessDeniedState />
      ) : isLoading ? (
        <SkeletonCard count={2} />
      ) : (
        <>
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ "& > *": { flex: 1 } }}>
            <StatTile
              label="Billable hours"
              value={formatHours(data?.totalBillableHours)}
              color="text.primary"
            />
            <StatTile
              label="Non-billable hours"
              value={formatHours(data?.totalNonBillableHours)}
              color="text.primary"
            />
            <StatTile
              label="Total cost"
              value={formatCurrency(data?.totalCost, hasRateConfigured)}
              color="success"
            />
          </Stack>

          {!hasRateConfigured && (
            <Typography variant="caption" color="text.secondary">
              This project has no default hourly rate configured, so cost totals show as $0.
              Set a rate in project settings to see real cost figures.
            </Typography>
          )}

          <DataTable
            columns={columns}
            rows={data?.byUser ?? []}
            getRowId={(row) => row.userId}
            totalCount={data?.byUser.length ?? 0}
            page={0}
            pageSize={Math.max(data?.byUser.length ?? 0, 10)}
            onPageChange={() => {}}
            onPageSizeChange={() => {}}
            emptyTitle="No billable time logged"
            emptyDescription="Cost breakdown appears once time is logged in this range."
          />
        </>
      )}
    </Stack>
  );
}

export function CostAnalysisPage() {
  const [projectId, setProjectId] = useState<string>("");

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Cost Analysis
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Billable and non-billable hours with a per-user cost breakdown.
          </Typography>
        </Stack>
        <CostAnalysisContent projectId={projectId || undefined} onProjectChange={setProjectId} />
      </Stack>
    </AppShell>
  );
}
