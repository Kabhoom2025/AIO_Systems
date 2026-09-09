import DarkModeOutlinedIcon from "@mui/icons-material/DarkModeOutlined";
import LightModeOutlinedIcon from "@mui/icons-material/LightModeOutlined";
import LogoutOutlinedIcon from "@mui/icons-material/LogoutOutlined";
import MenuIcon from "@mui/icons-material/Menu";
import ApartmentOutlinedIcon from "@mui/icons-material/ApartmentOutlined";
import AccountTreeOutlinedIcon from "@mui/icons-material/AccountTreeOutlined";
import GroupsOutlinedIcon from "@mui/icons-material/GroupsOutlined";
import PeopleAltOutlinedIcon from "@mui/icons-material/PeopleAltOutlined";
import MailOutlineIcon from "@mui/icons-material/MailOutline";
import SecurityOutlinedIcon from "@mui/icons-material/SecurityOutlined";
import HistoryOutlinedIcon from "@mui/icons-material/HistoryOutlined";
import DashboardOutlinedIcon from "@mui/icons-material/DashboardOutlined";
import ViewKanbanOutlinedIcon from "@mui/icons-material/ViewKanbanOutlined";
import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import AccessTimeOutlinedIcon from "@mui/icons-material/AccessTimeOutlined";
import ForumOutlinedIcon from "@mui/icons-material/ForumOutlined";
import MenuBookOutlinedIcon from "@mui/icons-material/MenuBookOutlined";
import AssessmentOutlinedIcon from "@mui/icons-material/AssessmentOutlined";
import FactCheckOutlinedIcon from "@mui/icons-material/FactCheckOutlined";
import {
  AppBar,
  Avatar,
  Badge,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Select,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { useMemo, useState, type ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { useAuthStore } from "../store/authStore";
import { useThemeStore } from "../store/themeStore";
import { useOrganizations } from "../hooks/useOrganizations";
import { useApprovals } from "../hooks/useApprovals";
import { NotificationBell } from "../features/notifications/NotificationBell";
import { TimerWidget } from "./TimerWidget";

const DRAWER_WIDTH = 260;

interface NavItem {
  label: string;
  path?: string;
  icon: ReactNode;
  disabled?: boolean;
  tooltip?: string;
}

const navItems: NavItem[] = [
  { label: "Dashboard", path: "/", icon: <DashboardOutlinedIcon /> },
  { label: "Organization", path: "/organization", icon: <ApartmentOutlinedIcon /> },
  { label: "Departments", path: "/departments", icon: <AccountTreeOutlinedIcon /> },
  { label: "Teams", path: "/teams", icon: <GroupsOutlinedIcon /> },
  { label: "Users", path: "/users", icon: <PeopleAltOutlinedIcon /> },
  { label: "Invitations", path: "/invitations", icon: <MailOutlineIcon /> },
  { label: "Roles & Permissions", path: "/roles", icon: <SecurityOutlinedIcon /> },
  { label: "Audit Logs", path: "/audit-logs", icon: <HistoryOutlinedIcon /> },
  { label: "Boards", path: "/projects", icon: <ViewKanbanOutlinedIcon /> },
  { label: "Reports", path: "/reports", icon: <AssessmentOutlinedIcon /> },
  { label: "Calendar", path: "/calendar", icon: <CalendarMonthOutlinedIcon /> },
  { label: "My Time", path: "/my-time", icon: <AccessTimeOutlinedIcon /> },
  { label: "Chat", path: "/chat", icon: <ForumOutlinedIcon /> },
  { label: "Wiki", path: "/wiki", icon: <MenuBookOutlinedIcon /> },
  { label: "Approvals", path: "/approvals", icon: <FactCheckOutlinedIcon /> },
];

interface AppShellProps {
  children: ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));
  const [mobileOpen, setMobileOpen] = useState(false);
  const [userMenuAnchor, setUserMenuAnchor] = useState<null | HTMLElement>(null);
  const navigate = useNavigate();
  const location = useLocation();

  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const mode = useThemeStore((s) => s.mode);
  const toggleMode = useThemeStore((s) => s.toggleMode);

  const { data: organizations } = useOrganizations();
  const { data: pendingApprovals } = useApprovals(true);
  const pendingApprovalCount = (pendingApprovals ?? []).filter((a) => a.status === "AwaitingApproval").length;
  const [selectedOrgId, setSelectedOrgId] = useState<string>(user?.organizationId ?? "");

  const currentOrgId = selectedOrgId || user?.organizationId || "";

  const initials = useMemo(() => {
    if (!user) return "?";
    return `${user.firstName?.[0] ?? ""}${user.lastName?.[0] ?? ""}`.toUpperCase();
  }, [user]);

  const handleLogout = () => {
    setUserMenuAnchor(null);
    logout();
    navigate("/login");
  };

  const drawerContent = (
    <Box sx={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <Toolbar sx={{ px: 2 }}>
        <Typography variant="h6" fontWeight={700} color="primary.main">
          ProjectFlow AI
        </Typography>
      </Toolbar>
      <Divider />
      <List sx={{ flex: 1, px: 1, py: 1 }}>
        {navItems.map((item) => {
          const selected = item.path
            ? item.path === "/"
              ? location.pathname === "/"
              : location.pathname.startsWith(item.path)
            : false;
          const button = (
            <ListItemButton
              disabled={item.disabled}
              selected={selected}
              onClick={() => {
                if (item.path) {
                  navigate(item.path);
                  if (isMobile) setMobileOpen(false);
                }
              }}
              sx={{
                borderRadius: 2,
                mb: 0.5,
                "&.Mui-selected": {
                  bgcolor: "primary.main",
                  color: "primary.contrastText",
                  "& .MuiListItemIcon-root": { color: "primary.contrastText" },
                  "&:hover": { bgcolor: "primary.dark" },
                },
              }}
            >
              <ListItemIcon>
                {item.label === "Approvals" ? (
                  <Badge badgeContent={pendingApprovalCount} color="error" max={99}>
                    {item.icon}
                  </Badge>
                ) : (
                  item.icon
                )}
              </ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          );
          return (
            <ListItem key={item.label} disablePadding>
              {item.disabled && item.tooltip ? (
                <Tooltip title={item.tooltip} placement="right">
                  <span style={{ width: "100%" }}>{button}</span>
                </Tooltip>
              ) : (
                button
              )}
            </ListItem>
          );
        })}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <AppBar
        position="fixed"
        elevation={0}
        color="default"
        sx={{
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
          bgcolor: "background.paper",
          borderBottom: 1,
          borderColor: "divider",
        }}
      >
        <Toolbar sx={{ display: "flex", justifyContent: "space-between", gap: 2 }}>
          <Stack direction="row" alignItems="center" spacing={1}>
            {isMobile && (
              <IconButton
                edge="start"
                aria-label="open navigation menu"
                onClick={() => setMobileOpen(true)}
              >
                <MenuIcon />
              </IconButton>
            )}
            {organizations && organizations.length > 1 && (
              <Select
                size="small"
                value={currentOrgId}
                onChange={(e) => setSelectedOrgId(e.target.value)}
                aria-label="Switch organization"
                sx={{ minWidth: 180 }}
              >
                {organizations.map((org) => (
                  <MenuItem key={org.id} value={org.id}>
                    {org.name}
                  </MenuItem>
                ))}
              </Select>
            )}
          </Stack>

          <Stack direction="row" alignItems="center" spacing={1.5}>
            <TimerWidget />
            <NotificationBell />
            <Tooltip title={mode === "light" ? "Switch to dark mode" : "Switch to light mode"}>
              <IconButton onClick={toggleMode} aria-label="Toggle color mode">
                {mode === "light" ? <DarkModeOutlinedIcon /> : <LightModeOutlinedIcon />}
              </IconButton>
            </Tooltip>
            <Tooltip title={user ? `${user.firstName} ${user.lastName}` : "Account"}>
              <IconButton
                onClick={(e) => setUserMenuAnchor(e.currentTarget)}
                aria-label="Open user menu"
              >
                <Avatar
                  src={user?.avatarUrl ?? undefined}
                  sx={{ width: 32, height: 32, bgcolor: "primary.main", fontSize: 14 }}
                >
                  {initials}
                </Avatar>
              </IconButton>
            </Tooltip>
            <Menu
              anchorEl={userMenuAnchor}
              open={!!userMenuAnchor}
              onClose={() => setUserMenuAnchor(null)}
            >
              <MenuItem disabled>{user?.email}</MenuItem>
              <Divider />
              <MenuItem onClick={handleLogout}>
                <ListItemIcon>
                  <LogoutOutlinedIcon fontSize="small" />
                </ListItemIcon>
                Log out
              </MenuItem>
            </Menu>
          </Stack>
        </Toolbar>
      </AppBar>

      <Drawer
        variant={isMobile ? "temporary" : "permanent"}
        open={isMobile ? mobileOpen : true}
        onClose={() => setMobileOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
            borderRight: 1,
            borderColor: "divider",
          },
        }}
      >
        {drawerContent}
      </Drawer>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          p: { xs: 2, md: 3 },
        }}
      >
        <Toolbar />
        <motion.div
          key={location.pathname}
          initial={{ opacity: 0, y: 6 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.2 }}
        >
          {children}
        </motion.div>
      </Box>
    </Box>
  );
}
