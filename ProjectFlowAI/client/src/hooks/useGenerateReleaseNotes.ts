import { useMutation } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import type { GenerateReleaseNotesRequest } from "../types";

export function useGenerateReleaseNotes() {
  return useMutation({
    mutationFn: (payload: GenerateReleaseNotesRequest) => aiApi.generateReleaseNotes(payload),
  });
}
