import { Paper, Stack, Switch, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { SkeletonCard } from "../../components/Skeletons";
import { NOTIFICATION_CHANNELS, type NotificationChannel } from "../../types";
import {
  useNotificationPreferences,
  useUpdateNotificationPreference,
} from "../../hooks/useNotificationPreferences";

const CHANNEL_LABELS: Record<NotificationChannel, string> = {
  InApp: "In-app",
  Email: "Email",
  Slack: "Slack",
  Teams: "Microsoft Teams",
  Sms: "SMS",
};

const CHANNEL_DESCRIPTIONS: Record<NotificationChannel, string> = {
  InApp: "Show notifications in the bell dropdown and as toasts while you're using the app.",
  Email: "Send a copy of important notifications to your email address.",
  Slack: "Post notifications to your organization's connected Slack webhook.",
  Teams: "Post notifications to your organization's connected Microsoft Teams webhook.",
  Sms: "Send a text message for urgent notifications.",
};

export function NotificationPreferencesPage() {
  const { data: preferences, isLoading } = useNotificationPreferences();
  const updatePreference = useUpdateNotificationPreference();

  const isEnabled = (channel: NotificationChannel) =>
    preferences?.find((p) => p.channel === channel)?.isEnabled ?? true;

  return (
    <AppShell>
      <Stack spacing={3} sx={{ maxWidth: 640 }}>
        <Typography variant="h5" fontWeight={700}>
          Notification preferences
        </Typography>

        {isLoading ? (
          <SkeletonCard count={1} />
        ) : (
          <Paper variant="outlined">
            <Stack divider={<Stack sx={{ borderBottom: 1, borderColor: "divider" }} />}>
              {NOTIFICATION_CHANNELS.map((channel) => (
                <Stack
                  key={channel}
                  direction="row"
                  alignItems="center"
                  justifyContent="space-between"
                  sx={{ p: 2 }}
                >
                  <Stack sx={{ maxWidth: 460 }}>
                    <Typography variant="body1" fontWeight={600}>
                      {CHANNEL_LABELS[channel]}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {CHANNEL_DESCRIPTIONS[channel]}
                    </Typography>
                  </Stack>
                  <Switch
                    checked={isEnabled(channel)}
                    onChange={(e) =>
                      updatePreference.mutate({ channel, isEnabled: e.target.checked })
                    }
                  />
                </Stack>
              ))}
            </Stack>
          </Paper>
        )}
      </Stack>
    </AppShell>
  );
}
