import type { InfiniteData, QueryClient } from "@tanstack/react-query";
import type { ChatMessage, ChatMessagesResponse } from "../types";

type MessagesPageData = InfiniteData<ChatMessagesResponse>;

/** Applies `updater` to every cached chat-messages query that contains `messageId`. */
export function updateMessageInAllThreads(
  queryClient: QueryClient,
  messageId: string,
  updater: (message: ChatMessage) => ChatMessage
) {
  queryClient.setQueriesData<MessagesPageData>(
    { queryKey: ["chat", "messages"] },
    (data) => {
      if (!data) return data;
      let changed = false;
      const pages = data.pages.map((page) => {
        let pageChanged = false;
        const messages = page.messages.map((m) => {
          if (m.id === messageId) {
            pageChanged = true;
            return updater(m);
          }
          return m;
        });
        if (!pageChanged) return page;
        changed = true;
        return { ...page, messages };
      });
      if (!changed) return data;
      return { ...data, pages };
    }
  );
}

/** Prepends a newly-received message into the matching thread's cache (into the newest page). */
export function prependMessageToThread(
  queryClient: QueryClient,
  queryKey: readonly unknown[],
  message: ChatMessage
) {
  queryClient.setQueryData<MessagesPageData>(queryKey, (data) => {
    if (!data || data.pages.length === 0) {
      return {
        pages: [{ messages: [message] }],
        pageParams: [undefined],
      } as MessagesPageData;
    }
    const firstPage = data.pages[0];
    if (firstPage.messages.some((m) => m.id === message.id)) {
      return data;
    }
    const pages = [...data.pages];
    pages[0] = { ...firstPage, messages: [message, ...firstPage.messages] };
    return { ...data, pages };
  });
}
