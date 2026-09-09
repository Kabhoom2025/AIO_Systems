import SmartToyOutlinedIcon from "@mui/icons-material/SmartToyOutlined";
import SendIcon from "@mui/icons-material/Send";
import {
  Avatar,
  Box,
  CircularProgress,
  IconButton,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useEffect, useRef, useState } from "react";
import { EmptyState } from "../../components/EmptyState";
import { useAuthStore } from "../../store/authStore";
import type { AiChatMessage } from "../../types";

interface PendingExchange {
  userMessage: string;
}

interface AiChatThreadProps {
  messages: AiChatMessage[] | undefined;
  isLoading: boolean;
  pending: PendingExchange | null;
  sending: boolean;
  onSend: (message: string) => void;
}

function ChatBubble({
  role,
  content,
  authorLabel,
  loading,
}: {
  role: "User" | "Assistant";
  content: string;
  authorLabel: string;
  loading?: boolean;
}) {
  const isAssistant = role === "Assistant";
  return (
    <Stack direction="row" spacing={1.5} sx={{ px: 2, py: 1 }}>
      <Avatar
        sx={{
          width: 32,
          height: 32,
          fontSize: 13,
          bgcolor: isAssistant ? "primary.main" : "secondary.main",
        }}
      >
        {isAssistant ? <SmartToyOutlinedIcon fontSize="small" /> : authorLabel.slice(0, 2).toUpperCase()}
      </Avatar>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography variant="body2" fontWeight={700}>
          {isAssistant ? "AI Assistant" : authorLabel}
        </Typography>
        {loading ? (
          <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 0.5 }}>
            <CircularProgress size={14} />
            <Typography variant="body2" color="text.secondary" fontStyle="italic">
              Thinking...
            </Typography>
          </Stack>
        ) : (
          <Typography variant="body2" sx={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}>
            {content}
          </Typography>
        )}
      </Box>
    </Stack>
  );
}

export function AiChatThread({ messages, isLoading, pending, sending, onSend }: AiChatThreadProps) {
  const [text, setText] = useState("");
  const user = useAuthStore((s) => s.user);
  const authorLabel = user ? `${user.firstName} ${user.lastName}` : "You";
  const scrollRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = scrollRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [messages?.length, pending]);

  const handleSend = () => {
    const trimmed = text.trim();
    if (!trimmed || sending) return;
    onSend(trimmed);
    setText("");
  };

  const hasContent = (messages && messages.length > 0) || !!pending;

  return (
    <Stack sx={{ flex: 1, minHeight: 0 }}>
      <Box ref={scrollRef} sx={{ flex: 1, overflowY: "auto", py: 1 }}>
        {isLoading ? (
          <Stack alignItems="center" sx={{ py: 6 }}>
            <CircularProgress size={24} />
          </Stack>
        ) : !hasContent ? (
          <EmptyState
            icon={<SmartToyOutlinedIcon sx={{ fontSize: 48 }} />}
            title="Ask the project assistant anything"
            description="It can answer questions about this project's tasks, sprints, and status."
          />
        ) : (
          <>
            {(messages ?? []).map((m) => (
              <ChatBubble key={m.id} role={m.role} content={m.content} authorLabel={authorLabel} />
            ))}
            {pending && (
              <>
                <ChatBubble role="User" content={pending.userMessage} authorLabel={authorLabel} />
                <ChatBubble role="Assistant" content="" authorLabel={authorLabel} loading />
              </>
            )}
          </>
        )}
      </Box>

      <Box sx={{ borderTop: 1, borderColor: "divider", p: 1.5 }}>
        <Stack direction="row" spacing={1} alignItems="flex-end">
          <TextField
            fullWidth
            multiline
            maxRows={6}
            size="small"
            placeholder="Ask the assistant about this project..."
            value={text}
            onChange={(e) => setText(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
          />
          <Tooltip title="Send">
            <span>
              <IconButton color="primary" disabled={!text.trim() || sending} onClick={handleSend}>
                <SendIcon />
              </IconButton>
            </span>
          </Tooltip>
        </Stack>
      </Box>
    </Stack>
  );
}
