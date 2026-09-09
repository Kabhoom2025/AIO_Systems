import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { EstimateStoryPointsRequest } from "../types";

export function useEstimateStoryPoints() {
  return useMutation({
    mutationFn: (payload: EstimateStoryPointsRequest) => aiApi.estimateStoryPoints(payload),
  });
}
