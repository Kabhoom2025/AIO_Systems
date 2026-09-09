import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { EmptyState } from "../../components/EmptyState";
import { FormTextField } from "../../components/FormTextField";
import { AppShell } from "../../components/AppShell";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { SkeletonCard } from "../../components/Skeletons";
import { useCreateRole, useRoles } from "../../hooks/useRoles";
import { usePermissions } from "../../hooks/usePermissions";
import { useAuthStore } from "../../store/authStore";

const roleSchema = z.object({
  name: z.string().min(1, "Name is required"),
  permissionKeys: z.array(z.string()).min(1, "Select at least one permission"),
});
type RoleFormValues = z.infer<typeof roleSchema>;

export function RolesPermissionsPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: roles, isLoading } = useRoles(organizationId);
  const { data: permissions, isLoading: permissionsLoading } = usePermissions();
  const createMutation = useCreateRole(organizationId);

  const [dialogOpen, setDialogOpen] = useState(false);

  const { control, handleSubmit, reset, watch } = useForm<RoleFormValues>({
    resolver: zodResolver(roleSchema),
    defaultValues: { name: "", permissionKeys: [] },
  });

  const selectedKeys = watch("permissionKeys");

  const groupedPermissions = useMemo(() => {
    const groups = new Map<string, typeof permissions>();
    (permissions ?? []).forEach((p) => {
      const list = groups.get(p.category) ?? [];
      list.push(p);
      groups.set(p.category, list as NonNullable<typeof permissions>);
    });
    return Array.from(groups.entries());
  }, [permissions]);

  const openCreate = () => {
    reset({ name: "", permissionKeys: [] });
    setDialogOpen(true);
  };

  const onSubmit = (values: RoleFormValues) => {
    createMutation.mutate(
      { organizationId, name: values.name, permissionKeys: values.permissionKeys },
      { onSuccess: () => setDialogOpen(false) }
    );
  };

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Roles &amp; Permissions
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
            New role
          </Button>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={3} />
        ) : !roles || roles.length === 0 ? (
          <EmptyState
            title="No custom roles yet"
            description="Create a role to grant a specific set of permissions to users."
            actionLabel="New role"
            onAction={openCreate}
          />
        ) : (
          <Stack spacing={2}>
            {roles.map((role) => (
              <Card key={role.id} variant="outlined">
                <CardContent>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="subtitle1" fontWeight={600}>
                      {role.name}
                    </Typography>
                    {role.isSystemRole && <Chip label="System" size="small" />}
                  </Stack>
                  <Stack direction="row" spacing={0.5} sx={{ mt: 1, flexWrap: "wrap", gap: 0.5 }}>
                    {role.permissions.map((k) => (
                      <Chip key={k} label={k} size="small" variant="outlined" />
                    ))}
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
        )}
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>New role</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {createMutation.error && <Alert severity="error">{createMutation.error.message}</Alert>}
              <FormTextField name="name" control={control} label="Role name" />

              <Typography variant="subtitle2" sx={{ mt: 1 }}>
                Permissions ({selectedKeys?.length ?? 0} selected)
              </Typography>

              {permissionsLoading ? (
                <SkeletonCard count={2} />
              ) : (
                <Controller
                  name="permissionKeys"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Stack spacing={2}>
                      {groupedPermissions.map(([category, perms]) => (
                        <Box key={category}>
                          <Typography variant="caption" color="text.secondary" sx={{ textTransform: "uppercase" }}>
                            {category}
                          </Typography>
                          <Stack sx={{ pl: 1 }}>
                            {(perms ?? []).map((p) => (
                              <FormControlLabel
                                key={p.key}
                                control={
                                  <Checkbox
                                    checked={field.value.includes(p.key)}
                                    onChange={(e) => {
                                      if (e.target.checked) {
                                        field.onChange([...field.value, p.key]);
                                      } else {
                                        field.onChange(field.value.filter((k: string) => k !== p.key));
                                      }
                                    }}
                                  />
                                }
                                label={p.description}
                              />
                            ))}
                          </Stack>
                        </Box>
                      ))}
                      {fieldState.error && (
                        <Typography variant="caption" color="error">
                          {fieldState.error.message}
                        </Typography>
                      )}
                    </Stack>
                  )}
                />
              )}
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
              Create role
            </Button>
          </DialogActions>
        </Box>
      </Dialog>
    </AppShell>
  );
}
