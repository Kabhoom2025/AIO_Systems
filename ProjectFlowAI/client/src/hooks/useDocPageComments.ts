import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { docPagesApi } from "../api/docPages";
import { docPageKeys } from "./useDocPages";
import type { CreateDocPageCommentRequest } from "../types";

export function useDocPageComments(id: string | undefined) {
  return useQuery({
    queryKey: docPageKeys.comments(id ?? ""),
    queryFn: () => docPagesApi.listComments(id as string),
    enabled: !!id,
  });
}

export function useAddDocPageComment(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDocPageCommentRequest) => docPagesApi.addComment(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: docPageKeys.comments(id) });
    },
  });
}

export function useRemoveDocPageComment(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (commentId: string) => docPagesApi.removeComment(commentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: docPageKeys.comments(id) });
    },
  });
}
