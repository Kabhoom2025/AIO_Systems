import { useMutation, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";

export function useChecklist(workItemId: string) {
  const queryClient = useQueryClient();
  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["workItems", "detail", workItemId] });

  const addItem = useMutation({
    mutationFn: (text: string) => workItemsApi.addChecklistItem(workItemId, text),
    onSuccess: invalidate,
  });

  const toggleItem = useMutation({
    mutationFn: ({ itemId, isDone }: { itemId: string; isDone: boolean }) =>
      workItemsApi.toggleChecklistItem(workItemId, itemId, isDone),
    onSuccess: invalidate,
  });

  const removeItem = useMutation({
    mutationFn: (itemId: string) => workItemsApi.removeChecklistItem(workItemId, itemId),
    onSuccess: invalidate,
  });

  return { addItem, toggleItem, removeItem };
}
