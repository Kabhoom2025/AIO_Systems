import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import ArchiveOutlinedIcon from "@mui/icons-material/ArchiveOutlined";
import SearchIcon from "@mui/icons-material/Search";
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  InputAdornment,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import { AppShell } from "../../components/AppShell";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { EmptyState } from "../../components/EmptyState";
import { FormTextField } from "../../components/FormTextField";
import { SkeletonCard } from "../../components/Skeletons";
import { useArchiveProject, useCreateProject, useProjects } from "../../hooks/useProjects";
import { useUsers } from "../../hooks/useUsers";
import { useAuthStore } from "../../store/authStore";
import type { Project, ProjectStatus } from "../../types";

const projectSchema = z.object({
  key: z
    .string()
    .min(1, "Key is required")
    .max(10, "Key must be 10 characters or fewer")
    .regex(/^[A-Za-z0-9]+$/, "Key must be alphanumeric"),
  name: z.string().min(1, "Name is required"),
  description: z.string().optional().or(z.literal("")),
  startDate: z.string().optional().or(z.literal("")),
  endDate: z.string().optional().or(z.literal("")),
});
type ProjectFormValues = z.infer<typeof projectSchema>;

const STATUS_FILTERS: { value: ProjectStatus | "All"; label: string }[] = [
  { value: "All", label: "All" },
  { value: "Active", label: "Active" },
  { value: "OnHold", label: "On Hold" },
  { value: "Completed", label: "Completed" },
  { value: "Archived", label: "Archived" },
];

const statusColor: Record<ProjectStatus, "success" | "warning" | "info" | "default"> = {
  Active: "success",
  OnHold: "warning",
  Completed: "info",
  Archived: "default",
};

export function ProjectsListPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const [status, setStatus] = useState<ProjectStatus | "All">("All");
  const [search, setSearch] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [ownerId, setOwnerId] = useState<string | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<Project | null>(null);

  const { data, isLoading } = useProjects({
    organizationId,
    status: status === "All" ? undefined : status,
    search: search || undefined,
    page: 1,
    pageSize: 50,
  });
  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 100 });
  const createMutation = useCreateProject(organizationId);
  const archiveMutation = useArchiveProject();

  const { control, handleSubmit, reset } = useForm<ProjectFormValues>({
    resolver: zodResolver(projectSchema),
    defaultValues: { key: "", name: "", description: "", startDate: "", endDate: "" },
  });

  const userOptions = useMemo(() => usersPage?.items ?? [], [usersPage]);

  const openCreate = () => {
    reset({ key: "", name: "", description: "", startDate: "", endDate: "" });
    setOwnerId(null);
    setDialogOpen(true);
  };

  const onSubmit = (values: ProjectFormValues) => {
    createMutation.mutate(
      {
        organizationId,
        key: values.key.toUpperCase(),
        name: values.name,
        description: values.description || undefined,
        startDate: values.startDate || null,
        endDate: values.endDate || null,
        ownerUserId: ownerId,
      },
      { onSuccess: () => setDialogOpen(false) }
    );
  };

  const projects = data?.items ?? [];

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Projects
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
            New project
          </Button>
        </Stack>

        <Stack direction={{ xs: "column", sm: "row" }} spacing={2} alignItems={{ sm: "center" }}>
          <TextField
            size="small"
            placeholder="Search projects..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ minWidth: 260 }}
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
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
            {STATUS_FILTERS.map((f) => (
              <Chip
                key={f.value}
                label={f.label}
                color={status === f.value ? "primary" : "default"}
                variant={status === f.value ? "filled" : "outlined"}
                onClick={() => setStatus(f.value)}
              />
            ))}
          </Stack>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={4} />
        ) : projects.length === 0 ? (
          <EmptyState
            title="No projects yet"
            description="Create your first project to start planning work."
            actionLabel="New project"
            onAction={openCreate}
          />
        ) : (
          <Grid container spacing={2}>
            {projects.map((project) => (
              <Grid key={project.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card variant="outlined" sx={{ height: "100%" }}>
                  <CardActionArea
                    onClick={() => navigate(`/projects/${project.id}/board`)}
                    sx={{ height: "100%" }}
                  >
                    <CardContent>
                      <Stack
                        direction="row"
                        justifyContent="space-between"
                        alignItems="flex-start"
                        spacing={1}
                      >
                        <Box>
                          <Typography variant="caption" color="text.secondary">
                            {project.key}
                          </Typography>
                          <Typography variant="subtitle1" fontWeight={600}>
                            {project.name}
                          </Typography>
                        </Box>
                        <Chip
                          size="small"
                          label={project.status}
                          color={statusColor[project.status]}
                        />
                      </Stack>
                      {project.description && (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{
                            mt: 1,
                            display: "-webkit-box",
                            WebkitLineClamp: 2,
                            WebkitBoxOrient: "vertical",
                            overflow: "hidden",
                          }}
                        >
                          {project.description}
                        </Typography>
                      )}
                      <Stack direction="row" justifyContent="flex-end" sx={{ mt: 1.5 }}>
                        {!project.isArchived && (
                          <Button
                            size="small"
                            color="inherit"
                            startIcon={<ArchiveOutlinedIcon fontSize="small" />}
                            onClick={(e) => {
                              e.stopPropagation();
                              setArchiveTarget(project);
                            }}
                          >
                            Archive
                          </Button>
                        )}
                      </Stack>
                    </CardContent>
                  </CardActionArea>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>New project</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {createMutation.error && (
                <Alert severity="error">{createMutation.error.message}</Alert>
              )}
              <Stack direction="row" spacing={2}>
                <FormTextField
                  name="key"
                  control={control}
                  label="Key"
                  textFieldProps={{ sx: { width: 160 } }}
                />
                <FormTextField name="name" control={control} label="Name" />
              </Stack>
              <FormTextField
                name="description"
                control={control}
                label="Description"
                multiline
                rows={2}
              />
              <Stack direction="row" spacing={2}>
                <FormTextField
                  name="startDate"
                  control={control}
                  label="Start date"
                  type="date"
                  textFieldProps={{ slotProps: { inputLabel: { shrink: true } } }}
                />
                <FormTextField
                  name="endDate"
                  control={control}
                  label="End date"
                  type="date"
                  textFieldProps={{ slotProps: { inputLabel: { shrink: true } } }}
                />
              </Stack>
              <Autocomplete
                options={userOptions}
                getOptionLabel={(u) => `${u.firstName} ${u.lastName} (${u.email})`}
                onChange={(_, value) => setOwnerId(value?.id ?? null)}
                renderInput={(params) => <TextField {...params} label="Owner" size="small" />}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createMutation.isPending}
              startIcon={createMutation.isPending ? <CircularProgress size={16} /> : undefined}
            >
              Create
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <ConfirmDialog
        open={!!archiveTarget}
        title="Archive project"
        message={`Are you sure you want to archive "${archiveTarget?.name}"? It will be hidden from active project lists.`}
        confirmLabel="Archive"
        destructive
        loading={archiveMutation.isPending}
        onConfirm={() => {
          if (archiveTarget) {
            archiveMutation.mutate(archiveTarget.id, { onSuccess: () => setArchiveTarget(null) });
          }
        }}
        onCancel={() => setArchiveTarget(null)}
      />
    </AppShell>
  );
}
