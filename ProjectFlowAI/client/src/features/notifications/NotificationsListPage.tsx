import { Button, FormControlLabel, List, ListItemButton, ListItemText, Pagination, Stack, Switch, Typography } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
} from "../../hooks/useNotifications";

const PAGE_SIZE = 20;

export function NotificationsListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [unreadOnly, setUnreadOnly] = useState(false);
  const { data, isLoading } = useNotifications({ page, pageSize: PAGE_SIZE, unreadOnly });
  const markRead = useMarkNotificationRead();
  const markAllRead = useMarkAllNotificationsRead();

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE)) : 1;

  return (
    <AppShell>
      <Stack spacing={2}>
        <Stack direction="row" alignItems="center" justifyContent="space-between">
          <Typography variant="h5" fontWeight={700}>
            Notifications
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center">
            <FormControlLabel
              control={
                <Switch
                  checked={unreadOnly}
                  onChange={(e) => {
                    setUnreadOnly(e.target.checked);
                    setPage(1);
                  }}
                />
              }
              label="Unread only"
            />
            <Button size="small" onClick={() => markAllRead.mutate()}>
              Mark all as read
            </Button>
          </Stack>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={5} />
        ) : (data?.items.length ?? 0) === 0 ? (
          <EmptyState title="No notifications" description="You're all caught up." />
        ) : (
          <List>
            {data!.items.map((n) => (
              <ListItemButton
                key={n.id}
                sx={{ bgcolor: n.isRead ? "transparent" : "action.hover", borderRadius: 1, mb: 0.5 }}
                onClick={() => {
                  if (!n.isRead) markRead.mutate(n.id);
                  if (n.linkUrl) navigate(n.linkUrl);
                }}
              >
                <ListItemText
                  primary={n.title}
                  secondary={`${n.body} · ${new Date(n.createdAt).toLocaleString()}`}
                  slotProps={{ primary: { fontWeight: n.isRead ? 400 : 700 } }}
                />
              </ListItemButton>
            ))}
          </List>
        )}

        {totalPages > 1 && (
          <Stack alignItems="center">
            <Pagination
              count={totalPages}
              page={page}
              onChange={(_, value) => setPage(value)}
              color="primary"
            />
          </Stack>
        )}
      </Stack>
    </AppShell>
  );
}
