import AddReactionOutlinedIcon from "@mui/icons-material/AddReactionOutlined";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import InsertDriveFileOutlinedIcon from "@mui/icons-material/InsertDriveFileOutlined";
import ReplyOutlinedIcon from "@mui/icons-material/ReplyOutlined";
import {
  Avatar,
  Box,
  Chip,
  IconButton,
  Link,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useAuthStore } from "../../store/authStore";
import type { ChatMessage } from "../../types";
import { EmojiPicker } from "./EmojiPicker";

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

interface MessageBubbleProps {
  message: ChatMessage;
  onReact: (emoji: string) => void;
  onReply?: () => void;
  onDelete?: () => void;
  showThreadOpener?: boolean;
}

export function MessageBubble({
  message,
  onReact,
  onReply,
  onDelete,
  showThreadOpener = true,
}: MessageBubbleProps) {
  const currentUserId = useAuthStore((s) => s.user?.id);
  const [hovered, setHovered] = useState(false);
  const [emojiAnchor, setEmojiAnchor] = useState<HTMLElement | null>(null);
  const isOwn = message.authorUserId === currentUserId;

  return (
    <Box
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      sx={{
        position: "relative",
        px: 2,
        py: 1,
        "&:hover": { bgcolor: "action.hover" },
        borderRadius: 1,
      }}
    >
      <Stack direction="row" spacing={1.5}>
        <Avatar sx={{ width: 32, height: 32, fontSize: 13 }}>
          {message.authorName.slice(0, 2).toUpperCase()}
        </Avatar>
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Stack direction="row" spacing={1} alignItems="baseline">
            <Typography variant="body2" fontWeight={700}>
              {message.authorName}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {formatTime(message.createdAt)}
            </Typography>
            {message.editedAt && !message.isDeleted && (
              <Typography variant="caption" color="text.disabled">
                (edited)
              </Typography>
            )}
          </Stack>

          {message.isDeleted ? (
            <Typography variant="body2" color="text.disabled" fontStyle="italic">
              This message was deleted
            </Typography>
          ) : (
            <>
              <Typography variant="body2" sx={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}>
                {message.body}
              </Typography>

              {message.attachments.length > 0 && (
                <Stack spacing={0.5} sx={{ mt: 0.5 }}>
                  {message.attachments.map((a) => (
                    <Stack key={a.id} direction="row" spacing={0.75} alignItems="center">
                      <InsertDriveFileOutlinedIcon fontSize="small" color="action" />
                      <Link href={a.downloadUrl} target="_blank" rel="noreferrer" variant="body2">
                        {a.fileName}
                      </Link>
                      <Typography variant="caption" color="text.secondary">
                        ({formatBytes(a.fileSizeBytes)})
                      </Typography>
                    </Stack>
                  ))}
                </Stack>
              )}

              <Stack direction="row" spacing={0.5} flexWrap="wrap" sx={{ mt: 0.5 }}>
                {message.reactions.map((r) => (
                  <Chip
                    key={r.emoji}
                    size="small"
                    label={`${r.emoji} ${r.userIds.length}`}
                    variant={currentUserId && r.userIds.includes(currentUserId) ? "filled" : "outlined"}
                    color={currentUserId && r.userIds.includes(currentUserId) ? "primary" : "default"}
                    onClick={() => onReact(r.emoji)}
                    sx={{ cursor: "pointer", height: 22 }}
                  />
                ))}
              </Stack>

              {showThreadOpener && message.replyCount > 0 && onReply && (
                <Typography
                  variant="caption"
                  color="primary"
                  sx={{ cursor: "pointer", display: "inline-block", mt: 0.5 }}
                  onClick={onReply}
                >
                  {message.replyCount} {message.replyCount === 1 ? "reply" : "replies"}
                </Typography>
              )}
            </>
          )}
        </Box>

        {hovered && !message.isDeleted && (
          <Stack direction="row" spacing={0.25} sx={{ height: 28 }}>
            <Tooltip title="Add reaction">
              <IconButton size="small" onClick={(e) => setEmojiAnchor(e.currentTarget)}>
                <AddReactionOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            {onReply && (
              <Tooltip title="Reply in thread">
                <IconButton size="small" onClick={onReply}>
                  <ReplyOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {isOwn && onDelete && (
              <Tooltip title="Delete">
                <IconButton size="small" onClick={onDelete}>
                  <DeleteOutlineIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        )}
      </Stack>

      <EmojiPicker
        anchorEl={emojiAnchor}
        onClose={() => setEmojiAnchor(null)}
        onSelect={(emoji) => onReact(emoji)}
      />
    </Box>
  );
}
