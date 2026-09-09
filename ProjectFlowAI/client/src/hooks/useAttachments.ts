import { useMutation, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";

export function useAttachments(workItemId: string) {
  const queryClient = useQueryClient();
  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["workItems", "detail", workItemId] });

  const upload = useMutation({
    mutationFn: (file: File) => workItemsApi.uploadAttachment(workItemId, file),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (attachmentId: string) => workItemsApi.removeAttachment(attachmentId),
    onSuccess: invalidate,
  });

  return { upload, remove };
}
