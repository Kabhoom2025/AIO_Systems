import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Card, CardContent, Chip, Skeleton, Stack, Typography } from "@mui/material";
import { isAiUnconfiguredError } from "../../utils/apiErrors";
import type { RiskLevel, RiskPredictionResponse } from "../../types";

const RISK_COLOR: Record<RiskLevel, "success" | "warning" | "error"> = {
  Low: "success",
  Medium: "warning",
  High: "error",
};

interface RiskPredictionWidgetProps {
  data: RiskPredictionResponse | undefined;
  isLoading: boolean;
  error: unknown;
}

/** A small stat card for the project-risk read the AI assistant gives — designed to sit
 * alongside DeadlinePredictionWidget on the AI Tools hub (or a reports page). Silently
 * collapses to a muted "not available" line on 503 rather than an alarming error card,
 * since an unconfigured AI provider is an expected setup state, not a fault. */
export function RiskPredictionWidget({ data, isLoading, error }: RiskPredictionWidgetProps) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Typography variant="body2" color="text.secondary">
            AI Risk Prediction
          </Typography>
          <WarningAmberOutlinedIcon fontSize="small" color="action" />
        </Stack>
        {isLoading ? (
          <Skeleton variant="text" width="60%" height={40} />
        ) : error ? (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {isAiUnconfiguredError(error)
              ? "AI features aren't configured yet."
              : "Risk prediction unavailable right now."}
          </Typography>
        ) : data ? (
          <Stack spacing={0.75} sx={{ mt: 0.5 }}>
            <Chip
              label={data.riskLevel}
              color={RISK_COLOR[data.riskLevel]}
              size="small"
              sx={{ alignSelf: "flex-start", fontWeight: 700 }}
            />
            <Typography variant="body2" color="text.secondary">
              {data.explanation}
            </Typography>
          </Stack>
        ) : null}
      </CardContent>
    </Card>
  );
}
