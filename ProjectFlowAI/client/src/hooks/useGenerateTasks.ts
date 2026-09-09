import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { GenerateTasksRequest } from "../types";

export function useGenerateTasks() {
  return useMutation({
    mutationFn: (payload: GenerateTasksRequest) => aiApi.generateTasks(payload),
  });
}
