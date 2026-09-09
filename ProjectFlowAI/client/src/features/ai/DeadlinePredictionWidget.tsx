import EventAvailableOutlinedIcon from "@mui/icons-material/EventAvailableOutlined";
import { Card, CardContent, Chip, Skeleton, Stack, Typography } from "@mui/material";
import { format } from "date-fns";
import { isAiUnconfiguredError } from "../../utils/apiErrors";
import type { DeadlinePredictionResponse, PredictionConfidence } from "../../types";

const CONFIDENCE_COLOR: Record<PredictionConfidence, "error" | "warning" | "success"> = {
  Low: "error",
  Medium: "warning",
  High: "success",
};

interface DeadlinePredictionWidgetProps {
  data: DeadlinePredictionResponse | undefined;
  isLoading: boolean;
  error: unknown;
}

export function DeadlinePredictionWidget({ data, isLoading, error }: DeadlinePredictionWidgetProps) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Typography variant="body2" color="text.secondary">
            AI Predicted Deadline
          </Typography>
          <EventAvailableOutlinedIcon fontSize="small" color="action" />
        </Stack>
        {isLoading ? (
          <Skeleton variant="text" width="60%" height={40} />
        ) : error ? (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {isAiUnconfiguredError(error)
              ? "AI features aren't configured yet."
              : "Deadline prediction unavailable right now."}
          </Typography>
        ) : data ? (
          <Stack spacing={0.75} sx={{ mt: 0.5 }}>
            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="h6" fontWeight={700}>
                {(() => {
                  try {
                    return format(new Date(data.predictedDate), "MMM d, yyyy");
                  } catch {
                    return data.predictedDate;
                  }
                })()}
              </Typography>
              <Chip
                label={`${data.confidence} confidence`}
                color={CONFIDENCE_COLOR[data.confidence]}
                size="small"
              />
            </Stack>
            <Typography variant="body2" color="text.secondary">
              {data.reasoning}
            </Typography>
          </Stack>
        ) : null}
      </CardContent>
    </Card>
  );
}
