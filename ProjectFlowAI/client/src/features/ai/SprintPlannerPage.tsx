import BoltOutlinedIcon from "@mui/icons-material/BoltOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import { usePlanSprint } from "../../hooks/usePlanSprint";
import { useSprints } from "../../hooks/useSprints";
import { useSetWorkItemSprint } from "../../hooks/useWorkItems";
import { useWorkItems } from "../../hooks/useWorkItems";
import { AiUnavailableState } from "./AiUnavailableState";

export function SprintPlannerPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [goal, setGoal] = useState("");
  const [capacity, setCapacity] = useState("20");
  const [targetSprintId, setTargetSprintId] = useState("");
  const [applied, setApplied] = useState(false);

  const planSprint = usePlanSprint();
  const setSprint = useSetWorkItemSprint(projectId ?? "");
  const { data: sprintsPage } = useSprints({ projectId: projectId ?? "", page: 1, pageSize: 100 });
  const { data: workItemsPage } = useWorkItems({ projectId: projectId ?? "", page: 1, pageSize: 500 });

  const plannableSprints = (sprintsPage?.items ?? []).filter((s) => s.status !== "Completed");

  const selectedTitles = useMemo(() => {
    if (!planSprint.data) return [];
    const byId = new Map((workItemsPage?.items ?? []).map((w) => [w.id, w]));
    return planSprint.data.selectedWorkItemIds.map((id) => byId.get(id) ?? { id, title: id });
  }, [planSprint.data, workItemsPage]);

  const handlePlan = () => {
    if (!projectId || !goal.trim()) return;
    setApplied(false);
    planSprint.mutate({
      projectId,
      sprintGoal: goal.trim(),
      capacityPoints: Number(capacity) || 0,
    });
  };

  const handleApply = async () => {
    if (!planSprint.data || !targetSprintId) return;
    for (const id of planSprint.data.selectedWorkItemIds) {
      await setSprint.mutateAsync({ id, sprintId: targetSprintId });
    }
    setApplied(true);
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Sprint Planner
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Give the assistant a sprint goal and your team's capacity — it will select which
          backlog items best fit, with its reasoning shown alongside.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <TextField
              label="Sprint goal"
              placeholder="e.g. Ship the onboarding redesign end-to-end"
              fullWidth
              multiline
              minRows={2}
              value={goal}
              onChange={(e) => setGoal(e.target.value)}
            />
            <TextField
              label="Capacity (story points)"
              type="number"
              sx={{ width: 220 }}
              value={capacity}
              onChange={(e) => setCapacity(e.target.value)}
            />
            <Box>
              <Button
                variant="contained"
                startIcon={
                  planSprint.isPending ? <CircularProgress size={16} color="inherit" /> : <BoltOutlinedIcon />
                }
                disabled={!goal.trim() || planSprint.isPending}
                onClick={handlePlan}
              >
                {planSprint.isPending ? "Planning..." : "Plan sprint"}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {planSprint.isError && <AiUnavailableState error={planSprint.error} />}

      {applied && (
        <Alert severity="success" onClose={() => setApplied(false)}>
          Assigned the selected items to the chosen sprint.
        </Alert>
      )}

      {planSprint.data && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="subtitle1" fontWeight={700}>
                Selected items ({planSprint.data.selectedWorkItemIds.length})
              </Typography>
              {selectedTitles.length === 0 ? (
                <EmptyState title="No items selected" description="The assistant didn't select any backlog items." />
              ) : (
                <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                  {selectedTitles.map((w) => (
                    <Chip key={w.id} label={w.title} variant="outlined" />
                  ))}
                </Stack>
              )}

              <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: "pre-wrap" }}>
                {planSprint.data.reasoning}
              </Typography>

              {selectedTitles.length > 0 && (
                <Stack direction="row" spacing={1.5} alignItems="center">
                  <Select
                    size="small"
                    displayEmpty
                    value={targetSprintId}
                    onChange={(e) => setTargetSprintId(e.target.value)}
                    sx={{ minWidth: 220 }}
                  >
                    <MenuItem value="">
                      <em>Choose target sprint</em>
                    </MenuItem>
                    {plannableSprints.map((s) => (
                      <MenuItem key={s.id} value={s.id}>
                        {s.name}
                      </MenuItem>
                    ))}
                  </Select>
                  <Button
                    variant="contained"
                    disabled={!targetSprintId || setSprint.isPending}
                    startIcon={setSprint.isPending ? <CircularProgress size={14} color="inherit" /> : undefined}
                    onClick={handleApply}
                  >
                    Apply to sprint
                  </Button>
                </Stack>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
