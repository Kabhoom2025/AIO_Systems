import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { KanbanCard } from "../../types";
import { TaskCard } from "./TaskCard";

interface SortableTaskCardProps {
  card: KanbanCard;
  columnKey: string;
  onClick: () => void;
}

export function SortableTaskCard({ card, columnKey, onClick }: SortableTaskCardProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: card.id,
    data: { columnKey, card },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <div ref={setNodeRef} style={style} {...attributes} {...listeners}>
      <TaskCard card={card} onClick={onClick} dragging={isDragging} />
    </div>
  );
}
