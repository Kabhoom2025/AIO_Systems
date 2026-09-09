import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { ReviewCodeRequest } from "../types";

export function useReviewCode() {
  return useMutation({
    mutationFn: (payload: ReviewCodeRequest) => aiApi.reviewCode(payload),
  });
}
