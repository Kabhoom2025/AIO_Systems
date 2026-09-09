import AddIcon from "@mui/icons-material/Add";
import {
  Autocomplete,
  Avatar,
  Button,
  Chip,
  InputAdornment,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { useCreateWorkItem, useWorkItems } from "../../hooks/useWorkItems";
import { useUsers } from "../../hooks/useUsers";
import { useAuthStore } from "../../store/authStore";
import {
  WORK_ITEM_PRIORITIES,
  WORK_ITEM_STATUSES,
  WORK_ITEM_STATUS_LABELS,
  type WorkItem,
  type WorkItemPriority,
  type WorkItemStatus,
} from "../../types";
import { PRIORITY_COLOR } from "./kanbanMeta";

export function WorkItemListPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [, setSearchParams] = useSearchParams();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const [status, setStatus] = useState<WorkItemStatus | "">("");
  const [priority, setPriority] = useState<WorkItemPriority | "">("");
  const [assigneeId, setAssigneeId] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);

  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 200 });
  const orgUsers = usersPage?.items ?? [];

  const { data, isLoading } = useWorkItems({
    projectId: projectId ?? "",
    status: status || undefined,
    priority: priority || undefined,
    assigneeId: assigneeId || undefined,
    search: search || undefined,
    page: page + 1,
    pageSize,
  });

  const createMutation = useCreateWorkItem(projectId ?? "");

  const openTask = (id: string) => setSearchParams({ task: id });

  const columns: DataTableColumn<WorkItem>[] = [
    {
      key: "title",
      label: "Title",
      render: (row) => (
        <Typography
          variant="body2"
          fontWeight={600}
          sx={{ cursor: "pointer", "&:hover": { textDecoration: "underline" } }}
          onClick={() => openTask(row.id)}
        >
          {row.title}
        </Typography>
      ),
    },
    { key: "type", label: "Type", render: (row) => <Chip size="small" label={row.type} /> },
    {
      key: "status",
      label: "Status",
      render: (row) => <Chip size="small" label={WORK_ITEM_STATUS_LABELS[row.status]} />,
    },
    {
      key: "priority",
      label: "Priority",
      render: (row) => (
        <Chip size="small" label={row.priority} color={PRIORITY_COLOR[row.priority]} variant="outlined" />
      ),
    },
    {
      key: "assignee",
      label: "Assignee",
      render: (row) => {
        const u = orgUsers.find((usr) => usr.id === row.assigneeUserId);
        return u ? (
          <Stack direction="row" spacing={1} alignItems="center">
            <Avatar sx={{ width: 22, height: 22, fontSize: 11 }}>{u.firstName[0]}</Avatar>
            <Typography variant="body2">
              {u.firstName} {u.lastName}
            </Typography>
          </Stack>
        ) : (
          <Typography variant="body2" color="text.secondary">
            Unassigned
          </Typography>
        );
      },
    },
    {
      key: "dueDate",
      label: "Due date",
      render: (row) => (row.dueDate ? row.dueDate.slice(0, 10) : "—"),
    },
  ];

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={2}>
        <Stack direction="row" spacing={2} flexWrap="wrap" gap={2}>
          <TextField
            size="small"
            placeholder="Search tasks..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(0);
            }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
          />
          <Select
            size="small"
            displayEmpty
            value={status}
            onChange={(e) => {
              setStatus(e.target.value as WorkItemStatus | "");
              setPage(0);
            }}
            sx={{ minWidth: 140 }}
          >
            <MenuItem value="">All statuses</MenuItem>
            {WORK_ITEM_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {WORK_ITEM_STATUS_LABELS[s]}
              </MenuItem>
            ))}
          </Select>
          <Select
            size="small"
            displayEmpty
            value={priority}
            onChange={(e) => {
              setPriority(e.target.value as WorkItemPriority | "");
              setPage(0);
            }}
            sx={{ minWidth: 140 }}
          >
            <MenuItem value="">All priorities</MenuItem>
            {WORK_ITEM_PRIORITIES.map((p) => (
              <MenuItem key={p} value={p}>
                {p}
              </MenuItem>
            ))}
          </Select>
          <Autocomplete
            size="small"
            sx={{ minWidth: 200 }}
            options={orgUsers}
            getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
            value={orgUsers.find((u) => u.id === assigneeId) ?? null}
            onChange={(_, value) => {
              setAssigneeId(value?.id ?? null);
              setPage(0);
            }}
            renderInput={(params) => <TextField {...params} label="Assignee" size="small" />}
          />
        </Stack>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => {
            if (projectId) {
              createMutation.mutate(
                { projectId, title: "New task", status: "Backlog" },
                { onSuccess: (item) => openTask(item.id) }
              );
            }
          }}
        >
          New task
        </Button>
      </Stack>

      <DataTable
        columns={columns}
        rows={data?.items ?? []}
        getRowId={(row) => row.id}
        isLoading={isLoading}
        totalCount={data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setPage(0);
        }}
        emptyTitle="No tasks found"
        emptyDescription="Create a task or adjust your filters."
      />
    </Stack>
  );
}
