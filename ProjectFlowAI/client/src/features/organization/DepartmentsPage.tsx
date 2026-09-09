import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { EmptyState } from "../../components/EmptyState";
import { FormSelect } from "../../components/FormSelect";
import { FormTextField } from "../../components/FormTextField";
import { AppShell } from "../../components/AppShell";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { SkeletonCard } from "../../components/Skeletons";
import {
  useCreateDepartment,
  useDeleteDepartment,
  useDepartments,
  useUpdateDepartment,
} from "../../hooks/useDepartments";
import { useAuthStore } from "../../store/authStore";
import type { Department } from "../../types";

const departmentSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional().or(z.literal("")),
  parentDepartmentId: z.string().optional().or(z.literal("")),
});
type DepartmentFormValues = z.infer<typeof departmentSchema>;

export function DepartmentsPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: departments, isLoading } = useDepartments(organizationId);
  const createMutation = useCreateDepartment(organizationId);
  const updateMutation = useUpdateDepartment(organizationId);
  const deleteMutation = useDeleteDepartment(organizationId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Department | null>(null);

  const { control, handleSubmit, reset } = useForm<DepartmentFormValues>({
    resolver: zodResolver(departmentSchema),
    defaultValues: { name: "", description: "", parentDepartmentId: "" },
  });

  const parentOptions = useMemo(
    () => [
      { value: "", label: "None (top-level)" },
      ...(departments ?? [])
        .filter((d) => d.id !== editing?.id)
        .map((d) => ({ value: d.id, label: d.name })),
    ],
    [departments, editing]
  );

  const groups = useMemo(() => {
    const list = departments ?? [];
    const topLevel = list.filter((d) => !d.parentDepartmentId);
    const childrenOf = (id: string) => list.filter((d) => d.parentDepartmentId === id);
    return { topLevel, childrenOf };
  }, [departments]);

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", description: "", parentDepartmentId: "" });
    setDialogOpen(true);
  };

  const openEdit = (dept: Department) => {
    setEditing(dept);
    reset({
      name: dept.name,
      description: dept.description ?? "",
      parentDepartmentId: dept.parentDepartmentId ?? "",
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: DepartmentFormValues) => {
    const payload = {
      name: values.name,
      description: values.description || undefined,
      parentDepartmentId: values.parentDepartmentId || null,
    };
    if (editing) {
      updateMutation.mutate(
        { id: editing.id, payload },
        { onSuccess: () => setDialogOpen(false) }
      );
    } else {
      createMutation.mutate(
        { organizationId, ...payload },
        { onSuccess: () => setDialogOpen(false) }
      );
    }
  };

  const renderCard = (dept: Department) => (
    <Card key={dept.id} variant="outlined">
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box>
            <Typography variant="subtitle1" fontWeight={600}>
              {dept.name}
            </Typography>
            {dept.description && (
              <Typography variant="body2" color="text.secondary">
                {dept.description}
              </Typography>
            )}
          </Box>
          <Stack direction="row" spacing={0.5}>
            <IconButton size="small" aria-label={`Edit ${dept.name}`} onClick={() => openEdit(dept)}>
              <EditOutlinedIcon fontSize="small" />
            </IconButton>
            <IconButton
              size="small"
              aria-label={`Delete ${dept.name}`}
              onClick={() => setConfirmDelete(dept)}
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          </Stack>
        </Stack>
        <Stack spacing={1} sx={{ mt: 1.5, pl: 2, borderLeft: 2, borderColor: "divider" }}>
          {groups.childrenOf(dept.id).map((child) => (
            <Stack
              key={child.id}
              direction="row"
              justifyContent="space-between"
              alignItems="center"
            >
              <Chip label={child.name} size="small" variant="outlined" />
              <Stack direction="row" spacing={0.5}>
                <IconButton size="small" onClick={() => openEdit(child)} aria-label={`Edit ${child.name}`}>
                  <EditOutlinedIcon fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  onClick={() => setConfirmDelete(child)}
                  aria-label={`Delete ${child.name}`}
                >
                  <DeleteOutlineIcon fontSize="small" />
                </IconButton>
              </Stack>
            </Stack>
          ))}
        </Stack>
      </CardContent>
    </Card>
  );

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Departments
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
            New department
          </Button>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={3} />
        ) : groups.topLevel.length === 0 ? (
          <EmptyState
            title="No departments yet"
            description="Create your first department to start organizing teams."
            actionLabel="New department"
            onAction={openCreate}
          />
        ) : (
          <Stack spacing={2}>{groups.topLevel.map(renderCard)}</Stack>
        )}
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? "Edit department" : "New department"}</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {(createMutation.error || updateMutation.error) && (
                <Alert severity="error">
                  {(createMutation.error ?? updateMutation.error)?.message}
                </Alert>
              )}
              <FormTextField name="name" control={control} label="Name" />
              <FormTextField name="description" control={control} label="Description" multiline rows={2} />
              <FormSelect
                name="parentDepartmentId"
                control={control}
                label="Parent department"
                options={parentOptions}
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
        open={!!confirmDelete}
        title="Delete department"
        message={`Are you sure you want to delete "${confirmDelete?.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        destructive
        loading={deleteMutation.isPending}
        onCancel={() => setConfirmDelete(null)}
        onConfirm={() => {
          if (confirmDelete) {
            deleteMutation.mutate(confirmDelete.id, { onSuccess: () => setConfirmDelete(null) });
          }
        }}
      />
    </AppShell>
  );
}
