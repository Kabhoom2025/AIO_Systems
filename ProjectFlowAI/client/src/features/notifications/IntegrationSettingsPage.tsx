import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Paper, Stack, Tab, Tabs, Typography } from "@mui/material";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import { AppShell } from "../../components/AppShell";
import { FormTextField } from "../../components/FormTextField";
import { SkeletonCard } from "../../components/Skeletons";
import { useAuthStore } from "../../store/authStore";
import { useIntegrationSettings, useUpdateIntegrationSettings } from "../../hooks/useIntegrationSettings";

const schema = z.object({
  slackWebhookUrl: z.string().optional().or(z.literal("")),
  teamsWebhookUrl: z.string().optional().or(z.literal("")),
  smsProviderUrl: z.string().optional().or(z.literal("")),
  smsProviderApiKey: z.string().optional().or(z.literal("")),
});
type FormValues = z.infer<typeof schema>;

export function IntegrationSettingsPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: settings, isLoading } = useIntegrationSettings(organizationId);
  const updateMutation = useUpdateIntegrationSettings(organizationId);

  const { control, handleSubmit, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      slackWebhookUrl: "",
      teamsWebhookUrl: "",
      smsProviderUrl: "",
      smsProviderApiKey: "",
    },
  });

  useEffect(() => {
    if (settings) {
      reset({
        slackWebhookUrl: settings.slackWebhookUrl ?? "",
        teamsWebhookUrl: settings.teamsWebhookUrl ?? "",
        smsProviderUrl: settings.smsProviderUrl ?? "",
        smsProviderApiKey: "",
      });
    }
  }, [settings, reset]);

  const onSubmit = (values: FormValues) => {
    updateMutation.mutate({
      slackWebhookUrl: values.slackWebhookUrl || undefined,
      teamsWebhookUrl: values.teamsWebhookUrl || undefined,
      smsProviderUrl: values.smsProviderUrl || undefined,
      smsProviderApiKey: values.smsProviderApiKey || undefined,
    });
  };

  return (
    <AppShell>
      <Stack spacing={3} sx={{ maxWidth: 640 }}>
        <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
          <Tabs value="integrations" onChange={(_, value) => value === "general" && navigate("/organization")}>
            <Tab label="General" value="general" />
            <Tab label="Integrations" value="integrations" />
          </Tabs>
        </Box>
        <Stack spacing={0.5}>
          <Typography variant="h5" fontWeight={700}>
            Integration settings
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Connect Slack, Microsoft Teams, and SMS so organization members can receive
            notifications outside of ProjectFlow AI.
          </Typography>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={1} />
        ) : (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <Stack spacing={2}>
                {updateMutation.isSuccess && (
                  <Alert severity="success">Integration settings saved.</Alert>
                )}
                {updateMutation.error && (
                  <Alert severity="error">{(updateMutation.error as Error).message}</Alert>
                )}
                <FormTextField
                  name="slackWebhookUrl"
                  control={control}
                  label="Slack webhook URL"
                />
                <FormTextField
                  name="teamsWebhookUrl"
                  control={control}
                  label="Microsoft Teams webhook URL"
                />
                <FormTextField
                  name="smsProviderUrl"
                  control={control}
                  label="SMS provider URL"
                />
                <FormTextField
                  name="smsProviderApiKey"
                  control={control}
                  label="SMS provider API key"
                  type="password"
                  textFieldProps={{
                    placeholder: "Leave blank to keep the existing key",
                  }}
                />
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
