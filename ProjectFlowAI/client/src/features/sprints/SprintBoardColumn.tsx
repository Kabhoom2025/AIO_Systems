import { useDroppable } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { Box, Chip, Paper, Stack, Typography } from "@mui/material";
import { EmptyState } from "../../components/EmptyState";
import { SortableTaskCard } from "../projects/SortableTaskCard";
import { WORK_ITEM_STATUS_LABELS, type KanbanCard, type WorkItemStatus } from "../../types";

interface SprintBoardColumnProps {
  status: WorkItemStatus;
  cards: KanbanCard[];
  onCardClick: (id: string) => void;
}

export function SprintBoardColumn({ status, cards, onCardClick }: SprintBoardColumnProps) {
  const { setNodeRef, isOver } = useDroppable({ id: status, data: { status } });

  return (
    <Paper
      variant="outlined"
      sx={{
        minWidth: 280,
        width: 280,
        flexShrink: 0,
        display: "flex",
        flexDirection: "column",
        maxHeight: "calc(100vh - 320px)",
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
          {WORK_ITEM_STATUS_LABELS[status]}
        </Typography>
        <Chip label={cards.length} size="small" />
      </Stack>
      <Box ref={setNodeRef} sx={{ flex: 1, overflowY: "auto", p: 1, minHeight: 120 }}>
        <SortableContext id={status} items={cards.map((c) => c.id)} strategy={verticalListSortingStrategy}>
          <Stack spacing={1}>
            {cards.map((card) => (
              <SortableTaskCard
                key={card.id}
                card={card}
                columnKey={status}
                onClick={() => onCardClick(card.id)}
              />
            ))}
          </Stack>
        </SortableContext>
        {cards.length === 0 && <EmptyState title="No items" description="Drag a task here." />}
      </Box>
    </Paper>
  );
}
