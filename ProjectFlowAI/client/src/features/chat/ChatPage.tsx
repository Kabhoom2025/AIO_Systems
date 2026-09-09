import { Box, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { AppShell } from "../../components/AppShell";
import { EmptyState } from "../../components/EmptyState";
import { useMarkChannelRead } from "../../hooks/useChatChannels";
import { useAuthStore } from "../../store/authStore";
import type { ChatThreadRef } from "../../types";
import { ChannelList } from "./ChannelList";
import { MessageThread } from "./MessageThread";
import { ThreadRepliesPanel } from "./ThreadRepliesPanel";

export function ChatPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";
  const [selected, setSelected] = useState<ChatThreadRef | null>(null);
  const [otherUserName, setOtherUserName] = useState<string | undefined>(undefined);
  const [openReplyParentId, setOpenReplyParentId] = useState<string | null>(null);
  const markChannelRead = useMarkChannelRead(organizationId);

  useEffect(() => {
    setOpenReplyParentId(null);
    if (selected?.kind === "channel") {
      markChannelRead.mutate(selected.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selected?.kind, selected?.id]);

  return (
    <AppShell>
      <Stack sx={{ height: "calc(100vh - 96px)" }} spacing={1}>
        <Typography variant="h5" fontWeight={700}>
          Chat
        </Typography>
        <Box sx={{ flex: 1, minHeight: 0, display: "flex", border: 1, borderColor: "divider", borderRadius: 1 }}>
          <ChannelList
            selected={selected}
            onSelect={(thread, name) => {
              setSelected(thread);
              setOtherUserName(name);
            }}
          />

          {!selected ? (
            <Box sx={{ flex: 1 }}>
              <EmptyState
                title="Select a conversation"
                description="Pick a channel or direct message from the sidebar, or start a new one."
              />
            </Box>
          ) : (
            <MessageThread
              thread={selected}
              otherUserName={otherUserName}
              onOpenReplies={(messageId) => setOpenReplyParentId(messageId)}
            />
          )}

          {selected && openReplyParentId && (
            <ThreadRepliesPanel
              thread={selected}
              parentMessageId={openReplyParentId}
              onClose={() => setOpenReplyParentId(null)}
            />
          )}
        </Box>
      </Stack>
    </AppShell>
  );
}
