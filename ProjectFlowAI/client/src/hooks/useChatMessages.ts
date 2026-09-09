import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { chatApi } from "../api/chat";
import { useAuthStore } from "../store/authStore";
import { chatKeys } from "./chatKeys";
import { prependMessageToThread, updateMessageInAllThreads } from "./chatCache";
import type { ChatMessagesResponse, ChatThreadRef, SendChatMessageRequest } from "../types";

const PAGE_SIZE = 50;

function fetchPage(thread: ChatThreadRef, before?: string): Promise<ChatMessagesResponse> {
  return thread.kind === "channel"
    ? chatApi.getChannelMessages(thread.id, before, PAGE_SIZE)
    : chatApi.getConversationMessages(thread.id, before, PAGE_SIZE);
}

/**
 * Loads messages for a channel or DM thread, newest-first per page (matching the API), as an
 * infinite query. `messages` is exposed already flattened and reversed to oldest-first so the
 * message list can render top-to-bottom like a normal chat feed, with `fetchOlder` wired to a
 * "load older" affordance at the top.
 */
export function useChatMessages(thread: ChatThreadRef | null) {
  const query = useInfiniteQuery({
    queryKey: thread ? chatKeys.messages(thread) : ["chat", "messages", "none"],
    queryFn: ({ pageParam }) => fetchPage(thread as ChatThreadRef, pageParam as string | undefined),
    enabled: !!thread,
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.messages.length > 0
        ? lastPage.messages[lastPage.messages.length - 1].id
        : undefined,
    staleTime: 10 * 1000,
  });

  const messages = (query.data?.pages ?? [])
    .flatMap((p) => p.messages)
    .slice()
    .reverse();

  return {
    ...query,
    messages,
    fetchOlder: query.fetchNextPage,
    hasOlder: query.hasNextPage,
    loadingOlder: query.isFetchingNextPage,
  };
}

export function useSendMessage(thread: ChatThreadRef | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: SendChatMessageRequest) => {
      if (!thread) throw new Error("No active thread");
      return thread.kind === "channel"
        ? chatApi.sendChannelMessage(thread.id, payload)
        : chatApi.sendConversationMessage(thread.id, payload);
    },
    onSuccess: (message) => {
      if (!thread) return;
      prependMessageToThread(queryClient, chatKeys.messages(thread), message);
    },
  });
}

/** Toggles a reaction on a message with an optimistic cache update; the server confirms (or
 * corrects) via the ReactionChanged hub event, which uses the same updateMessageInAllThreads
 * helper so both paths converge on the same cache shape. */
export function useReactToMessage() {
  const queryClient = useQueryClient();
  const currentUserId = useAuthStore((s) => s.user?.id);

  return useMutation({
    mutationFn: ({ messageId, emoji }: { messageId: string; emoji: string }) =>
      chatApi.react(messageId, emoji),
    onMutate: ({ messageId, emoji }) => {
      if (!currentUserId) return;
      updateMessageInAllThreads(queryClient, messageId, (message) => {
        const reactions = message.reactions.map((r) => ({ ...r, userIds: [...r.userIds] }));
        const existing = reactions.find((r) => r.emoji === emoji);
        if (existing) {
          const hasReacted = existing.userIds.includes(currentUserId);
          existing.userIds = hasReacted
            ? existing.userIds.filter((id) => id !== currentUserId)
            : [...existing.userIds, currentUserId];
          return {
            ...message,
            reactions: reactions.filter((r) => r.userIds.length > 0),
          };
        }
        return { ...message, reactions: [...reactions, { emoji, userIds: [currentUserId] }] };
      });
    },
  });
}

export function useDeleteMessage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (messageId: string) => chatApi.removeMessage(messageId),
    onSuccess: (_, messageId) => {
      updateMessageInAllThreads(queryClient, messageId, (m) => ({ ...m, isDeleted: true, body: "" }));
    },
  });
}

export { updateMessageInAllThreads };
