import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Switch,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useParams } from "react-router-dom";
import { z } from "zod";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { FormTextField } from "../../components/FormTextField";
import {
  useCreateMilestone,
  useDeleteMilestone,
  useMilestones,
  useUpdateMilestone,
} from "../../hooks/useMilestones";
import type { Milestone } from "../../types";

const milestoneSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional().or(z.literal("")),
  dueDate: z.string().optional().or(z.literal("")),
});
type MilestoneFormValues = z.infer<typeof milestoneSchema>;

export function MilestonesPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const { data: milestones, isLoading } = useMilestones(projectId);
  const createMutation = useCreateMilestone(projectId ?? "");
  const updateMutation = useUpdateMilestone(projectId ?? "");
  const deleteMutation = useDeleteMilestone(projectId ?? "");

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Milestone | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Milestone | null>(null);

  const { control, handleSubmit, reset } = useForm<MilestoneFormValues>({
    resolver: zodResolver(milestoneSchema),
    defaultValues: { name: "", description: "", dueDate: "" },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", description: "", dueDate: "" });
    setDialogOpen(true);
  };

  const openEdit = (m: Milestone) => {
    setEditing(m);
    reset({ name: m.name, description: m.description ?? "", dueDate: m.dueDate?.slice(0, 10) ?? "" });
    setDialogOpen(true);
  };

  const onSubmit = (values: MilestoneFormValues) => {
    if (!projectId) return;
    const payload = {
      name: values.name,
      description: values.description || undefined,
      dueDate: values.dueDate || null,
    };
    if (editing) {
      updateMutation.mutate({ id: editing.id, payload }, { onSuccess: () => setDialogOpen(false) });
    } else {
      createMutation.mutate({ projectId, ...payload }, { onSuccess: () => setDialogOpen(false) });
    }
  };

  const columns: DataTableColumn<Milestone>[] = [
    { key: "name", label: "Name", render: (row) => row.name },
    {
      key: "description",
      label: "Description",
      render: (row) => row.description || "—",
    },
    {
      key: "dueDate",
      label: "Due date",
      sortable: true,
      render: (row) => (row.dueDate ? row.dueDate.slice(0, 10) : "—"),
    },
    {
      key: "status",
      label: "Status",
      render: (row) => (
        <Stack direction="row" spacing={1} alignItems="center">
          <Chip
            size="small"
            label={row.status}
            color={row.status === "Completed" ? "success" : "default"}
          />
          <Switch
            size="small"
            checked={row.status === "Completed"}
            onChange={(e) =>
              updateMutation.mutate({
                id: row.id,
                payload: { status: e.target.checked ? "Completed" : "Open" },
              })
            }
          />
        </Stack>
      ),
    },
    {
      key: "actions",
      label: "",
      align: "right",
      render: (row) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <Button size="small" onClick={() => openEdit(row)}>
            Edit
          </Button>
          <IconButton size="small" onClick={() => setDeleteTarget(row)}>
            <DeleteOutlineIcon fontSize="small" />
          </IconButton>
        </Stack>
      ),
    },
  ];

  const sorted = [...(milestones ?? [])].sort((a, b) => {
    if (!a.dueDate) return 1;
    if (!b.dueDate) return -1;
    return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
  });

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h6" fontWeight={700}>
          Milestones
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
          New milestone
        </Button>
      </Stack>

      <DataTable
        columns={columns}
        rows={sorted}
        getRowId={(row) => row.id}
        isLoading={isLoading}
        totalCount={sorted.length}
        page={0}
        pageSize={sorted.length || 10}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        emptyTitle="No milestones yet"
        emptyDescription="Create a milestone to track key project dates."
      />

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? "Edit milestone" : "New milestone"}</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {(createMutation.error || updateMutation.error) && (
                <Alert severity="error">
                  {(createMutation.error ?? updateMutation.error)?.message}
                </Alert>
              )}
              <FormTextField name="name" control={control} label="Name" />
              <FormTextField
                name="description"
                control={control}
                label="Description"
                multiline
                rows={2}
              />
              <FormTextField
                name="dueDate"
                control={control}
                label="Due date"
                type="date"
                textFieldProps={{ slotProps: { inputLabel: { shrink: true } } }}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createMutation.isPending || updateMutation.isPending}
              startIcon={
                createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={16} />
                ) : undefined
              }
            >
              Save
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete milestone"
        message={`Delete "${deleteTarget?.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        destructive
        loading={deleteMutation.isPending}
        onConfirm={() => {
          if (deleteTarget) {
            deleteMutation.mutate(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) });
          }
        }}
        onCancel={() => setDeleteTarget(null)}
      />
    </Stack>
  );
}
