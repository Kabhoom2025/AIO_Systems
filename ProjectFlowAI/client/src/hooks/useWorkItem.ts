import { useQuery } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";

export function useWorkItem(id: string | undefined) {
  return useQuery({
    queryKey: ["workItems", "detail", id],
    queryFn: () => workItemsApi.get(id as string),
    enabled: !!id,
    staleTime: 15 * 1000,
  });
}
