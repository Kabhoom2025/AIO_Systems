import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { PlanSprintRequest } from "../types";

export function usePlanSprint() {
  return useMutation({
    mutationFn: (payload: PlanSprintRequest) => aiApi.planSprint(payload),
  });
}
