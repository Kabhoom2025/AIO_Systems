import { Box, Stack, Typography } from "@mui/material";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { useAiChatMessages, useAiConversations, useSendAiChatMessage } from "../../hooks/useAiChat";
import { AiChatSidebar } from "./AiChatSidebar";
import { AiChatThread } from "./AiChatThread";
import { AiUnavailableState } from "./AiUnavailableState";
import { isAiUnconfiguredError } from "../../utils/apiErrors";

export function AiChatPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [pendingUserMessage, setPendingUserMessage] = useState<string | null>(null);

  const conversations = useAiConversations(projectId);
  const messages = useAiChatMessages(conversationId ?? undefined);
  const sendMessage = useSendAiChatMessage(projectId ?? "");

  const handleSend = (message: string) => {
    setPendingUserMessage(message);
    sendMessage.mutate(
      { message, conversationId },
      {
        onSuccess: (data) => {
          setConversationId(data.conversationId);
          setPendingUserMessage(null);
        },
        onError: () => setPendingUserMessage(null),
      }
    );
  };

  // A 503 (AI not configured) can surface either from loading the conversation list or
  // from sending a message in a fresh session where nothing has loaded yet.
  const unconfigured =
    isAiUnconfiguredError(conversations.error) || isAiUnconfiguredError(sendMessage.error);

  return (
    <Stack sx={{ height: "calc(100vh - 220px)", minHeight: 480 }} spacing={1}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Assistant Chat
        </Typography>
        <Typography variant="body2" color="text.secondary">
          A conversational assistant scoped to this project.
        </Typography>
      </Box>

      {unconfigured ? (
        <Box sx={{ flex: 1, border: 1, borderColor: "divider", borderRadius: 1 }}>
          <AiUnavailableState error={conversations.error ?? sendMessage.error} />
        </Box>
      ) : (
        <Box sx={{ flex: 1, minHeight: 0, display: "flex", border: 1, borderColor: "divider", borderRadius: 1 }}>
          <AiChatSidebar
            conversations={conversations.data}
            isLoading={conversations.isLoading}
            selectedId={conversationId}
            onSelect={setConversationId}
          />
          <AiChatThread
            messages={messages.data}
            isLoading={!!conversationId && messages.isLoading}
            pending={pendingUserMessage ? { userMessage: pendingUserMessage } : null}
            sending={sendMessage.isPending}
            onSend={handleSend}
          />
        </Box>
      )}
    </Stack>
  );
}
