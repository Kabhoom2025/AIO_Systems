import AddCommentOutlinedIcon from "@mui/icons-material/AddCommentOutlined";
import ChatBubbleOutlineIcon from "@mui/icons-material/ChatBubbleOutline";
import {
  Box,
  Button,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Skeleton,
  Stack,
  Typography,
} from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { EmptyState } from "../../components/EmptyState";
import type { AiConversationSummary } from "../../types";

interface AiChatSidebarProps {
  conversations: AiConversationSummary[] | undefined;
  isLoading: boolean;
  selectedId: string | null;
  onSelect: (id: string | null) => void;
}

export function AiChatSidebar({ conversations, isLoading, selectedId, onSelect }: AiChatSidebarProps) {
  return (
    <Box sx={{ width: 280, borderRight: 1, borderColor: "divider", display: "flex", flexDirection: "column" }}>
      <Box sx={{ p: 1.5 }}>
        <Button
          fullWidth
          variant="outlined"
          startIcon={<AddCommentOutlinedIcon />}
          onClick={() => onSelect(null)}
        >
          New conversation
        </Button>
      </Box>
      <Box sx={{ flex: 1, overflowY: "auto" }}>
        {isLoading ? (
          <Stack spacing={1} sx={{ p: 1.5 }}>
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} variant="rounded" height={48} />
            ))}
          </Stack>
        ) : !conversations || conversations.length === 0 ? (
          <Box sx={{ p: 1.5 }}>
            <EmptyState
              icon={<ChatBubbleOutlineIcon sx={{ fontSize: 36 }} />}
              title="No conversations yet"
              description="Start a new conversation with the project assistant."
            />
          </Box>
        ) : (
          <List disablePadding>
            {conversations.map((c) => (
              <ListItemButton
                key={c.id}
                selected={c.id === selectedId}
                onClick={() => onSelect(c.id)}
                sx={{ borderRadius: 0, alignItems: "flex-start", py: 1 }}
              >
                <ListItemIcon sx={{ minWidth: 32, mt: 0.5 }}>
                  <ChatBubbleOutlineIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary={c.title || "Untitled conversation"}
                  slotProps={{ primary: { noWrap: true, fontWeight: 600, variant: "body2" } }}
                  secondary={
                    <Typography variant="caption" color="text.secondary">
                      {formatDistanceToNow(new Date(c.lastMessageAt), { addSuffix: true })}
                    </Typography>
                  }
                />
              </ListItemButton>
            ))}
          </List>
        )}
      </Box>
    </Box>
  );
}
