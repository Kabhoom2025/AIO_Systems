import { useQuery } from "@tanstack/react-query";
import { automationApi } from "../api/automation";

export function useWorkflowRuns(workflowId: string | undefined, page: number, pageSize: number) {
  return useQuery({
    queryKey: ["workflows", "runs", workflowId, page, pageSize],
    queryFn: () => automationApi.getRuns(workflowId as string, page, pageSize),
    enabled: !!workflowId,
    placeholderData: (prev) => prev,
    staleTime: 5 * 1000,
  });
}
