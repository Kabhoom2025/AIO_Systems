import AttachFileIcon from "@mui/icons-material/AttachFile";
import EmojiEmotionsOutlinedIcon from "@mui/icons-material/EmojiEmotionsOutlined";
import SendIcon from "@mui/icons-material/Send";
import { Box, CircularProgress, IconButton, Stack, TextField, Tooltip } from "@mui/material";
import { useRef, useState } from "react";
import { EmojiPicker } from "./EmojiPicker";

interface MessageComposerProps {
  onSend: (body: string) => void;
  onTyping?: () => void;
  onAttach?: (file: File) => void;
  attaching?: boolean;
  sending?: boolean;
  placeholder?: string;
  replyContext?: string;
  onCancelReply?: () => void;
}

export function MessageComposer({
  onSend,
  onTyping,
  onAttach,
  attaching,
  sending,
  placeholder = "Message...",
  replyContext,
  onCancelReply,
}: MessageComposerProps) {
  const [text, setText] = useState("");
  const [emojiAnchor, setEmojiAnchor] = useState<HTMLElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const handleSend = () => {
    const trimmed = text.trim();
    if (!trimmed) return;
    onSend(trimmed);
    setText("");
  };

  return (
    <Box sx={{ borderTop: 1, borderColor: "divider", p: 1.5 }}>
      {replyContext && (
        <Stack
          direction="row"
          alignItems="center"
          justifyContent="space-between"
          sx={{ px: 1, py: 0.5, mb: 1, bgcolor: "action.hover", borderRadius: 1 }}
        >
          <Box sx={{ fontSize: 13, color: "text.secondary" }}>Replying to: {replyContext}</Box>
          <IconButton size="small" onClick={onCancelReply}>
            ✕
          </IconButton>
        </Stack>
      )}
      <Stack direction="row" spacing={1} alignItems="flex-end">
        <TextField
          fullWidth
          multiline
          maxRows={6}
          size="small"
          placeholder={placeholder}
          value={text}
          onChange={(e) => {
            setText(e.target.value);
            onTyping?.();
          }}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault();
              handleSend();
            }
          }}
        />
        <Tooltip title="Emoji">
          <IconButton onClick={(e) => setEmojiAnchor(e.currentTarget)}>
            <EmojiEmotionsOutlinedIcon />
          </IconButton>
        </Tooltip>
        {onAttach && (
          <Tooltip title="Attach file">
            <span>
              <IconButton disabled={attaching} onClick={() => fileInputRef.current?.click()}>
                {attaching ? <CircularProgress size={20} /> : <AttachFileIcon />}
              </IconButton>
            </span>
          </Tooltip>
        )}
        <input
          ref={fileInputRef}
          type="file"
          hidden
          onChange={(e) => {
            const file = e.target.files?.[0];
            e.target.value = "";
            if (file) onAttach?.(file);
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
      <EmojiPicker
        anchorEl={emojiAnchor}
        onClose={() => setEmojiAnchor(null)}
        onSelect={(emoji) => setText((t) => t + emoji)}
      />
    </Box>
  );
}
