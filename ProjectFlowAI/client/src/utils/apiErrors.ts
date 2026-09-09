import axios from "axios";

/** True when the error is an HTTP 403 — used to show a clear "no access" state
 * instead of a raw error dump for permission-gated screens (e.g. reports.view). */
export function isForbiddenError(error: unknown): boolean {
  return axios.isAxiosError(error) && error.response?.status === 403;
}

/** TanStack Query `retry` option that never retries a 403 (permission denied won't
 * change on retry) but retries other transient failures once. */
export function retrySkipping403(failureCount: number, error: unknown): boolean {
  if (isForbiddenError(error)) return false;
  return failureCount < 1;
}

/** True when the error is an HTTP 503 — the AI backend returns this ProblemDetails
 * status when no AI provider (e.g. Anthropic API key) is configured. Every /ai/* screen
 * uses this to show a clear "AI features aren't configured yet" state instead of a raw
 * error dump or an infinite spinner. */
export function isAiUnconfiguredError(error: unknown): boolean {
  return axios.isAxiosError(error) && error.response?.status === 503;
}

/** TanStack Query `retry` option for /ai/* queries — a 503 (not configured) won't
 * resolve on retry, so fail fast; otherwise allow one retry for transient issues. */
export function retrySkipping503(failureCount: number, error: unknown): boolean {
  if (isAiUnconfiguredError(error)) return false;
  return failureCount < 1;
}

/** Best-effort human-readable message extracted from an Axios/ProblemDetails error,
 * for surfacing non-503 AI failures without a raw stack dump. */
export function getErrorMessage(error: unknown, fallback = "Something went wrong."): string {
  if (axios.isAxiosError(error)) {
    const detail = (error.response?.data as { detail?: string; title?: string } | undefined);
    return detail?.detail || detail?.title || error.message || fallback;
  }
  if (error instanceof Error) return error.message;
  return fallback;
}
