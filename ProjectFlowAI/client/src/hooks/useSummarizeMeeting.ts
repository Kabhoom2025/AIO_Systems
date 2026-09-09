import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { SummarizeMeetingRequest } from "../types";

export function useSummarizeMeeting() {
  return useMutation({
    mutationFn: (payload: SummarizeMeetingRequest) => aiApi.summarizeMeeting(payload),
  });
}
