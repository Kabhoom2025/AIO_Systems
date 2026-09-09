import { Box, Typography, keyframes } from "@mui/material";

const bounce = keyframes`
  0%, 80%, 100% { transform: scale(0.6); opacity: 0.4; }
  40% { transform: scale(1); opacity: 1; }
`;

function Dot({ delay }: { delay: string }) {
  return (
    <Box
      sx={{
        width: 5,
        height: 5,
        borderRadius: "50%",
        bgcolor: "text.secondary",
        display: "inline-block",
        mx: 0.25,
        animation: `${bounce} 1.2s infinite`,
        animationDelay: delay,
      }}
    />
  );
}

interface TypingIndicatorProps {
  label: string;
}

export function TypingIndicator({ label }: TypingIndicatorProps) {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 0.75, px: 2, py: 0.5, height: 28 }}>
      <Box>
        <Dot delay="0s" />
        <Dot delay="0.15s" />
        <Dot delay="0.3s" />
      </Box>
      <Typography variant="caption" color="text.secondary" fontStyle="italic">
        {label}
      </Typography>
    </Box>
  );
}
