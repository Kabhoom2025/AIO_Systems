import DoneAllIcon from "@mui/icons-material/DoneAll";
import SettingsOutlinedIcon from "@mui/icons-material/SettingsOutlined";
import {
  Box,
  Button,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Menu,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { useNavigate } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
} from "../../hooks/useNotifications";

interface NotificationDropdownProps {
  anchorEl: HTMLElement | null;
  onClose: () => void;
}

export function NotificationDropdown({ anchorEl, onClose }: NotificationDropdownProps) {
  const navigate = useNavigate();
  const { data, isLoading } = useNotifications({ page: 1, pageSize: 20 });
  const markRead = useMarkNotificationRead();
  const markAllRead = useMarkAllNotificationsRead();

  const items = data?.items ?? [];

  return (
    <Menu
      anchorEl={anchorEl}
      open={!!anchorEl}
      onClose={onClose}
      anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
      transformOrigin={{ vertical: "top", horizontal: "right" }}
      slotProps={{ paper: { sx: { width: 380, maxHeight: 480 } } }}
    >
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, py: 1 }}>
        <Typography variant="subtitle1" fontWeight={700}>
          Notifications
        </Typography>
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Mark all as read">
            <IconButton size="small" onClick={() => markAllRead.mutate()}>
              <DoneAllIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Notification settings">
            <IconButton
              size="small"
              onClick={() => {
                onClose();
                navigate("/notification-preferences");
              }}
            >
              <SettingsOutlinedIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      </Stack>
      <Divider />

      {!isLoading && items.length === 0 && (
        <Box sx={{ width: 380 }}>
          <EmptyState title="You're all caught up" description="No notifications yet." />
        </Box>
      )}

      <List dense sx={{ overflowY: "auto", maxHeight: 360 }}>
        {items.map((n) => (
          <ListItemButton
            key={n.id}
            sx={{ bgcolor: n.isRead ? "transparent" : "action.hover", alignItems: "flex-start" }}
            onClick={() => {
              if (!n.isRead) markRead.mutate(n.id);
              onClose();
              if (n.linkUrl) navigate(n.linkUrl);
            }}
          >
            <ListItemText
              primary={n.title}
              secondary={
                <>
                  <Typography component="span" variant="body2" color="text.secondary" display="block">
                    {n.body}
                  </Typography>
                  <Typography component="span" variant="caption" color="text.disabled">
                    {formatDistanceToNow(new Date(n.createdAt), { addSuffix: true })}
                  </Typography>
                </>
              }
              slotProps={{ primary: { fontWeight: n.isRead ? 400 : 700 } }}
            />
          </ListItemButton>
        ))}
      </List>

      {items.length > 0 && (
        <>
          <Divider />
          <Box sx={{ p: 1, textAlign: "center" }}>
            <Button
              size="small"
              onClick={() => {
                onClose();
                navigate("/notifications");
              }}
            >
              View all
            </Button>
          </Box>
        </>
      )}
    </Menu>
  );
}
