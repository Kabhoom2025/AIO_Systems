import { useMemo, useState } from "react";
import { Button, Chip, MenuItem, Select, Stack, Typography } from "@mui/material";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { EmptyState } from "../../components/EmptyState";
import { useWorkItems } from "../../hooks/useWorkItems";
import { useSetWorkItemSprint } from "../../hooks/useWorkItems";
import type { Sprint, WorkItem } from "../../types";

interface BacklogPanelProps {
  projectId: string;
  sprints: Sprint[];
}

export function BacklogPanel({ projectId, sprints }: BacklogPanelProps) {
  const { data, isLoading } = useWorkItems({ projectId, page: 1, pageSize: 200 });
  const setSprintMutation = useSetWorkItemSprint(projectId);
  const [selectedSprintByItem, setSelectedSprintByItem] = useState<Record<string, string>>({});

  const backlogItems = useMemo(
    () => (data?.items ?? []).filter((w) => !w.sprintId),
    [data]
  );

  const assignableSprints = sprints.filter((s) => s.status !== "Completed");

  const columns: DataTableColumn<WorkItem>[] = [
    { key: "title", label: "Title", render: (row) => row.title },
    {
      key: "type",
      label: "Type",
      render: (row) => <Chip size="small" label={row.type} variant="outlined" />,
    },
    {
      key: "priority",
      label: "Priority",
      render: (row) => <Chip size="small" label={row.priority} variant="outlined" />,
    },
    {
      key: "storyPoints",
      label: "Points",
      align: "right",
      render: (row) => row.storyPoints ?? "—",
    },
    {
      key: "addToSprint",
      label: "Add to sprint",
      align: "right",
      render: (row) => (
        <Stack direction="row" spacing={1} justifyContent="flex-end">
          <Select
            size="small"
            displayEmpty
            value={selectedSprintByItem[row.id] ?? ""}
            onChange={(e) =>
              setSelectedSprintByItem((prev) => ({ ...prev, [row.id]: e.target.value }))
            }
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="" disabled>
              Choose sprint
            </MenuItem>
            {assignableSprints.map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {s.name}
              </MenuItem>
            ))}
          </Select>
          <Button
            size="small"
            variant="outlined"
            disabled={!selectedSprintByItem[row.id] || setSprintMutation.isPending}
            onClick={() => {
              const sprintId = selectedSprintByItem[row.id];
              if (sprintId) {
                setSprintMutation.mutate({ id: row.id, sprintId });
              }
            }}
          >
            Add
          </Button>
        </Stack>
      ),
    },
  ];

  return (
    <Stack spacing={1.5}>
      <Typography variant="body2" color="text.secondary">
        Work items not yet assigned to a sprint. Move them into an upcoming or active sprint to
        plan the next iteration.
      </Typography>
      {!isLoading && backlogItems.length === 0 ? (
        <EmptyState title="Backlog is empty" description="Every work item is assigned to a sprint." />
      ) : (
        <DataTable
          columns={columns}
          rows={backlogItems}
          getRowId={(row) => row.id}
          isLoading={isLoading}
          totalCount={backlogItems.length}
          page={0}
          pageSize={backlogItems.length || 10}
          onPageChange={() => {}}
          onPageSizeChange={() => {}}
          emptyTitle="Backlog is empty"
          emptyDescription="Every work item is assigned to a sprint."
        />
      )}
    </Stack>
  );
}
