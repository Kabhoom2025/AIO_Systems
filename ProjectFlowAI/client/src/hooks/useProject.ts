import { useQuery } from "@tanstack/react-query";
import { projectsApi } from "../api/projects";

export function useProject(id: string | undefined) {
  return useQuery({
    queryKey: ["projects", "detail", id],
    queryFn: () => projectsApi.get(id as string),
    enabled: !!id,
    staleTime: 30 * 1000,
  });
}
