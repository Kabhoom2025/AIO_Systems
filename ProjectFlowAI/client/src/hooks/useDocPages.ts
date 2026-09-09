import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { docPagesApi } from "../api/docPages";
import type { CreateDocPageRequest, DocPageListParams, UpdateDocPageRequest } from "../types";

export const docPageKeys = {
  list: (params: DocPageListParams) => ["docPages", "list", params] as const,
  detail: (id: string) => ["docPages", "detail", id] as const,
  versions: (id: string) => ["docPages", "versions", id] as const,
  comments: (id: string) => ["docPages", "comments", id] as const,
};

export function useDocPages(params: DocPageListParams | undefined) {
  return useQuery({
    queryKey: docPageKeys.list(params ?? { organizationId: "", scope: "OrgWiki" }),
    queryFn: () => docPagesApi.list(params as DocPageListParams),
    enabled: !!params?.organizationId,
    staleTime: 15 * 1000,
  });
}

export function useDocPage(id: string | undefined) {
  return useQuery({
    queryKey: docPageKeys.detail(id ?? ""),
    queryFn: () => docPagesApi.get(id as string),
    enabled: !!id,
  });
}

export function useCreateDocPage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDocPageRequest) => docPagesApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["docPages", "list"] });
    },
  });
}

export function useUpdateDocPage(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateDocPageRequest) => docPagesApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["docPages", "list"] });
      queryClient.invalidateQueries({ queryKey: docPageKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: docPageKeys.versions(id) });
    },
  });
}

export function useDeleteDocPage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => docPagesApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["docPages", "list"] });
    },
  });
}

export function useUploadDocImage(id: string) {
  return useMutation({
    mutationFn: (file: File) => docPagesApi.uploadImage(id, file),
  });
}
