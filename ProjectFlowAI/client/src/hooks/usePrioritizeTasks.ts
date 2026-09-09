import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { PrioritizeTasksRequest } from "../types";

export function usePrioritizeTasks() {
  return useMutation({
    mutationFn: (payload: PrioritizeTasksRequest) => aiApi.prioritizeTasks(payload),
  });
}
