import { useDroppable } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { Box, Chip, Paper, Stack, Typography } from "@mui/material";
import { EmptyState } from "../../components/EmptyState";
import type { KanbanBoard } from "../../types";
import { SortableTaskCard } from "./SortableTaskCard";

interface KanbanColumnProps {
  columnKey: keyof KanbanBoard;
  label: string;
  cards: KanbanBoard[keyof KanbanBoard];
  onCardClick: (id: string) => void;
}

export function KanbanColumn({ columnKey, label, cards, onCardClick }: KanbanColumnProps) {
  const { setNodeRef, isOver } = useDroppable({ id: columnKey, data: { columnKey } });

  return (
    <Paper
      variant="outlined"
      sx={{
        minWidth: 280,
        width: 280,
        flexShrink: 0,
        display: "flex",
        flexDirection: "column",
        maxHeight: "calc(100vh - 260px)",
        bgcolor: isOver ? "action.hover" : "background.paper",
        transition: "background-color 120ms ease",
      }}
    >
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        sx={{ px: 1.5, py: 1, borderBottom: 1, borderColor: "divider" }}
      >
        <Typography variant="subtitle2" fontWeight={700}>
          {label}
        </Typography>
        <Chip label={cards.length} size="small" />
      </Stack>
      <Box ref={setNodeRef} sx={{ flex: 1, overflowY: "auto", p: 1, minHeight: 120 }}>
        <SortableContext
          id={columnKey}
          items={cards.map((c) => c.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing={1}>
            {cards.map((card) => (
              <SortableTaskCard
                key={card.id}
                card={card}
                columnKey={columnKey}
                onClick={() => onCardClick(card.id)}
              />
            ))}
          </Stack>
        </SortableContext>
        {cards.length === 0 && (
          <EmptyState title="No tasks" description="Drag a task here." />
        )}
      </Box>
    </Paper>
  );
}
