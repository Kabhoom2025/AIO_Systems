import { useMutation, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";
import type { CreateCommentRequest, UpdateCommentRequest } from "../types";

export function useComments(workItemId: string) {
  const queryClient = useQueryClient();
  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["workItems", "detail", workItemId] });

  const addComment = useMutation({
    mutationFn: (payload: CreateCommentRequest) => workItemsApi.addComment(workItemId, payload),
    onSuccess: invalidate,
  });

  const updateComment = useMutation({
    mutationFn: ({ commentId, payload }: { commentId: string; payload: UpdateCommentRequest }) =>
      workItemsApi.updateComment(commentId, payload),
    onSuccess: invalidate,
  });

  const removeComment = useMutation({
    mutationFn: (commentId: string) => workItemsApi.removeComment(commentId),
    onSuccess: invalidate,
  });

  return { addComment, updateComment, removeComment };
}
