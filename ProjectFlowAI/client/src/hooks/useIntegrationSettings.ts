import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationsApi } from "../api/notifications";
import type { UpdateIntegrationSettingsRequest } from "../types";

export function useIntegrationSettings(organizationId: string | undefined) {
  return useQuery({
    queryKey: ["integration-settings", organizationId],
    queryFn: () => notificationsApi.getIntegrationSettings(organizationId as string),
    enabled: !!organizationId,
  });
}

export function useUpdateIntegrationSettings(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateIntegrationSettingsRequest) =>
      notificationsApi.updateIntegrationSettings(organizationId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["integration-settings", organizationId] });
    },
  });
}
