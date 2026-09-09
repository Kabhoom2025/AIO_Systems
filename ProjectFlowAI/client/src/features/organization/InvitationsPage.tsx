import { zodResolver } from "@hookform/resolvers/zod";
import AddIcon from "@mui/icons-material/Add";
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
  Stack,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { AppShell } from "../../components/AppShell";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { FormSelect } from "../../components/FormSelect";
import { FormTextField } from "../../components/FormTextField";
import { useCreateInvitation, useInvitations, useRevokeInvitation } from "../../hooks/useInvitations";
import { useRoles } from "../../hooks/useRoles";
import { useAuthStore } from "../../store/authStore";
import type { Invitation } from "../../types";

const inviteSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  roleId: z.string().min(1, "Role is required"),
});
type InviteFormValues = z.infer<typeof inviteSchema>;

export function InvitationsPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: invitations, isLoading } = useInvitations(organizationId);
  const { data: roles } = useRoles(organizationId);
  const createMutation = useCreateInvitation(organizationId);
  const revokeMutation = useRevokeInvitation(organizationId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [revoking, setRevoking] = useState<Invitation | null>(null);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const { control, handleSubmit, reset } = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: { email: "", roleId: "" },
  });

  const roleOptions = useMemo(
    () => (roles ?? []).map((r) => ({ value: r.id, label: r.name })),
    [roles]
  );

  const openInvite = () => {
    reset({ email: "", roleId: "" });
    setDialogOpen(true);
  };

  const onSubmit = (values: InviteFormValues) => {
    createMutation.mutate(
      { organizationId, email: values.email, roleId: values.roleId },
      { onSuccess: () => setDialogOpen(false) }
    );
  };

  const rows = invitations ?? [];
  const paged = rows.slice(page * pageSize, page * pageSize + pageSize);

  const columns: DataTableColumn<Invitation>[] = [
    { key: "email", label: "Email", render: (i) => i.email },
    {
      key: "roleId",
      label: "Role",
      render: (i) => roleOptions.find((r) => r.value === i.roleId)?.label ?? i.roleId,
    },
    {
      key: "status",
      label: "Status",
      render: (i) => (
        <Chip label={i.status ?? "pending"} size="small" variant="outlined" color="info" />
      ),
    },
    {
      key: "actions",
      label: "",
      align: "right",
      render: (i) => (
        <Button size="small" color="error" onClick={() => setRevoking(i)}>
          Revoke
        </Button>
      ),
    },
  ];

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Invitations
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={openInvite}>
            Invite user
          </Button>
        </Stack>

        <DataTable
          columns={columns}
          rows={paged}
          getRowId={(i) => i.id}
          isLoading={isLoading}
          totalCount={rows.length}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(0);
          }}
          emptyTitle="No pending invitations"
          emptyDescription="Invite teammates to join your organization."
        />
      </Stack>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Invite user</DialogTitle>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 0.5 }}>
              {createMutation.error && <Alert severity="error">{createMutation.error.message}</Alert>}
              <FormTextField name="email" control={control} label="Email" type="email" />
              <FormSelect name="roleId" control={control} label="Role" options={roleOptions} />
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
              Send invite
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <ConfirmDialog
        open={!!revoking}
        title="Revoke invitation"
        message={`Revoke the invitation sent to "${revoking?.email}"?`}
        confirmLabel="Revoke"
        destructive
        loading={revokeMutation.isPending}
        onCancel={() => setRevoking(null)}
        onConfirm={() => {
          if (revoking) {
            revokeMutation.mutate(revoking.id, { onSuccess: () => setRevoking(null) });
          }
        }}
      />
    </AppShell>
  );
}
