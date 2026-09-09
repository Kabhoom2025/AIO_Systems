import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import GroupAddOutlinedIcon from "@mui/icons-material/GroupAddOutlined";
import {
  Alert,
  Autocomplete,
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
  TextField,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { EmptyState } from "../../components/EmptyState";
import { FormSelect } from "../../components/FormSelect";
import { FormTextField } from "../../components/FormTextField";
import { AppShell } from "../../components/AppShell";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { SkeletonCard } from "../../components/Skeletons";
import {
  useAddTeamMember,
  useCreateTeam,
  useRemoveTeamMember,
  useTeams,
  useUpdateTeam,
} from "../../hooks/useTeams";
import { useDepartments } from "../../hooks/useDepartments";
import { useUsers } from "../../hooks/useUsers";
import { useAuthStore } from "../../store/authStore";
import type { Team } from "../../types";

const teamSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional().or(z.literal("")),
  departmentId: z.string().optional().or(z.literal("")),
});
type TeamFormValues = z.infer<typeof teamSchema>;

export function TeamsPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: teams, isLoading } = useTeams(organizationId);
  const { data: departments } = useDepartments(organizationId);
  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 100 });

  const createMutation = useCreateTeam(organizationId);
  const updateMutation = useUpdateTeam(organizationId);
  const addMemberMutation = useAddTeamMember(organizationId);
  const removeMemberMutation = useRemoveTeamMember(organizationId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Team | null>(null);
  const [memberDialogTeam, setMemberDialogTeam] = useState<Team | null>(null);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

  const { control, handleSubmit, reset } = useForm<TeamFormValues>({
    resolver: zodResolver(teamSchema),
    defaultValues: { name: "", description: "", departmentId: "" },
  });

  const departmentOptions = useMemo(
    () => [
      { value: "", label: "No department" },
      ...(departments ?? []).map((d) => ({ value: d.id, label: d.name })),
    ],
    [departments]
  );

  const userOptions = useMemo(() => usersPage?.items ?? [], [usersPage]);

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", description: "", departmentId: "" });
    setDialogOpen(true);
  };

  const openEdit = (team: Team) => {
    setEditing(team);
    reset({
      name: team.name,
      description: team.description ?? "",
      departmentId: team.departmentId ?? "",
    });
    setDialogOpen(true);
  };

  const onSubmit = (values: TeamFormValues) => {
    const payload = {
      name: values.name,
      description: values.description || undefined,
      departmentId: values.departmentId || null,
    };
    if (editing) {
      updateMutation.mutate({ id: editing.id, payload }, { onSuccess: () => setDialogOpen(false) });
    } else {
      createMutation.mutate({ organizationId, ...payload }, { onSuccess: () => setDialogOpen(false) });
    }
  };

  const handleAddMember = () => {
    if (memberDialogTeam && selectedUserId) {
      addMemberMutation.mutate(
        { teamId: memberDialogTeam.id, payload: { userId: selectedUserId, roleInTeam: "member" } },
        { onSuccess: () => setSelectedUserId(null) }
      );
    }
  };

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Teams
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
            New team
          </Button>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={3} />
        ) : !teams || teams.length === 0 ? (
          <EmptyState
            title="No teams yet"
            description="Create a team to start assigning members."
            actionLabel="New team"
            onAction={openCreate}
          />
        ) : (
          <Stack spacing={2}>
            {teams.map((team) => (
              <Card key={team.id} variant="outlined">
                <CardContent>
                  <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                    <Box>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {team.name}
                      </Typography>
                      {team.description && (
                        <Typography variant="body2" color="text.secondary">
                          {team.description}
                        </Typography>
                      )}
                    </Box>
                    <Stack direction="row" spacing={1}>
                      <Button size="small" onClick={() => openEdit(team)}>
                        Edit
                      </Button>
                      <Button
                        size="small"
                        startIcon={<GroupAddOutlinedIcon fontSize="small" />}
                        onClick={() => setMemberDialogTeam(team)}
                      >
                        Members
                      </Button>
                    </Stack>
                  </Stack>
                  <Stack direction="row" spacing={1} sx={{ mt: 1.5, flexWrap: "wrap", gap: 1 }}>
                    {(team.members ?? []).length === 0 ? (
                      <Typography variant="caption" color="text.secondary">
                        No members yet
                      </Typography>
                    ) : (
                      team.members?.map((m) => (
                        <Chip key={m.userId} label={m.roleInTeam} size="small" variant="outlined" />
                      ))
                    )}
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
        )}
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? "Edit team" : "New team"}</DialogTitle>
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
              <FormSelect name="departmentId" control={control} label="Department" options={departmentOptions} />
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

      <Dialog
        open={!!memberDialogTeam}
        onClose={() => setMemberDialogTeam(null)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Manage members — {memberDialogTeam?.name}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 0.5 }}>
            <Stack direction="row" spacing={1}>
              <Autocomplete
                sx={{ flex: 1 }}
                options={userOptions}
                getOptionLabel={(u) => `${u.firstName} ${u.lastName} (${u.email})`}
                onChange={(_, value) => setSelectedUserId(value?.id ?? null)}
                renderInput={(params) => <TextField {...params} label="Add member" size="small" />}
              />
              <Button
                variant="contained"
                disabled={!selectedUserId || addMemberMutation.isPending}
                onClick={handleAddMember}
              >
                Add
              </Button>
            </Stack>
            <Stack spacing={1}>
              {(memberDialogTeam?.members ?? []).map((m) => (
                <Stack
                  key={m.userId}
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                  sx={{ p: 1, border: 1, borderColor: "divider", borderRadius: 1 }}
                >
                  <Typography variant="body2">
                    {userOptions.find((u) => u.id === m.userId)?.email ?? m.userId} — {m.roleInTeam}
                  </Typography>
                  <IconButton
                    size="small"
                    aria-label="Remove member"
                    onClick={() =>
                      memberDialogTeam &&
                      removeMemberMutation.mutate({ teamId: memberDialogTeam.id, userId: m.userId })
                    }
                  >
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </Stack>
              ))}
              {(memberDialogTeam?.members ?? []).length === 0 && (
                <Typography variant="body2" color="text.secondary">
                  No members added yet.
                </Typography>
              )}
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMemberDialogTeam(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </AppShell>
  );
}
