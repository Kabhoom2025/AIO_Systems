import ChatBubbleOutlineIcon from "@mui/icons-material/ChatBubbleOutline";
import ChecklistOutlinedIcon from "@mui/icons-material/ChecklistOutlined";
import EventOutlinedIcon from "@mui/icons-material/EventOutlined";
import { Avatar, Box, Card, CardContent, Chip, Stack, Tooltip, Typography } from "@mui/material";
import { format } from "date-fns";
import type { KanbanCard } from "../../types";
import { PRIORITY_COLOR, TYPE_ICON } from "./kanbanMeta";

interface TaskCardProps {
  card: KanbanCard;
  onClick?: () => void;
  dragging?: boolean;
}

export function TaskCard({ card, onClick, dragging }: TaskCardProps) {
  const initials = card.assigneeName
    ? card.assigneeName
        .split(" ")
        .map((p) => p[0])
        .slice(0, 2)
        .join("")
        .toUpperCase()
    : undefined;

  return (
    <Card
      variant="outlined"
      onClick={onClick}
      sx={{
        cursor: "pointer",
        opacity: dragging ? 0.6 : 1,
        boxShadow: dragging ? 4 : 0,
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <CardContent sx={{ p: 1.5, "&:last-child": { pb: 1.5 } }}>
        <Stack direction="row" spacing={0.75} alignItems="center" sx={{ mb: 0.75 }}>
          {TYPE_ICON[card.type]}
          <Chip
            label={card.priority}
            size="small"
            color={PRIORITY_COLOR[card.priority]}
            variant="outlined"
            sx={{ height: 20, fontSize: "0.65rem" }}
          />
          {card.storyPoints != null && (
            <Chip
              label={`${card.storyPoints} pt`}
              size="small"
              variant="outlined"
              sx={{ height: 20, fontSize: "0.65rem" }}
            />
          )}
        </Stack>

        <Typography variant="body2" fontWeight={600} sx={{ mb: 1 }}>
          {card.title}
        </Typography>

        {card.labelColors.length > 0 && (
          <Stack direction="row" spacing={0.5} sx={{ mb: 1 }}>
            {card.labelColors.map((color, i) => (
              <Box
                key={`${color}-${i}`}
                sx={{ width: 14, height: 6, borderRadius: 1, bgcolor: color }}
              />
            ))}
          </Stack>
        )}

        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Stack direction="row" spacing={1.25} alignItems="center" color="text.secondary">
            {card.checklistTotal > 0 && (
              <Stack direction="row" spacing={0.4} alignItems="center">
                <ChecklistOutlinedIcon sx={{ fontSize: 15 }} />
                <Typography variant="caption">
                  {card.checklistDone}/{card.checklistTotal}
                </Typography>
              </Stack>
            )}
            {card.commentCount > 0 && (
              <Stack direction="row" spacing={0.4} alignItems="center">
                <ChatBubbleOutlineIcon sx={{ fontSize: 15 }} />
                <Typography variant="caption">{card.commentCount}</Typography>
              </Stack>
            )}
            {card.dueDate && (
              <Stack direction="row" spacing={0.4} alignItems="center">
                <EventOutlinedIcon sx={{ fontSize: 15 }} />
                <Typography variant="caption">{format(new Date(card.dueDate), "MMM d")}</Typography>
              </Stack>
            )}
          </Stack>
          {card.assigneeName && (
            <Tooltip title={card.assigneeName}>
              <Avatar sx={{ width: 24, height: 24, fontSize: 11, bgcolor: "primary.main" }}>
                {initials}
              </Avatar>
            </Tooltip>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
