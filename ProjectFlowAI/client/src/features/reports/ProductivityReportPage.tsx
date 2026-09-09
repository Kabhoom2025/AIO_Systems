import { format, subMonths } from "date-fns";
import { useState } from "react";
import { Stack, ToggleButton, ToggleButtonGroup, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { useProductivityReport } from "../../hooks/useProductivityReport";
import type { ProductivityBucketSize } from "../../types";
import { isForbiddenError } from "../../utils/apiErrors";
import { DateRangeFields } from "./DateRangeFields";
import { ProductivityChart } from "./ProductivityChart";
import { ProjectPickerField } from "./ProjectPickerField";

interface ProductivityContentProps {
  projectId: string | undefined;
  onProjectChange?: (projectId: string) => void;
}

export function ProductivityContent({ projectId, onProjectChange }: ProductivityContentProps) {
  const [from, setFrom] = useState(format(subMonths(new Date(), 3), "yyyy-MM-dd"));
  const [to, setTo] = useState(format(new Date(), "yyyy-MM-dd"));
  const [bucket, setBucket] = useState<ProductivityBucketSize>("week");

  const { data, isLoading, error } = useProductivityReport(
    projectId ? { projectId, from, to, bucket } : undefined
  );

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap alignItems="center">
        {onProjectChange && (
          <ProjectPickerField value={projectId ?? ""} onChange={onProjectChange} />
        )}
        <DateRangeFields from={from} to={to} onFromChange={setFrom} onToChange={setTo} />
        <ToggleButtonGroup
          size="small"
          exclusive
          value={bucket}
          onChange={(_, value) => value && setBucket(value)}
        >
          <ToggleButton value="week">Weekly</ToggleButton>
          <ToggleButton value="month">Monthly</ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      {!projectId ? (
        <EmptyState title="Select a project" description="Pick a project to see its productivity trend." />
      ) : isForbiddenError(error) ? (
        <AccessDeniedState />
      ) : isLoading ? (
        <SkeletonCard count={1} />
      ) : (
        <ProductivityChart buckets={data?.buckets ?? []} />
      )}
    </Stack>
  );
}

export function ProductivityReportPage() {
  const [projectId, setProjectId] = useState<string>("");

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Productivity
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Completed items and story points per week or month.
          </Typography>
        </Stack>
        <ProductivityContent projectId={projectId || undefined} onProjectChange={setProjectId} />
      </Stack>
    </AppShell>
  );
}
