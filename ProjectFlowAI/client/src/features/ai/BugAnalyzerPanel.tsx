import BugReportOutlinedIcon from "@mui/icons-material/BugReportOutlined";
import { Box, Button, Chip, CircularProgress, Stack, Typography } from "@mui/material";
import { useAnalyzeBug } from "../../hooks/useAnalyzeBug";
import { AiUnavailableState } from "./AiUnavailableState";

interface BugAnalyzerPanelProps {
  workItemId: string;
}

/** Embedded in WorkItemDetailDrawer for Bug-type work items — "AI: Analyze Bug" runs
 * POST /ai/analyze-bug and shows the probable root cause, repro steps, suggested
 * severity, and reasoning inline, right where the rest of the task detail lives. */
export function BugAnalyzerPanel({ workItemId }: BugAnalyzerPanelProps) {
  const analyzeBug = useAnalyzeBug();

  return (
    <Stack spacing={1.5}>
      <Button
        size="small"
        variant="outlined"
        startIcon={
          analyzeBug.isPending ? <CircularProgress size={14} /> : <BugReportOutlinedIcon fontSize="small" />
        }
        disabled={analyzeBug.isPending}
        onClick={() => analyzeBug.mutate({ workItemId })}
      >
        {analyzeBug.isPending ? "Analyzing..." : "AI: Analyze Bug"}
      </Button>

      {analyzeBug.isError && <AiUnavailableState error={analyzeBug.error} />}

      {analyzeBug.data && (
        <Stack spacing={1.25} sx={{ p: 1.5, border: 1, borderColor: "divider", borderRadius: 1 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="subtitle2" fontWeight={700}>
              Suggested severity
            </Typography>
            <Chip size="small" color="warning" label={analyzeBug.data.suggestedSeverity} />
          </Stack>
          <Box>
            <Typography variant="caption" color="text.secondary" fontWeight={700}>
              PROBABLE ROOT CAUSE
            </Typography>
            <Typography variant="body2">{analyzeBug.data.probableRootCause}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary" fontWeight={700}>
              REPRO STEPS
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
              {analyzeBug.data.reproSteps}
            </Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary" fontWeight={700}>
              REASONING
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {analyzeBug.data.reasoning}
            </Typography>
          </Box>
        </Stack>
      )}
    </Stack>
  );
}
