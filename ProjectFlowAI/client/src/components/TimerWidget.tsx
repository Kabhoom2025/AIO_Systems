import PlayArrowOutlinedIcon from "@mui/icons-material/PlayArrowOutlined";
import StopOutlinedIcon from "@mui/icons-material/StopOutlined";
import TimerOutlinedIcon from "@mui/icons-material/TimerOutlined";
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Snackbar,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useEffect, useState } from "react";
import { useActiveTimer, useStartTimer, useStopTimer } from "../hooks/useActiveTimer";
import { useProjects } from "../hooks/useProjects";
import { useWorkItems } from "../hooks/useWorkItems";
import { useAuthStore } from "../store/authStore";

function formatElapsed(startedAt: string): string {
  const seconds = Math.max(0, Math.floor((Date.now() - new Date(startedAt).getTime()) / 1000));
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  return [h, m, s].map((n) => String(n).padStart(2, "0")).join(":");
}

export function TimerWidget() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: activeTimer, isLoading } = useActiveTimer();
  const startMutation = useStartTimer();
  const stopMutation = useStopTimer();

  const [now, setNow] = useState(Date.now());
  const [pickerOpen, setPickerOpen] = useState(false);
  const [busySnackbar, setBusySnackbar] = useState(false);

  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);
  const [selectedWorkItemId, setSelectedWorkItemId] = useState<string | null>(null);

  const { data: projectsPage } = useProjects({ organizationId, page: 1, pageSize: 100 });
  const { data: workItemsPage } = useWorkItems({
    projectId: selectedProjectId ?? "",
    page: 1,
    pageSize: 200,
  });

  useEffect(() => {
    if (!activeTimer) return;
    const interval = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(interval);
  }, [activeTimer]);

  // Re-render elapsed display each second while a timer is running.
  void now;

  const handleStart = () => {
    if (!selectedWorkItemId) return;
    startMutation.mutate(
      { workItemId: selectedWorkItemId },
      {
        onSuccess: () => {
          setPickerOpen(false);
          setSelectedProjectId(null);
          setSelectedWorkItemId(null);
        },
        onError: (error: unknown) => {
          const status = (error as { response?: { status?: number } })?.response?.status;
          if (status === 409) {
            setPickerOpen(false);
            setBusySnackbar(true);
          }
        },
      }
    );
  };

  if (isLoading) {
    return <Box sx={{ width: 160 }} />;
  }

  if (activeTimer) {
    return (
      <>
        <Tooltip title={`Tracking time on: ${activeTimer.workItemTitle}`}>
          <Chip
            icon={<TimerOutlinedIcon fontSize="small" />}
            label={
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography variant="caption" noWrap sx={{ maxWidth: 160 }}>
                  {activeTimer.workItemTitle}
                </Typography>
                <Typography variant="caption" fontWeight={700} sx={{ fontVariantNumeric: "tabular-nums" }}>
                  {formatElapsed(activeTimer.startedAt)}
                </Typography>
              </Stack>
            }
            color="primary"
            variant="outlined"
            onDelete={() => stopMutation.mutate()}
            deleteIcon={
              <Tooltip title="Stop timer">
                <StopOutlinedIcon />
              </Tooltip>
            }
            sx={{ height: 32, "& .MuiChip-label": { pr: 0.5 } }}
          />
        </Tooltip>
      </>
    );
  }

  return (
    <>
      <Button
        size="small"
        variant="outlined"
        startIcon={<PlayArrowOutlinedIcon fontSize="small" />}
        onClick={() => setPickerOpen(true)}
      >
        No timer running
      </Button>

      <Dialog open={pickerOpen} onClose={() => setPickerOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Start a timer</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 0.5 }}>
            <Autocomplete
              options={projectsPage?.items ?? []}
              getOptionLabel={(p) => p.name}
              value={(projectsPage?.items ?? []).find((p) => p.id === selectedProjectId) ?? null}
              onChange={(_, value) => {
                setSelectedProjectId(value?.id ?? null);
                setSelectedWorkItemId(null);
              }}
              renderInput={(params) => <TextField {...params} label="Project" size="small" />}
            />
            <Autocomplete
              options={workItemsPage?.items ?? []}
              getOptionLabel={(w) => w.title}
              disabled={!selectedProjectId}
              value={(workItemsPage?.items ?? []).find((w) => w.id === selectedWorkItemId) ?? null}
              onChange={(_, value) => setSelectedWorkItemId(value?.id ?? null)}
              renderInput={(params) => <TextField {...params} label="Work item" size="small" />}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPickerOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={!selectedWorkItemId || startMutation.isPending}
            onClick={handleStart}
          >
            Start
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={busySnackbar} autoHideDuration={4000} onClose={() => setBusySnackbar(false)}>
        <Alert severity="info" onClose={() => setBusySnackbar(false)}>
          You already have a timer running — showing it in the topbar instead.
        </Alert>
      </Snackbar>
    </>
  );
}
