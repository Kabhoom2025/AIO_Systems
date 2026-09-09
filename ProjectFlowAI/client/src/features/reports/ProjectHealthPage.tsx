import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import BlockOutlinedIcon from "@mui/icons-material/BlockOutlined";
import { Card, CardContent, LinearProgress, List, ListItem, ListItemIcon, ListItemText, Stack, Typography } from "@mui/material";
import { useSearchParams } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { AccessDeniedState } from "../../components/AccessDeniedState";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { useProjectHealth } from "../../hooks/useProjectHealth";
import { isForbiddenError } from "../../utils/apiErrors";
import { HEALTH_COLOR, HEALTH_LABEL, HealthChip } from "./reportsMeta";
import { ProjectPickerField } from "./ProjectPickerField";

interface ProjectHealthContentProps {
  projectId: string | undefined;
  onProjectChange?: (projectId: string) => void;
}

export function ProjectHealthContent({ projectId, onProjectChange }: ProjectHealthContentProps) {
  const { data, isLoading, error } = useProjectHealth(projectId);

  return (
    <Stack spacing={2}>
      {onProjectChange && (
        <ProjectPickerField value={projectId ?? ""} onChange={onProjectChange} />
      )}

      {!projectId ? (
        <EmptyState title="Select a project" description="Pick a project to see its health." />
      ) : isForbiddenError(error) ? (
        <AccessDeniedState />
      ) : isLoading ? (
        <SkeletonCard count={1} />
      ) : !data ? (
        <EmptyState title="No health data" description="This project has no health data yet." />
      ) : (
        <Card variant="outlined" sx={{ borderLeft: 6, borderLeftColor: `${HEALTH_COLOR[data.status]}.main` }}>
          <CardContent>
            <Stack spacing={2.5}>
              <Stack direction="row" alignItems="center" justifyContent="space-between">
                <Typography variant="h6" fontWeight={700}>
                  {HEALTH_LABEL[data.status]}
                </Typography>
                <HealthChip health={data.status} size="medium" />
              </Stack>

              <Stack direction={{ xs: "column", sm: "row" }} spacing={3}>
                <Stack sx={{ flex: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    Overdue items
                  </Typography>
                  <Typography variant="h5" fontWeight={700} color={data.overdueCount > 0 ? "error.main" : "text.primary"}>
                    {data.overdueCount}
                  </Typography>
                </Stack>
                <Stack sx={{ flex: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    Blocked items
                  </Typography>
                  <Typography variant="h5" fontWeight={700} color={data.blockedCount > 0 ? "warning.main" : "text.primary"}>
                    {data.blockedCount}
                  </Typography>
                </Stack>
                <Stack sx={{ flex: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    Sprint progress
                  </Typography>
                  {data.sprintProgressPercent === null ? (
                    <Typography variant="h5" fontWeight={700} color="text.disabled">
                      —
                    </Typography>
                  ) : (
                    <Stack spacing={0.5} sx={{ mt: 0.5 }}>
                      <LinearProgress
                        variant="determinate"
                        value={Math.min(100, Math.max(0, data.sprintProgressPercent))}
                        sx={{ height: 8, borderRadius: 4 }}
                      />
                      <Typography variant="caption" color="text.secondary">
                        {Math.round(data.sprintProgressPercent)}%
                      </Typography>
                    </Stack>
                  )}
                </Stack>
              </Stack>

              <Stack spacing={1}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Why this status
                </Typography>
                {data.reasons.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">
                    No specific concerns flagged — this project looks healthy.
                  </Typography>
                ) : (
                  <List dense disablePadding>
                    {data.reasons.map((reason, i) => (
                      <ListItem key={i} disableGutters>
                        <ListItemIcon sx={{ minWidth: 32 }}>
                          {data.status === "Red" ? (
                            <ErrorOutlineIcon fontSize="small" color="error" />
                          ) : (
                            <BlockOutlinedIcon fontSize="small" color="warning" />
                          )}
                        </ListItemIcon>
                        <ListItemText primary={reason} />
                      </ListItem>
                    ))}
                  </List>
                )}
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}

export function ProjectHealthPage() {
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
            Project Health
          </Typography>
          <Typography variant="body2" color="text.secondary">
            A focused health check for a single project, with the reasons spelled out.
          </Typography>
        </Stack>
        <ProjectHealthContent projectId={projectId} onProjectChange={handleProjectChange} />
      </Stack>
    </AppShell>
  );
}
