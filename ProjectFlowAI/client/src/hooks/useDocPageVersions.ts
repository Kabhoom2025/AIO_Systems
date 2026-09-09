import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { docPagesApi } from "../api/docPages";
import { docPageKeys } from "./useDocPages";

export function useDocPageVersions(id: string | undefined) {
  return useQuery({
    queryKey: docPageKeys.versions(id ?? ""),
    queryFn: () => docPagesApi.listVersions(id as string),
    enabled: !!id,
  });
}

export function useDocPageVersion(id: string | undefined, versionId: string | undefined) {
  return useQuery({
    queryKey: [...docPageKeys.versions(id ?? ""), versionId],
    queryFn: () => docPagesApi.getVersion(id as string, versionId as string),
    enabled: !!id && !!versionId,
  });
}

export function useRestoreDocPageVersion(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (versionId: string) => docPagesApi.restoreVersion(id, versionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: docPageKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: docPageKeys.versions(id) });
      queryClient.invalidateQueries({ queryKey: ["docPages", "list"] });
    },
  });
}
