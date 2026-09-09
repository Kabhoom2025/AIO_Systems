import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import { retrySkipping503 } from "../utils/apiErrors";
import type { AiChatRequest } from "../types";

export const aiChatKeys = {
  conversations: (projectId: string) => ["aiChat", "conversations", projectId] as const,
  messages: (conversationId: string) => ["aiChat", "messages", conversationId] as const,
};

export function useAiConversations(projectId: string | undefined) {
  return useQuery({
    queryKey: aiChatKeys.conversations(projectId ?? ""),
    queryFn: () => aiApi.listConversations(projectId as string),
    enabled: !!projectId,
    retry: retrySkipping503,
    staleTime: 10 * 1000,
  });
}

export function useAiChatMessages(conversationId: string | undefined) {
  return useQuery({
    queryKey: aiChatKeys.messages(conversationId ?? ""),
    queryFn: () => aiApi.getConversationMessages(conversationId as string),
    enabled: !!conversationId,
    retry: retrySkipping503,
  });
}

/** Sends a chat message and keeps both the conversation list and the active thread's
 * message cache in sync — the response only returns the assistant reply, so we
 * optimistically append the user's own message locally as well. */
export function useSendAiChatMessage(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (args: { message: string; conversationId: string | null }) =>
      aiApi.chat({ projectId, message: args.message, conversationId: args.conversationId }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: aiChatKeys.conversations(projectId) });
      queryClient.invalidateQueries({ queryKey: aiChatKeys.messages(data.conversationId) });
    },
  });
}

export type { AiChatRequest };
