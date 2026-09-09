export type MessageRole = 'User' | 'Assistant' | 'System';

export interface ChatMessage {
  id: string;
  conversationId: string;
  role: MessageRole;
  content: string;
  createdAt: string;
  tokenCount?: number | null;
  model?: string | null;
  /** Client-only: true while an assistant message is still streaming in. */
  isStreaming?: boolean;
  /** Client-only: true if this message failed to send/generate. */
  isError?: boolean;
}
