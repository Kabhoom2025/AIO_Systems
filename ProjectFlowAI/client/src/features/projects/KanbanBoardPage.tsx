import {
  DndContext,
  DragOverlay,
  PointerSensor,
  closestCenter,
  useSensor,
  useSensors,
  type DragEndEvent,
  type DragStartEvent,
} from "@dnd-kit/core";
import AddIcon from "@mui/icons-material/Add";
import { Box, Button, Skeleton, Stack } from "@mui/material";
import { useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useCreateWorkItem } from "../../hooks/useWorkItems";
import { useKanbanBoard, useMoveWorkItem } from "../../hooks/useKanbanBoard";
import type { KanbanBoard, KanbanCard } from "../../types";
import { KANBAN_COLUMNS } from "./kanbanMeta";
import { KanbanColumn } from "./KanbanColumn";
import { TaskCard } from "./TaskCard";

function findColumnOf(board: KanbanBoard, cardId: string): keyof KanbanBoard | null {
  for (const col of KANBAN_COLUMNS) {
    if (board[col.key].some((c) => c.id === cardId)) return col.key;
  }
  return null;
}

export function KanbanBoardPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const { data: board, isLoading } = useKanbanBoard(projectId);
  const moveMutation = useMoveWorkItem(projectId ?? "");
  const createMutation = useCreateWorkItem(projectId ?? "");

  const [activeCard, setActiveCard] = useState<KanbanCard | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } })
  );

  const handleDragStart = (event: DragStartEvent) => {
    if (!board) return;
    const cardId = event.active.id as string;
    const col = findColumnOf(board, cardId);
    if (col) {
      const card = board[col].find((c) => c.id === cardId) ?? null;
      setActiveCard(card);
    }
  };

  const handleDragEnd = (event: DragEndEvent) => {
    setActiveCard(null);
    if (!board || !event.over) return;
    const cardId = event.active.id as string;
    const fromColumn = findColumnOf(board, cardId);
    if (!fromColumn) return;

    // The `over` target is either a column (droppable) or another card (sortable item).
    const overId = event.over.id as string;
    let toColumn = KANBAN_COLUMNS.find((c) => c.key === overId)?.key ?? null;
    let toIndex: number;

    if (toColumn) {
      // Dropped on the column's empty area / container itself -> append to end.
      toIndex = board[toColumn].length;
    } else {
      // Dropped on top of another card - find which column it belongs to.
      toColumn = findColumnOf(board, overId);
      if (!toColumn) return;
      toIndex = board[toColumn].findIndex((c) => c.id === overId);
      if (toIndex === -1) toIndex = board[toColumn].length;
    }

    if (fromColumn === toColumn) {
      const currentIndex = board[fromColumn].findIndex((c) => c.id === cardId);
      if (currentIndex === toIndex) return;
    }

    moveMutation.mutate({ cardId, fromColumn, toColumn, toIndex });
  };

  const totalCards = useMemo(
    () => (board ? KANBAN_COLUMNS.reduce((sum, c) => sum + board[c.key].length, 0) : 0),
    [board]
  );

  const handleQuickAdd = () => {
    if (!projectId) return;
    createMutation.mutate({ projectId, title: "New task", status: "Backlog" });
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Box color="text.secondary" sx={{ fontSize: 14 }}>
          {isLoading ? <Skeleton width={80} /> : `${totalCards} tasks`}
        </Box>
        <Button size="small" startIcon={<AddIcon />} onClick={handleQuickAdd}>
          Quick add task
        </Button>
      </Stack>

      {isLoading || !board ? (
        <Stack direction="row" spacing={2} sx={{ overflowX: "auto" }}>
          {KANBAN_COLUMNS.map((c) => (
            <Skeleton key={c.key} variant="rounded" width={280} height={420} sx={{ flexShrink: 0 }} />
          ))}
        </Stack>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragStart={handleDragStart}
          onDragEnd={handleDragEnd}
        >
          <Stack direction="row" spacing={2} sx={{ overflowX: "auto", pb: 1 }}>
            {KANBAN_COLUMNS.map((col) => (
              <KanbanColumn
                key={col.key}
                columnKey={col.key}
                label={col.label}
                cards={board[col.key]}
                onCardClick={(id) => navigate(`/projects/${projectId}/board?task=${id}`)}
              />
            ))}
          </Stack>
          <DragOverlay>{activeCard ? <TaskCard card={activeCard} /> : null}</DragOverlay>
        </DndContext>
      )}
    </Stack>
  );
}
