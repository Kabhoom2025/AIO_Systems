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
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import ForumOutlinedIcon from "@mui/icons-material/ForumOutlined";
import ShowChartIcon from "@mui/icons-material/ShowChart";
import {
  Box,
  Button,
  Chip,
  IconButton,
  Skeleton,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { TaskCard } from "../projects/TaskCard";
import { useMoveSprintWorkItem, useSprintBoard } from "../../hooks/useSprintBoard";
import { useSprint } from "../../hooks/useSprints";
import type { KanbanCard, SprintBoard, WorkItemStatus } from "../../types";
import { WORK_ITEM_STATUSES } from "../../types";
import { SPRINT_STATUS_COLOR } from "./sprintMeta";
import { SprintBoardColumn } from "./SprintBoardColumn";
import { BurndownChart } from "./BurndownChart";
import { BurnupChart } from "./BurnupChart";

function findStatusOf(board: SprintBoard, cardId: string): WorkItemStatus | null {
  for (const col of board.columns) {
    if (col.items.some((c) => c.id === cardId)) return col.status;
  }
  return null;
}

export function SprintBoardPage() {
  const { projectId, sprintId } = useParams<{ projectId: string; sprintId: string }>();
  const navigate = useNavigate();
  const { data: sprint } = useSprint(sprintId);
  const { data: board, isLoading } = useSprintBoard(sprintId);
  const moveMutation = useMoveSprintWorkItem(sprintId ?? "");
  const [activeCard, setActiveCard] = useState<KanbanCard | null>(null);
  const [chartView, setChartView] = useState<"none" | "burndown" | "burnup">("none");

  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

  const orderedColumns = useMemo(() => {
    if (!board) return [];
    const byStatus = new Map(board.columns.map((c) => [c.status, c]));
    return WORK_ITEM_STATUSES.filter((s) => byStatus.has(s)).map((s) => byStatus.get(s)!);
  }, [board]);

  const handleDragStart = (event: DragStartEvent) => {
    if (!board) return;
    const cardId = event.active.id as string;
    const status = findStatusOf(board, cardId);
    if (status) {
      const card = board.columns.find((c) => c.status === status)?.items.find((c) => c.id === cardId) ?? null;
      setActiveCard(card);
    }
  };

  const handleDragEnd = (event: DragEndEvent) => {
    setActiveCard(null);
    if (!board || !event.over) return;
    const cardId = event.active.id as string;
    const fromStatus = findStatusOf(board, cardId);
    if (!fromStatus) return;

    const overId = event.over.id as string;
    let toStatus = WORK_ITEM_STATUSES.find((s) => s === overId) ?? null;
    let toIndex: number;

    const toCol = (status: WorkItemStatus) => board.columns.find((c) => c.status === status);

    if (toStatus) {
      toIndex = toCol(toStatus)?.items.length ?? 0;
    } else {
      toStatus = findStatusOf(board, overId);
      if (!toStatus) return;
      const col = toCol(toStatus);
      toIndex = col ? col.items.findIndex((c) => c.id === overId) : 0;
      if (toIndex === -1) toIndex = col?.items.length ?? 0;
    }

    if (fromStatus === toStatus) {
      const currentIndex = toCol(fromStatus)?.items.findIndex((c) => c.id === cardId) ?? -1;
      if (currentIndex === toIndex) return;
    }

    moveMutation.mutate({ cardId, fromStatus, toStatus, toIndex });
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" spacing={1.5}>
        <IconButton aria-label="Back to sprints" onClick={() => navigate(`/projects/${projectId}/sprints`)}>
          <ArrowBackIcon />
        </IconButton>
        {sprint ? (
          <Stack direction="row" spacing={1.5} alignItems="center" sx={{ flex: 1 }}>
            <Typography variant="h6" fontWeight={700}>
              {sprint.name}
            </Typography>
            <Chip size="small" label={sprint.status} color={SPRINT_STATUS_COLOR[sprint.status]} />
          </Stack>
        ) : (
          <Skeleton variant="text" width={180} height={32} sx={{ flex: 1 }} />
        )}
        <Button
          size="small"
          startIcon={<ForumOutlinedIcon fontSize="small" />}
          onClick={() => navigate(`/projects/${projectId}/sprints/${sprintId}/retrospective`)}
        >
          Retrospective
        </Button>
        <ToggleButtonGroup
          size="small"
          exclusive
          value={chartView}
          onChange={(_, v) => setChartView(v ?? "none")}
        >
          <ToggleButton value="none">Board</ToggleButton>
          <ToggleButton value="burndown">
            <ShowChartIcon fontSize="small" sx={{ mr: 0.5 }} /> Burndown
          </ToggleButton>
          <ToggleButton value="burnup">
            <ShowChartIcon fontSize="small" sx={{ mr: 0.5 }} /> Burnup
          </ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      {chartView === "burndown" && sprintId && <BurndownChart sprintId={sprintId} />}
      {chartView === "burnup" && sprintId && <BurnupChart sprintId={sprintId} />}

      {chartView === "none" &&
        (isLoading || !board ? (
          <Stack direction="row" spacing={2} sx={{ overflowX: "auto" }}>
            {WORK_ITEM_STATUSES.map((s) => (
              <Skeleton key={s} variant="rounded" width={280} height={420} sx={{ flexShrink: 0 }} />
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
              {orderedColumns.map((col) => (
                <SprintBoardColumn
                  key={col.status}
                  status={col.status}
                  cards={col.items}
                  onCardClick={(id) => navigate(`/projects/${projectId}/sprints/${sprintId}/board?task=${id}`)}
                />
              ))}
            </Stack>
            <DragOverlay>{activeCard ? <TaskCard card={activeCard} /> : null}</DragOverlay>
          </DndContext>
        ))}
      <Box />
    </Stack>
  );
}
