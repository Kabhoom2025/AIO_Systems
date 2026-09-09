import AutoAwesomeOutlinedIcon from "@mui/icons-material/AutoAwesomeOutlined";
import { CircularProgress, IconButton, Popover, Stack, Tooltip, Typography } from "@mui/material";
import { useState, type MouseEvent } from "react";
import { useEstimateStoryPoints } from "../../hooks/useEstimateStoryPoints";
import { isAiUnconfiguredError } from "../../utils/apiErrors";

interface StoryPointEstimatorButtonProps {
  title: string;
  description?: string | null;
  onEstimate: (points: number) => void;
}

/** A small inline "Suggest with AI" action for the story points field — used both in the
 * work item creation form and WorkItemDetailDrawer. Shows the AI's reasoning in a popover
 * before the caller decides whether to accept it into the field. */
export function StoryPointEstimatorButton({ title, description, onEstimate }: StoryPointEstimatorButtonProps) {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const estimate = useEstimateStoryPoints();

  const handleClick = (e: MouseEvent<HTMLElement>) => {
    setAnchorEl(e.currentTarget);
    estimate.mutate(
      { title, description: description ?? "" },
      {
        onSuccess: (data) => onEstimate(data.suggestedPoints),
      }
    );
  };

  return (
    <>
      <Tooltip title="Suggest story points with AI">
        <span>
          <IconButton size="small" disabled={!title.trim() || estimate.isPending} onClick={handleClick}>
            {estimate.isPending ? <CircularProgress size={16} /> : <AutoAwesomeOutlinedIcon fontSize="small" />}
          </IconButton>
        </span>
      </Tooltip>
      <Popover
        open={!!anchorEl && (estimate.isSuccess || estimate.isError)}
        anchorEl={anchorEl}
        onClose={() => setAnchorEl(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
      >
        <Stack spacing={0.5} sx={{ p: 1.5, maxWidth: 280 }}>
          {estimate.isError ? (
            <Typography variant="body2" color="text.secondary">
              {isAiUnconfiguredError(estimate.error)
                ? "AI features aren't configured yet."
                : "Couldn't get a suggestion right now."}
            </Typography>
          ) : estimate.data ? (
            <>
              <Typography variant="subtitle2" fontWeight={700}>
                Suggested: {estimate.data.suggestedPoints} points
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {estimate.data.reasoning}
              </Typography>
            </>
          ) : null}
        </Stack>
      </Popover>
    </>
  );
}
