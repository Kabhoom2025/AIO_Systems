import NotificationsOutlinedIcon from "@mui/icons-material/NotificationsOutlined";
import { Badge, IconButton, Tooltip } from "@mui/material";
import { useState } from "react";
import { useUnreadNotificationCount } from "../../hooks/useNotifications";
import { NotificationDropdown } from "./NotificationDropdown";

export function NotificationBell() {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const { data } = useUnreadNotificationCount();
  const count = data?.count ?? 0;

  return (
    <>
      <Tooltip title="Notifications">
        <IconButton
          aria-label="Open notifications"
          onClick={(e) => setAnchorEl(e.currentTarget)}
        >
          <Badge badgeContent={count} color="error" max={99}>
            <NotificationsOutlinedIcon />
          </Badge>
        </IconButton>
      </Tooltip>
      <NotificationDropdown anchorEl={anchorEl} onClose={() => setAnchorEl(null)} />
    </>
  );
}
