import { Box, Popover } from "@mui/material";

const EMOJIS = [
  "👍", "👎", "😀", "😂", "😍", "🎉", "🚀", "👀",
  "❤️", "🔥", "🙏", "😢", "😮", "🤔", "✅", "❌",
  "💯", "👏", "🙌", "😅", "🤝", "⭐", "🐛", "⚡",
];

interface EmojiPickerProps {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  onSelect: (emoji: string) => void;
}

export function EmojiPicker({ anchorEl, onClose, onSelect }: EmojiPickerProps) {
  return (
    <Popover
      open={!!anchorEl}
      anchorEl={anchorEl}
      onClose={onClose}
      anchorOrigin={{ vertical: "top", horizontal: "left" }}
      transformOrigin={{ vertical: "bottom", horizontal: "left" }}
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: "repeat(8, 1fr)",
          gap: 0.5,
          p: 1,
          width: 240,
        }}
      >
        {EMOJIS.map((emoji) => (
          <Box
            key={emoji}
            component="button"
            onClick={() => {
              onSelect(emoji);
              onClose();
            }}
            sx={{
              border: "none",
              background: "none",
              cursor: "pointer",
              fontSize: 20,
              lineHeight: 1,
              p: 0.75,
              borderRadius: 1,
              "&:hover": { bgcolor: "action.hover" },
            }}
          >
            {emoji}
          </Box>
        ))}
      </Box>
    </Popover>
  );
}
