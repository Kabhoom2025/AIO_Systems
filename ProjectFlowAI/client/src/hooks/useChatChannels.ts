import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { chatApi } from "../api/chat";
import { chatKeys } from "./chatKeys";
import type { CreateChatChannelRequest, CreateConversationRequest } from "../types";

export function useChatChannels(organizationId: string | undefined, projectId?: string) {
  return useQuery({
    queryKey: chatKeys.channels(organizationId ?? "", projectId),
    queryFn: () => chatApi.listChannels(organizationId as string, projectId),
    enabled: !!organizationId,
    staleTime: 15 * 1000,
  });
}

export function useChatConversations(organizationId: string | undefined) {
  return useQuery({
    queryKey: chatKeys.conversations(organizationId ?? ""),
    queryFn: () => chatApi.listConversations(organizationId as string),
    enabled: !!organizationId,
    staleTime: 15 * 1000,
  });
}

export function useCreateChatChannel(organizationId: string, projectId?: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateChatChannelRequest) => chatApi.createChannel(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: chatKeys.channels(organizationId, projectId) });
    },
  });
}

export function useCreateConversation(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateConversationRequest) => chatApi.createConversation(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: chatKeys.conversations(organizationId) });
    },
  });
}

export function useMarkChannelRead(organizationId: string, projectId?: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (channelId: string) => chatApi.markChannelRead(channelId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: chatKeys.channels(organizationId, projectId) });
    },
  });
}
