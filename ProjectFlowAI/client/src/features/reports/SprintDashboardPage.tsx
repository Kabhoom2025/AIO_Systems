import FlagOutlinedIcon from "@mui/icons-material/FlagOutlined";
import { Chip, FormControl, Grid, InputLabel, MenuItem, Select, Stack, Typography } from "@mui/material";
import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { StatTile } from "../../components/StatTile";
import { useSprints } from "../../hooks/useSprints";
import { useSprintDashboard } from "../../hooks/useSprintDashboard";
import { isForbiddenError } from "../../utils/apiErrors";
import { BurndownChart } from "../sprints/BurndownChart";
import { SPRINT_STATUS_COLOR } from "../sprints/sprintMeta";
import { TaskCard } from "../projects/TaskCard";
import { ProjectPickerField } from "./ProjectPickerField";

interface SprintDashboardContentProps {
  projectId: string | undefined;
  onProjectChange?: (projectId: string) => void;
}

export function SprintDashboardContent({ projectId, onProjectChange }: SprintDashboardContentProps) {
  const [searchParams, setSearchParams] = useSearchParams();
  const sprintId = searchParams.get("sprintId") ?? undefined;

  const { data: sprintsPage, isLoading: sprintsLoading } = useSprints({
    projectId: projectId ?? "",
    page: 1,
    pageSize: 100,
  });
  const sprints = sprintsPage?.items ?? [];

  // Auto-select the most recently created sprint (typically the active one) once the
  // project's sprints load and nothing is selected yet.
  useEffect(() => {
    if (!sprintId && sprints.length > 0) {
      const next = new URLSearchParams(searchParams);
      next.set("sprintId", sprints[0].id);
      setSearchParams(next, { replace: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projectId, sprints.length]);

  const { data, isLoading, error } = useSprintDashboard(sprintId);

  const handleSprintChange = (id: string) => {
    const next = new URLSearchParams(searchParams);
    next.set("sprintId", id);
    setSearchParams(next, { replace: true });
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
        {onProjectChange && (
          <ProjectPickerField value={projectId ?? ""} onChange={onProjectChange} />
        )}
        <FormControl size="small" sx={{ minWidth: 220 }} disabled={!projectId || sprintsLoading}>
          <InputLabel id="sprint-picker-label">Sprint</InputLabel>
          <Select
            labelId="sprint-picker-label"
            label="Sprint"
            value={sprintId ?? ""}
            onChange={(e) => handleSprintChange(e.target.value)}
            displayEmpty
          >
            {sprints.length === 0 && (
              <MenuItem value="" disabled>
                {sprintsLoading ? "Loading…" : "No sprints"}
              </MenuItem>
            )}
            {sprints.map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {s.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Stack>

      {!projectId ? (
        <EmptyState title="Select a project" description="Pick a project to see its sprints." />
      ) : !sprintId ? (
        <EmptyState title="No sprint selected" description="Pick a sprint to see its dashboard." />
      ) : isForbiddenError(error) ? (
        <AccessDeniedState />
      ) : isLoading ? (
        <SkeletonCard count={2} />
      ) : !data ? (
        <EmptyState title="No data" description="This sprint has no report data yet." />
      ) : (
        <Stack spacing={2}>
          <Stack direction="row" alignItems="center" spacing={1.5} flexWrap="wrap" useFlexGap>
            <Typography variant="h6" fontWeight={700}>
              {data.sprint.name}
            </Typography>
            <Chip size="small" label={data.sprint.status} color={SPRINT_STATUS_COLOR[data.sprint.status]} />
            {data.sprint.goal && (
              <Typography variant="body2" color="text.secondary">
                {data.sprint.goal}
              </Typography>
            )}
          </Stack>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatTile
                label="Story points"
                value={`${data.sprint.completedStoryPoints}/${data.sprint.totalStoryPoints}`}
                color="text.primary"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatTile
                label="Work items"
                value={`${data.sprint.completedWorkItemCount}/${data.sprint.workItemCount}`}
                color="text.primary"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatTile
                label="Open retro action items"
                value={data.openRetroActionItemCount}
                icon={<FlagOutlinedIcon color={data.openRetroActionItemCount > 0 ? "warning" : "disabled"} />}
                color={data.openRetroActionItemCount > 0 ? "warning" : "text.primary"}
              />
            </Grid>
          </Grid>

          <BurndownChart sprintId={sprintId} />

          <Stack spacing={1}>
            <Typography variant="subtitle1" fontWeight={700}>
              Blocked items ({data.blockedItems.length})
            </Typography>
            {data.blockedItems.length === 0 ? (
              <EmptyState title="Nothing blocked" description="No work items are currently blocked in this sprint." />
            ) : (
              <Stack spacing={1}>
                {data.blockedItems.map((item) => (
                  <TaskCard key={item.id} card={item} />
                ))}
              </Stack>
            )}
          </Stack>
        </Stack>
      )}
    </Stack>
  );
}

export function SprintDashboardPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const projectId = searchParams.get("projectId") ?? undefined;

  const handleProjectChange = (id: string) => {
    setSearchParams({ projectId: id });
  };

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Sprint Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Burndown, blocked items, and retro follow-ups for a single sprint.
          </Typography>
        </Stack>
        <SprintDashboardContent projectId={projectId} onProjectChange={handleProjectChange} />
      </Stack>
    </AppShell>
  );
}
