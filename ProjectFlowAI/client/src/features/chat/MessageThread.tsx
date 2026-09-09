import { Box, Button, CircularProgress, Stack } from "@mui/material";
import { useEffect, useRef } from "react";
import { EmptyState } from "../../components/EmptyState";
import { useChatMessages, useDeleteMessage, useReactToMessage, useSendMessage } from "../../hooks/useChatMessages";
import { useChatSocket } from "../../providers/ChatSocketProvider";
import type { ChatThreadRef } from "../../types";
import { MessageBubble } from "./MessageBubble";
import { MessageComposer } from "./MessageComposer";
import { TypingIndicator } from "./TypingIndicator";
import { chatApi } from "../../api/chat";

interface MessageThreadProps {
  thread: ChatThreadRef;
  otherUserName?: string;
  onOpenReplies: (messageId: string) => void;
}

export function MessageThread({ thread, otherUserName, onOpenReplies }: MessageThreadProps) {
  const { messages, isLoading, hasOlder, loadingOlder, fetchOlder } = useChatMessages(thread);
  const sendMessage = useSendMessage(thread);
  const reactToMessage = useReactToMessage();
  const deleteMessage = useDeleteMessage();
  const { notifyTyping, typingLabelFor } = useChatSocket();

  const scrollRef = useRef<HTMLDivElement | null>(null);
  const prevLenRef = useRef(0);

  useEffect(() => {
    // Auto-scroll to bottom on initial load and when new messages arrive at the end, but not
    // when older history was prepended by "load older".
    const el = scrollRef.current;
    if (!el) return;
    if (messages.length > prevLenRef.current) {
      el.scrollTop = el.scrollHeight;
    }
    prevLenRef.current = messages.length;
  }, [messages.length]);

  const typingLabelRaw = typingLabelFor(thread);
  const typingLabel = typingLabelRaw
    ? thread.kind === "conversation" && otherUserName
      ? `${otherUserName} is typing...`
      : "Someone is typing..."
    : null;

  const topLevelMessages = messages.filter((m) => !m.parentMessageId);

  return (
    <Stack sx={{ flex: 1, minHeight: 0 }}>
      <Box ref={scrollRef} sx={{ flex: 1, overflowY: "auto", py: 1 }}>
        {hasOlder && (
          <Stack alignItems="center" sx={{ py: 1 }}>
            <Button size="small" disabled={loadingOlder} onClick={() => fetchOlder()}>
              {loadingOlder ? <CircularProgress size={16} /> : "Load older messages"}
            </Button>
          </Stack>
        )}

        {!isLoading && topLevelMessages.length === 0 && (
          <EmptyState title="No messages yet" description="Say hello to get the conversation started." />
        )}

        {topLevelMessages.map((m) => (
          <MessageBubble
            key={m.id}
            message={m}
            onReact={(emoji) => reactToMessage.mutate({ messageId: m.id, emoji })}
            onReply={() => onOpenReplies(m.id)}
            onDelete={() => deleteMessage.mutate(m.id)}
          />
        ))}
      </Box>

      {typingLabel && <TypingIndicator label={typingLabel} />}

      <MessageComposer
        placeholder={thread.kind === "channel" ? "Message channel..." : `Message ${otherUserName ?? ""}...`}
        sending={sendMessage.isPending}
        onSend={(body) => sendMessage.mutate({ body, parentMessageId: null })}
        onTyping={() => notifyTyping(thread, otherUserName)}
        onAttach={async (file) => {
          // Attachments are uploaded against a message id per the contract, so we first send a
          // placeholder-free message with the file name as body, then attach the file to it.
          const created = await sendMessage.mutateAsync({ body: `📎 ${file.name}`, parentMessageId: null });
          await chatApi.uploadAttachment(created.id, file);
        }}
      />
    </Stack>
  );
}
