import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Paper, Stack, Tab, Tabs, Typography } from "@mui/material";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import { AppShell } from "../../components/AppShell";
import { SkeletonCard } from "../../components/Skeletons";
import { FormTextField } from "../../components/FormTextField";
import { useAuthStore } from "../../store/authStore";
import { useOrganization, useUpdateOrganization } from "../../hooks/useOrganizations";

const orgSchema = z.object({
  name: z.string().min(1, "Name is required"),
  slug: z
    .string()
    .min(1, "Slug is required")
    .regex(/^[a-z0-9-]+$/, "Slug may only contain lowercase letters, numbers, and hyphens"),
  domain: z.string().optional().or(z.literal("")),
});
type OrgFormValues = z.infer<typeof orgSchema>;

export function OrganizationSettingsPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId;

  const { data: organization, isLoading } = useOrganization(organizationId);
  const updateMutation = useUpdateOrganization(organizationId ?? "");

  const { control, handleSubmit, reset } = useForm<OrgFormValues>({
    resolver: zodResolver(orgSchema),
    defaultValues: { name: "", slug: "", domain: "" },
  });

  useEffect(() => {
    if (organization) {
      reset({
        name: organization.name,
        slug: organization.slug,
        domain: organization.domain ?? "",
      });
    }
  }, [organization, reset]);

  const onSubmit = (values: OrgFormValues) => {
    updateMutation.mutate({
      name: values.name,
      slug: values.slug,
      domain: values.domain || undefined,
    });
  };

  return (
    <AppShell>
      <Stack spacing={3} sx={{ maxWidth: 560 }}>
        <Typography variant="h5" fontWeight={700}>
          Organization settings
        </Typography>

        <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
          <Tabs value="general" onChange={(_, value) => value === "integrations" && navigate("/integration-settings")}>
            <Tab label="General" value="general" />
            <Tab label="Integrations" value="integrations" />
          </Tabs>
        </Box>

        {isLoading ? (
          <SkeletonCard count={1} />
        ) : (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <Stack spacing={2}>
                {updateMutation.isSuccess && (
                  <Alert severity="success">Organization updated successfully.</Alert>
                )}
                {updateMutation.error && (
                  <Alert severity="error">{updateMutation.error.message}</Alert>
                )}
                <FormTextField name="name" control={control} label="Organization name" />
                <FormTextField name="slug" control={control} label="Slug" />
                <FormTextField name="domain" control={control} label="Domain (optional)" />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Subscription plan: {organization?.subscriptionPlan ?? "-"}
                  </Typography>
                </Box>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={updateMutation.isPending}
                  startIcon={updateMutation.isPending ? <CircularProgress size={18} /> : undefined}
                  sx={{ alignSelf: "flex-start" }}
                >
                  Save changes
                </Button>
              </Stack>
            </Box>
          </Paper>
        )}
      </Stack>
    </AppShell>
  );
}
