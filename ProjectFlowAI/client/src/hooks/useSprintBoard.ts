import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";
import { workItemsApi } from "../api/workItems";
import type { KanbanCard, MoveWorkItemRequest, SprintBoard, WorkItemStatus } from "../types";

export function sprintBoardKey(sprintId: string) {
  return ["sprintBoard", sprintId] as const;
}

export function useSprintBoard(sprintId: string | undefined) {
  return useQuery({
    queryKey: sprintBoardKey(sprintId ?? ""),
    queryFn: () => sprintsApi.getBoard(sprintId as string),
    enabled: !!sprintId,
    staleTime: 10 * 1000,
  });
}

interface MoveSprintCardArgs {
  cardId: string;
  fromStatus: WorkItemStatus;
  toStatus: WorkItemStatus;
  toIndex: number;
}

/**
 * Moves a sprint-board card between/within status columns with an optimistic cache update,
 * mirroring useKanbanBoard's useMoveWorkItem but against the SprintBoard {columns:[...]} shape.
 */
export function useMoveSprintWorkItem(sprintId: string) {
  const queryClient = useQueryClient();
  const key = sprintBoardKey(sprintId);

  return useMutation({
    mutationFn: ({ cardId, toStatus, toIndex }: MoveSprintCardArgs) => {
      const payload: MoveWorkItemRequest = { status: toStatus, position: toIndex };
      return workItemsApi.move(cardId, payload);
    },
    onMutate: async ({ cardId, fromStatus, toStatus, toIndex }: MoveSprintCardArgs) => {
      await queryClient.cancelQueries({ queryKey: key });
      const previous = queryClient.getQueryData<SprintBoard>(key);
      if (previous) {
        const next: SprintBoard = { ...previous, columns: previous.columns.map((c) => ({ ...c, items: [...c.items] })) };
        const fromCol = next.columns.find((c) => c.status === fromStatus);
        const toCol = next.columns.find((c) => c.status === toStatus);
        if (fromCol && toCol) {
          const cardIndex = fromCol.items.findIndex((c) => c.id === cardId);
          if (cardIndex !== -1) {
            const [card] = fromCol.items.splice(cardIndex, 1);
            const clampedIndex = Math.max(0, Math.min(toIndex, toCol.items.length));
            toCol.items.splice(clampedIndex, 0, card as KanbanCard);
          }
        }
        queryClient.setQueryData<SprintBoard>(key, next);
      }
      return { previous };
    },
    onError: (_err, _vars, context) => {
      if (context?.previous) {
        queryClient.setQueryData(key, context.previous);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: key });
    },
  });
}
