import CloseIcon from "@mui/icons-material/Close";
import { Box, Divider, IconButton, Stack, Typography } from "@mui/material";
import { EmptyState } from "../../components/EmptyState";
import { useChatMessages, useDeleteMessage, useReactToMessage, useSendMessage } from "../../hooks/useChatMessages";
import type { ChatThreadRef } from "../../types";
import { MessageBubble } from "./MessageBubble";
import { MessageComposer } from "./MessageComposer";

interface ThreadRepliesPanelProps {
  thread: ChatThreadRef;
  parentMessageId: string;
  onClose: () => void;
}

export function ThreadRepliesPanel({ thread, parentMessageId, onClose }: ThreadRepliesPanelProps) {
  const { messages } = useChatMessages(thread);
  const sendMessage = useSendMessage(thread);
  const reactToMessage = useReactToMessage();
  const deleteMessage = useDeleteMessage();

  const parent = messages.find((m) => m.id === parentMessageId);
  const replies = messages.filter((m) => m.parentMessageId === parentMessageId);

  return (
    <Box
      sx={{
        width: 360,
        flexShrink: 0,
        borderLeft: 1,
        borderColor: "divider",
        display: "flex",
        flexDirection: "column",
        height: "100%",
      }}
    >
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, py: 1.5 }}>
        <Typography variant="subtitle1" fontWeight={700}>
          Thread
        </Typography>
        <IconButton size="small" onClick={onClose}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </Stack>
      <Divider />

      <Box sx={{ flex: 1, overflowY: "auto" }}>
        {parent && (
          <>
            <MessageBubble
              message={parent}
              onReact={(emoji) => reactToMessage.mutate({ messageId: parent.id, emoji })}
              showThreadOpener={false}
            />
            <Divider sx={{ mx: 2 }} />
          </>
        )}

        {replies.length === 0 ? (
          <EmptyState title="No replies yet" description="Reply below to start the thread." />
        ) : (
          replies.map((m) => (
            <MessageBubble
              key={m.id}
              message={m}
              showThreadOpener={false}
              onReact={(emoji) => reactToMessage.mutate({ messageId: m.id, emoji })}
              onDelete={() => deleteMessage.mutate(m.id)}
            />
          ))
        )}
      </Box>

      <MessageComposer
        placeholder="Reply..."
        sending={sendMessage.isPending}
        onSend={(body) => sendMessage.mutate({ body, parentMessageId })}
      />
    </Box>
  );
}
