import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";
import type { KanbanBoard, KanbanCard, MoveWorkItemRequest } from "../types";
import { KANBAN_COLUMN_TO_STATUS } from "../types";

export function kanbanKey(projectId: string) {
  return ["kanban", projectId] as const;
}

export function useKanbanBoard(projectId: string | undefined) {
  return useQuery({
    queryKey: kanbanKey(projectId ?? ""),
    queryFn: () => workItemsApi.getKanban(projectId as string),
    enabled: !!projectId,
    staleTime: 10 * 1000,
  });
}

interface MoveCardArgs {
  cardId: string;
  fromColumn: keyof KanbanBoard;
  toColumn: keyof KanbanBoard;
  toIndex: number;
}

/**
 * Moves a card between/within columns with an optimistic cache update, then
 * calls POST /work-items/{id}/move. Rolls back to the previous cache snapshot
 * on error.
 */
export function useMoveWorkItem(projectId: string) {
  const queryClient = useQueryClient();
  const key = kanbanKey(projectId);

  return useMutation({
    mutationFn: ({ cardId, toColumn, toIndex }: MoveCardArgs) => {
      const payload: MoveWorkItemRequest = {
        status: KANBAN_COLUMN_TO_STATUS[toColumn],
        position: toIndex,
      };
      return workItemsApi.move(cardId, payload);
    },
    onMutate: async ({ cardId, fromColumn, toColumn, toIndex }: MoveCardArgs) => {
      await queryClient.cancelQueries({ queryKey: key });
      const previous = queryClient.getQueryData<KanbanBoard>(key);
      if (previous) {
        const next: KanbanBoard = { ...previous };
        const fromList = [...next[fromColumn]];
        const cardIndex = fromList.findIndex((c) => c.id === cardId);
        if (cardIndex !== -1) {
          const [card] = fromList.splice(cardIndex, 1);
          next[fromColumn] = fromList;
          const toList = fromColumn === toColumn ? fromList : [...next[toColumn]];
          const clampedIndex = Math.max(0, Math.min(toIndex, toList.length));
          toList.splice(clampedIndex, 0, card);
          next[toColumn] = toList;
        }
        queryClient.setQueryData<KanbanBoard>(key, next);
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

export type { KanbanCard };
