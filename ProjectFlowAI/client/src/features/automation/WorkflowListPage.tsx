import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import HistoryOutlinedIcon from "@mui/icons-material/HistoryOutlined";
import { Box, Button, Chip, IconButton, Stack, Switch, Tooltip, Typography } from "@mui/material";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import {
  useDeleteWorkflow,
  useDisableWorkflow,
  useEnableWorkflow,
  useWorkflows,
} from "../../hooks/useWorkflows";
import { WORKFLOW_TRIGGER_LABELS, type WorkflowSummary } from "../../types";

export function WorkflowListPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const { data, isLoading } = useWorkflows(projectId);
  const deleteWorkflow = useDeleteWorkflow(projectId ?? "");
  const enableWorkflow = useEnableWorkflow(projectId ?? "");
  const disableWorkflow = useDisableWorkflow(projectId ?? "");
  const [toDelete, setToDelete] = useState<WorkflowSummary | null>(null);

  const columns: DataTableColumn<WorkflowSummary>[] = [
    { key: "name", label: "Name", render: (w) => <Typography fontWeight={600}>{w.name}</Typography> },
    {
      key: "trigger",
      label: "Trigger",
      render: (w) => <Chip size="small" variant="outlined" label={WORKFLOW_TRIGGER_LABELS[w.triggerType]} />,
    },
    { key: "conditions", label: "Conditions", render: (w) => w.conditionCount },
    { key: "actions", label: "Actions", render: (w) => w.actionCount },
    {
      key: "enabled",
      label: "Enabled",
      render: (w) => (
        <Switch
          checked={w.isEnabled}
          onChange={() => (w.isEnabled ? disableWorkflow : enableWorkflow).mutate(w.id)}
        />
      ),
    },
    {
      key: "actionsCol",
      label: "",
      align: "right",
      render: (w) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <Tooltip title="Run history">
            <IconButton size="small" onClick={() => navigate(`/projects/${projectId}/automation/${w.id}/runs`)}>
              <HistoryOutlinedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => navigate(`/projects/${projectId}/automation/${w.id}/edit`)}>
              <EditOutlinedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" onClick={() => setToDelete(w)}>
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Box>
          <Typography variant="h5" fontWeight={700}>
            Automation
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Workflows that automatically react to project events.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate(`/projects/${projectId}/automation/new`)}
        >
          New workflow
        </Button>
      </Stack>

      <DataTable
        columns={columns}
        rows={data ?? []}
        getRowId={(w) => w.id}
        isLoading={isLoading}
        totalCount={data?.length ?? 0}
        page={0}
        pageSize={data?.length && data.length > 10 ? data.length : 10}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        emptyTitle="No workflows yet"
        emptyDescription="Create a workflow to automate status changes, assignments, notifications, and more."
      />

      <ConfirmDialog
        open={!!toDelete}
        title="Delete workflow"
        message={`Delete "${toDelete?.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        destructive
        loading={deleteWorkflow.isPending}
        onCancel={() => setToDelete(null)}
        onConfirm={() => {
          if (toDelete) deleteWorkflow.mutate(toDelete.id, { onSuccess: () => setToDelete(null) });
        }}
      />
    </Stack>
  );
}
