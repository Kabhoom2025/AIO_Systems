import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import FlagOutlinedIcon from "@mui/icons-material/FlagOutlined";
import PlayArrowOutlinedIcon from "@mui/icons-material/PlayArrowOutlined";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ViewKanbanOutlinedIcon from "@mui/icons-material/ViewKanbanOutlined";
import ForumOutlinedIcon from "@mui/icons-material/ForumOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Tab,
  Tabs,
  Tooltip,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { FormTextField } from "../../components/FormTextField";
import {
  useCompleteSprint,
  useCreateSprint,
  useDeleteSprint,
  useSprints,
  useStartSprint,
  useUpdateSprint,
} from "../../hooks/useSprints";
import type { Sprint } from "../../types";
import { SPRINT_STATUSES } from "../../types";
import { SPRINT_STATUS_COLOR } from "./sprintMeta";
import { BacklogPanel } from "./BacklogPanel";
import { VelocityChart } from "./VelocityChart";

const sprintSchema = z
  .object({
    name: z.string().min(1, "Name is required"),
    goal: z.string().optional().or(z.literal("")),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().min(1, "End date is required"),
  })
  .refine((v) => !v.startDate || !v.endDate || v.endDate > v.startDate, {
    message: "End date must be after start date",
    path: ["endDate"],
  });

type SprintFormValues = z.infer<typeof sprintSchema>;

function extractApiError(error: unknown): string | null {
  const err = error as { response?: { status?: number; data?: { detail?: string; title?: string } } };
  if (!err?.response) return null;
  if (err.response.status === 409) {
    return (
      err.response.data?.detail ??
      "This project already has another active sprint. Complete it before starting a new one."
    );
  }
  return err.response.data?.detail ?? err.response.data?.title ?? null;
}

export function SprintsPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<"sprints" | "backlog" | "velocity">("sprints");
  const [statusFilter, setStatusFilter] = useState<string>("");

  const { data, isLoading } = useSprints({
    projectId: projectId ?? "",
    status: statusFilter || undefined,
    page: 1,
    pageSize: 100,
    sortBy: "startDate",
    sortDir: "desc",
  });

  const createMutation = useCreateSprint(projectId ?? "");
  const updateMutation = useUpdateSprint(projectId ?? "");
  const startMutation = useStartSprint(projectId ?? "");
  const completeMutation = useCompleteSprint(projectId ?? "");
  const deleteMutation = useDeleteSprint(projectId ?? "");

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Sprint | null>(null);
  const [startTarget, setStartTarget] = useState<Sprint | null>(null);
  const [completeTarget, setCompleteTarget] = useState<Sprint | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Sprint | null>(null);
  const [startError, setStartError] = useState<string | null>(null);

  const { control, handleSubmit, reset } = useForm<SprintFormValues>({
    resolver: zodResolver(sprintSchema),
    defaultValues: { name: "", goal: "", startDate: "", endDate: "" },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", goal: "", startDate: "", endDate: "" });
    setDialogOpen(true);
  };

  const openEdit = (s: Sprint) => {
    setEditing(s);
    reset({
      name: s.name,
      goal: s.goal ?? "",
      startDate: s.startDate.slice(0, 10),
      endDate: s.endDate.slice(0, 10),
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: SprintFormValues) => {
    if (!projectId) return;
    const payload = {
      name: values.name,
      goal: values.goal || undefined,
      startDate: values.startDate,
      endDate: values.endDate,
    };
    if (editing) {
      updateMutation.mutate({ id: editing.id, payload }, { onSuccess: () => setDialogOpen(false) });
    } else {
      createMutation.mutate({ projectId, ...payload }, { onSuccess: () => setDialogOpen(false) });
    }
  };

  const sprints = data?.items ?? [];

  const formError = extractApiError(createMutation.error ?? updateMutation.error);

  const orderedSprints = useMemo(
    () => [...sprints].sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime()),
    [sprints]
  );

  return (
    <Stack spacing={2}>
      <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
        <Tabs value={tab} onChange={(_, v) => setTab(v)}>
          <Tab label="Sprints" value="sprints" />
          <Tab label="Backlog" value="backlog" />
          <Tab label="Velocity" value="velocity" />
        </Tabs>
      </Box>

      {tab === "sprints" && (
        <Stack spacing={2}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Select
              size="small"
              displayEmpty
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              sx={{ minWidth: 160 }}
            >
              <MenuItem value="">All statuses</MenuItem>
              {SPRINT_STATUSES.map((s) => (
                <MenuItem key={s} value={s}>
                  {s}
                </MenuItem>
              ))}
            </Select>
            <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
              New sprint
            </Button>
          </Stack>

          {startError && (
            <Alert severity="error" onClose={() => setStartError(null)}>
              {startError}
            </Alert>
          )}

          {isLoading ? (
            <SkeletonCard count={3} />
          ) : orderedSprints.length === 0 ? (
            <EmptyState
              title="No sprints yet"
              description="Create a sprint to start planning work in time-boxed iterations."
              actionLabel="New sprint"
              onAction={openCreate}
            />
          ) : (
            <Stack spacing={1.5}>
              {orderedSprints.map((s) => {
                const pointsPct = s.totalStoryPoints > 0 ? (s.completedStoryPoints / s.totalStoryPoints) * 100 : 0;
                const itemsPct = s.workItemCount > 0 ? (s.completedWorkItemCount / s.workItemCount) * 100 : 0;
                return (
                  <Card key={s.id} variant="outlined">
                    <CardContent>
                      <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                        <Stack spacing={0.5} sx={{ flex: 1 }}>
                          <Stack direction="row" spacing={1} alignItems="center">
                            <Typography variant="subtitle1" fontWeight={700}>
                              {s.name}
                            </Typography>
                            <Chip size="small" label={s.status} color={SPRINT_STATUS_COLOR[s.status]} />
                          </Stack>
                          {s.goal && (
                            <Typography variant="body2" color="text.secondary">
                              {s.goal}
                            </Typography>
                          )}
                          <Typography variant="caption" color="text.secondary">
                            {s.startDate.slice(0, 10)} – {s.endDate.slice(0, 10)}
                          </Typography>
                        </Stack>
                        <Stack direction="row" spacing={0.5}>
                          <Tooltip title="Sprint board">
                            <IconButton
                              size="small"
                              onClick={() => navigate(`/projects/${projectId}/sprints/${s.id}/board`)}
                            >
                              <ViewKanbanOutlinedIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Retrospective">
                            <IconButton
                              size="small"
                              onClick={() => navigate(`/projects/${projectId}/sprints/${s.id}/retrospective`)}
                            >
                              <ForumOutlinedIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          {s.status === "Planned" && (
                            <Tooltip title="Start sprint">
                              <IconButton size="small" onClick={() => setStartTarget(s)}>
                                <PlayArrowOutlinedIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          {s.status === "Active" && (
                            <Tooltip title="Complete sprint">
                              <IconButton size="small" onClick={() => setCompleteTarget(s)}>
                                <CheckCircleOutlineIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          )}
                          <Button size="small" onClick={() => openEdit(s)}>
                            Edit
                          </Button>
                          <IconButton size="small" onClick={() => setDeleteTarget(s)}>
                            <DeleteOutlineIcon fontSize="small" />
                          </IconButton>
                        </Stack>
                      </Stack>

                      <Stack spacing={1} sx={{ mt: 1.5 }}>
                        <Stack direction="row" justifyContent="space-between">
                          <Typography variant="caption" color="text.secondary">
                            Story points: {s.completedStoryPoints}/{s.totalStoryPoints}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            Items: {s.completedWorkItemCount}/{s.workItemCount}
                          </Typography>
                        </Stack>
                        <LinearProgress variant="determinate" value={pointsPct} />
                        <LinearProgress variant="determinate" value={itemsPct} color="secondary" />
                      </Stack>
                    </CardContent>
                  </Card>
                );
              })}
            </Stack>
          )}
        </Stack>
      )}

      {tab === "backlog" && projectId && <BacklogPanel projectId={projectId} sprints={sprints} />}

      {tab === "velocity" && projectId && <VelocityChart projectId={projectId} />}

      {/* Create / edit dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? "Edit sprint" : "New sprint"}</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {formError && <Alert severity="error">{formError}</Alert>}
              <FormTextField name="name" control={control} label="Name" />
              <FormTextField name="goal" control={control} label="Goal" multiline rows={2} />
              <Stack direction="row" spacing={2}>
                <FormTextField
                  name="startDate"
                  control={control}
                  label="Start date"
                  type="date"
                  textFieldProps={{ slotProps: { inputLabel: { shrink: true } } }}
                />
                <FormTextField
                  name="endDate"
                  control={control}
                  label="End date"
                  type="date"
                  textFieldProps={{ slotProps: { inputLabel: { shrink: true } } }}
                />
              </Stack>
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createMutation.isPending || updateMutation.isPending}
              startIcon={
                createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={16} />
                ) : (
                  <FlagOutlinedIcon fontSize="small" />
                )
              }
            >
              Save
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <ConfirmDialog
        open={!!startTarget}
        title="Start sprint"
        message={`Start "${startTarget?.name}"? A project can only have one active sprint at a time.`}
        confirmLabel="Start"
        loading={startMutation.isPending}
        onConfirm={() => {
          if (startTarget) {
            startMutation.mutate(startTarget.id, {
              onSuccess: () => {
                setStartTarget(null);
                setStartError(null);
              },
              onError: (error) => {
                setStartTarget(null);
                setStartError(
                  extractApiError(error) ?? "Could not start this sprint. Please try again."
                );
              },
            });
          }
        }}
        onCancel={() => setStartTarget(null)}
      />

      <ConfirmDialog
        open={!!completeTarget}
        title="Complete sprint"
        message={`Complete "${completeTarget?.name}"? Incomplete items will remain assigned to this sprint unless moved.`}
        confirmLabel="Complete"
        loading={completeMutation.isPending}
        onConfirm={() => {
          if (completeTarget) {
            completeMutation.mutate(completeTarget.id, { onSuccess: () => setCompleteTarget(null) });
          }
        }}
        onCancel={() => setCompleteTarget(null)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete sprint"
        message={`Delete "${deleteTarget?.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        destructive
        loading={deleteMutation.isPending}
        onConfirm={() => {
          if (deleteTarget) {
            deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
          }
        }}
        onCancel={() => setDeleteTarget(null)}
      />
    </Stack>
  );
}
