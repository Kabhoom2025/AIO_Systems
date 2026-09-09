import SmartToyOutlinedIcon from "@mui/icons-material/SmartToyOutlined";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import { EmptyState } from "../../components/EmptyState";
import { getErrorMessage, isAiUnconfiguredError } from "../../utils/apiErrors";

interface AiUnavailableStateProps {
  error?: unknown;
  title?: string;
}

/** Shared empty/error state for every /ai/* screen. A 503 means the backend has no AI
 * provider configured — that's an expected, non-broken state, so it gets a calm,
 * specific message rather than a raw error dump. Any other error still gets a readable
 * message instead of a stack trace. */
export function AiUnavailableState({ error, title }: AiUnavailableStateProps) {
  const unconfigured = isAiUnconfiguredError(error);
  return (
    <EmptyState
      icon={unconfigured ? <SmartToyOutlinedIcon sx={{ fontSize: 48 }} /> : <ErrorOutlineIcon sx={{ fontSize: 48 }} />}
      title={title ?? (unconfigured ? "AI features aren't configured yet" : "Something went wrong")}
      description={
        unconfigured
          ? "An administrator needs to configure an AI provider for this organization before this feature can be used."
          : getErrorMessage(error)
      }
    />
  );
}
