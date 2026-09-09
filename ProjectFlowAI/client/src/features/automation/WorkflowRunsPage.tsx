import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import {
  Box,
  Chip,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { useWorkflow } from "../../hooks/useWorkflow";
import { useWorkflowRuns } from "../../hooks/useWorkflowRuns";
import type { WorkflowRun, WorkflowRunStatus } from "../../types";

const STATUS_COLOR: Record<WorkflowRunStatus, "info" | "success" | "error" | "warning"> = {
  Running: "info",
  Succeeded: "success",
  Failed: "error",
  AwaitingApproval: "warning",
};

function prettyLog(logJson: string): string {
  try {
    return JSON.stringify(JSON.parse(logJson), null, 2);
  } catch {
    return logJson;
  }
}

export function WorkflowRunsPage() {
  const { projectId, workflowId } = useParams<{ projectId: string; workflowId: string }>();
  const navigate = useNavigate();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [logRun, setLogRun] = useState<WorkflowRun | null>(null);

  const { data: workflow } = useWorkflow(workflowId);
  const { data, isLoading } = useWorkflowRuns(workflowId, page + 1, pageSize);

  const columns: DataTableColumn<WorkflowRun>[] = [
    {
      key: "status",
      label: "Status",
      render: (r) => <Chip size="small" label={r.status} color={STATUS_COLOR[r.status]} />,
    },
    { key: "triggerEntityId", label: "Trigger entity", render: (r) => r.triggerEntityId },
    {
      key: "startedAt",
      label: "Started",
      render: (r) => formatDistanceToNow(new Date(r.startedAt), { addSuffix: true }),
    },
    {
      key: "completedAt",
      label: "Completed",
      render: (r) =>
        r.completedAt ? formatDistanceToNow(new Date(r.completedAt), { addSuffix: true }) : "—",
    },
    {
      key: "log",
      label: "",
      align: "right",
      render: (r) => (
        <Tooltip title="View log">
          <IconButton size="small" onClick={() => setLogRun(r)}>
            <VisibilityOutlinedIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      ),
    },
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" spacing={1.5}>
        <IconButton onClick={() => navigate(`/projects/${projectId}/automation`)}>
          <ArrowBackIcon />
        </IconButton>
        <Box>
          <Typography variant="h5" fontWeight={700}>
            {workflow ? `Run history — ${workflow.name}` : "Run history"}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Every time this workflow fired, with its resulting status.
          </Typography>
        </Box>
      </Stack>

      <DataTable
        columns={columns}
        rows={data?.items ?? []}
        getRowId={(r) => r.id}
        isLoading={isLoading}
        totalCount={data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        emptyTitle="No runs yet"
        emptyDescription="This workflow hasn't been triggered yet."
      />

      <Dialog open={!!logRun} onClose={() => setLogRun(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Run log</DialogTitle>
        <DialogContent>
          <Box
            component="pre"
            sx={{
              whiteSpace: "pre-wrap",
              wordBreak: "break-word",
              fontFamily: "monospace",
              fontSize: 12,
              bgcolor: "action.hover",
              p: 1.5,
              borderRadius: 1,
              maxHeight: 400,
              overflowY: "auto",
            }}
          >
            {logRun ? prettyLog(logRun.logJson) : ""}
          </Box>
        </DialogContent>
      </Dialog>
    </Stack>
  );
}
