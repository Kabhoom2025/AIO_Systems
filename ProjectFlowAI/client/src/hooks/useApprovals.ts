import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { automationApi } from "../api/automation";
import type { DecideApprovalRequest } from "../types";

export const approvalsKey = (assignedToMe: boolean) => ["approvals", assignedToMe] as const;

export function useApprovals(assignedToMe: boolean) {
  return useQuery({
    queryKey: approvalsKey(assignedToMe),
    queryFn: () => automationApi.listApprovals(assignedToMe),
    staleTime: 10 * 1000,
  });
}

export function useDecideApproval() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: DecideApprovalRequest }) =>
      automationApi.decideApproval(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals"] });
    },
  });
}
