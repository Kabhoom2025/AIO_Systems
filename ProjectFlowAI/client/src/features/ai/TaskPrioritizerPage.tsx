import SsidChartOutlinedIcon from "@mui/icons-material/SsidChartOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import { usePrioritizeTasks } from "../../hooks/usePrioritizeTasks";
import { useUpdateWorkItem, useWorkItems } from "../../hooks/useWorkItems";
import { AiUnavailableState } from "./AiUnavailableState";

export function TaskPrioritizerPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [appliedCount, setAppliedCount] = useState(0);

  const prioritize = usePrioritizeTasks();
  const updateWorkItem = useUpdateWorkItem(projectId ?? "");
  const { data: workItemsPage } = useWorkItems({ projectId: projectId ?? "", page: 1, pageSize: 500 });

  const byId = useMemo(
    () => new Map((workItemsPage?.items ?? []).map((w) => [w.id, w])),
    [workItemsPage]
  );

  const handleRun = () => {
    if (!projectId) return;
    setAppliedCount(0);
    prioritize.mutate(
      { projectId },
      {
        onSuccess: (data) => setSelected(new Set(data.items.map((i) => i.workItemId))),
      }
    );
  };

  const toggle = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleAccept = async () => {
    if (!prioritize.data) return;
    const toApply = prioritize.data.items.filter((i) => selected.has(i.workItemId));
    for (const item of toApply) {
      await updateWorkItem.mutateAsync({
        id: item.workItemId,
        payload: { priority: item.suggestedPriority },
      });
    }
    setAppliedCount(toApply.length);
    setSelected(new Set());
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Task Prioritizer
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Ask the assistant to re-rank the backlog, then selectively accept its suggested
          priorities.
        </Typography>
      </Box>

      <Box>
        <Button
          variant="contained"
          startIcon={
            prioritize.isPending ? <CircularProgress size={16} color="inherit" /> : <SsidChartOutlinedIcon />
          }
          disabled={prioritize.isPending}
          onClick={handleRun}
        >
          {prioritize.isPending ? "Analyzing backlog..." : "Run AI prioritization"}
        </Button>
      </Box>

      {prioritize.isError && <AiUnavailableState error={prioritize.error} />}

      {appliedCount > 0 && (
        <Alert severity="success" onClose={() => setAppliedCount(0)}>
          Updated priority on {appliedCount} item{appliedCount === 1 ? "" : "s"}.
        </Alert>
      )}

      {prioritize.data && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle1" fontWeight={700}>
                  Suggested re-ranking ({prioritize.data.items.length})
                </Typography>
                <Button
                  size="small"
                  variant="contained"
                  disabled={selected.size === 0 || updateWorkItem.isPending}
                  startIcon={updateWorkItem.isPending ? <CircularProgress size={14} color="inherit" /> : undefined}
                  onClick={handleAccept}
                >
                  Accept selected ({selected.size})
                </Button>
              </Stack>

              {prioritize.data.items.length === 0 ? (
                <EmptyState title="Nothing to re-rank" description="The backlog may be empty." />
              ) : (
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell padding="checkbox" />
                      <TableCell>Task</TableCell>
                      <TableCell>Current</TableCell>
                      <TableCell>Suggested</TableCell>
                      <TableCell>Reasoning</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {prioritize.data.items.map((item) => {
                      const workItem = byId.get(item.workItemId);
                      return (
                        <TableRow key={item.workItemId} hover>
                          <TableCell padding="checkbox">
                            <Checkbox
                              checked={selected.has(item.workItemId)}
                              onChange={() => toggle(item.workItemId)}
                            />
                          </TableCell>
                          <TableCell>{workItem?.title ?? item.workItemId}</TableCell>
                          <TableCell>
                            {workItem && <Chip size="small" label={workItem.priority} variant="outlined" />}
                          </TableCell>
                          <TableCell>
                            <Chip size="small" color="primary" label={item.suggestedPriority} />
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" color="text.secondary">
                              {item.reasoning}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
