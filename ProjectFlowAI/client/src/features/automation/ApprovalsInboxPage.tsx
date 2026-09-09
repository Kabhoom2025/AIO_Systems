import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import HighlightOffIcon from "@mui/icons-material/HighlightOff";
import InboxOutlinedIcon from "@mui/icons-material/InboxOutlined";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { useState } from "react";
import { AppShell } from "../../components/AppShell";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { useApprovals, useDecideApproval } from "../../hooks/useApprovals";
import type { Approval } from "../../types";

export function ApprovalsInboxPage() {
  const { data, isLoading } = useApprovals(true);
  const decide = useDecideApproval();
  const [comments, setComments] = useState<Record<string, string>>({});
  const [decidingId, setDecidingId] = useState<string | null>(null);

  const pending = (data ?? []).filter((a) => a.status === "AwaitingApproval");

  const handleDecide = (approval: Approval, approve: boolean) => {
    setDecidingId(approval.id);
    decide.mutate(
      { id: approval.id, payload: { approve, comment: comments[approval.id] ?? "" } },
      { onSettled: () => setDecidingId(null) }
    );
  };

  return (
    <AppShell>
      <Stack spacing={2}>
        <Box>
          <Typography variant="h5" fontWeight={700}>
            Approvals
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Workflow steps waiting on your decision.
          </Typography>
        </Box>

        {isLoading ? (
          <SkeletonCard count={3} />
        ) : pending.length === 0 ? (
          <EmptyState
            icon={<InboxOutlinedIcon sx={{ fontSize: 48 }} />}
            title="Nothing pending"
            description="You have no approvals waiting for a decision."
          />
        ) : (
          <Stack spacing={1.5}>
            {pending.map((a) => (
              <Card key={a.id} variant="outlined">
                <CardContent>
                  <Stack spacing={1.5}>
                    <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                      <Box>
                        <Typography variant="subtitle1" fontWeight={700}>
                          {a.workflowName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Requested {formatDistanceToNow(new Date(a.requestedAt), { addSuffix: true })}
                        </Typography>
                      </Box>
                      <Chip size="small" label={a.status} color="warning" />
                    </Stack>
                    <TextField
                      size="small"
                      label="Comment (optional)"
                      fullWidth
                      value={comments[a.id] ?? ""}
                      onChange={(e) => setComments((prev) => ({ ...prev, [a.id]: e.target.value }))}
                    />
                    <Stack direction="row" spacing={1.5}>
                      <Button
                        variant="contained"
                        color="success"
                        startIcon={
                          decidingId === a.id && decide.isPending ? (
                            <CircularProgress size={14} color="inherit" />
                          ) : (
                            <CheckCircleOutlineIcon />
                          )
                        }
                        disabled={decide.isPending}
                        onClick={() => handleDecide(a, true)}
                      >
                        Approve
                      </Button>
                      <Button
                        variant="outlined"
                        color="error"
                        startIcon={<HighlightOffIcon />}
                        disabled={decide.isPending}
                        onClick={() => handleDecide(a, false)}
                      >
                        Reject
                      </Button>
                    </Stack>
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
        )}
      </Stack>
    </AppShell>
  );
}
