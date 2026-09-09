import { format, startOfMonth } from "date-fns";
import { useState } from "react";
import { Stack, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { StatTile } from "../../components/StatTile";
import { useCycleTimeReport } from "../../hooks/useCycleTimeReport";
import type { CycleTimeItem } from "../../types";
import { isForbiddenError } from "../../utils/apiErrors";
import { formatHours } from "./reportsMeta";
import { DateRangeFields } from "./DateRangeFields";
import { ProjectPickerField } from "./ProjectPickerField";

interface CycleTimeContentProps {
  projectId: string | undefined;
  onProjectChange?: (projectId: string) => void;
}

export function CycleTimeContent({ projectId, onProjectChange }: CycleTimeContentProps) {
  const [from, setFrom] = useState(format(startOfMonth(new Date()), "yyyy-MM-dd"));
  const [to, setTo] = useState(format(new Date(), "yyyy-MM-dd"));

  const { data, isLoading, error } = useCycleTimeReport(
    projectId ? { projectId, from, to } : undefined
  );

  const columns: DataTableColumn<CycleTimeItem>[] = [
    { key: "title", label: "Work item", render: (row) => row.title },
    {
      key: "leadTimeHours",
      label: "Lead time (h)",
      align: "right",
      render: (row) => formatHours(row.leadTimeHours),
    },
    {
      key: "cycleTimeHours",
      label: "Cycle time (h)",
      align: "right",
      render: (row) => formatHours(row.cycleTimeHours),
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
        <EmptyState title="Select a project" description="Pick a project to see its cycle time report." />
      ) : isForbiddenError(error) ? (
        <AccessDeniedState />
      ) : isLoading ? (
        <SkeletonCard count={2} />
      ) : (
        <>
          <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ "& > *": { flex: 1 } }}>
            <StatTile
              label="Average lead time"
              value={`${formatHours(data?.averageLeadTimeHours)} h`}
              color="text.primary"
            />
            <StatTile
              label="Average cycle time"
              value={`${formatHours(data?.averageCycleTimeHours)} h`}
              color="text.primary"
            />
          </Stack>

          <DataTable
            columns={columns}
            rows={data?.items ?? []}
            getRowId={(row) => row.workItemId}
            totalCount={data?.items.length ?? 0}
            page={0}
            pageSize={Math.max(data?.items.length ?? 0, 10)}
            onPageChange={() => {}}
            onPageSizeChange={() => {}}
            emptyTitle="No completed items in range"
            emptyDescription="Cycle time is measured on items that have moved through the workflow in this date range."
          />
        </>
      )}
    </Stack>
  );
}

export function CycleTimeReportPage() {
  const [projectId, setProjectId] = useState<string>("");

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Cycle Time
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Average lead and cycle time per completed work item.
          </Typography>
        </Stack>
        <CycleTimeContent projectId={projectId || undefined} onProjectChange={setProjectId} />
      </Stack>
    </AppShell>
  );
}
