import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { AnalyzeBugRequest } from "../types";

export function useAnalyzeBug() {
  return useMutation({
    mutationFn: (payload: AnalyzeBugRequest) => aiApi.analyzeBug(payload),
  });
}
