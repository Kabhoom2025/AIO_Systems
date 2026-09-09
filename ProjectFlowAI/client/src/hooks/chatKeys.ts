import type { ChatThreadRef } from "../types";

export const chatKeys = {
  channels: (organizationId: string, projectId?: string) =>
    ["chat", "channels", organizationId, projectId ?? null] as const,
  conversations: (organizationId: string) =>
    ["chat", "conversations", organizationId] as const,
  messages: (thread: ChatThreadRef) => ["chat", "messages", thread.kind, thread.id] as const,
};
