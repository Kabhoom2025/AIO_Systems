import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import NotificationsOutlinedIcon from "@mui/icons-material/NotificationsOutlined";
import SpeedOutlinedIcon from "@mui/icons-material/SpeedOutlined";
import {
  Chip,
  Divider,
  Grid,
  LinearProgress,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { useNavigate } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { StatTile } from "../../components/StatTile";
import { useDashboard } from "../../hooks/useDashboard";
import { useUnreadNotificationCount } from "../../hooks/useNotifications";
import { useAuthStore } from "../../store/authStore";
import type { DashboardTaskCard } from "../../types";
import { TaskCard } from "../projects/TaskCard";

function TaskList({
  tasks,
  emptyTitle,
  emptyDescription,
  onOpenTask,
}: {
  tasks: DashboardTaskCard[];
  emptyTitle: string;
  emptyDescription: string;
  onOpenTask: (task: DashboardTaskCard) => void;
}) {
  if (tasks.length === 0) {
    return <EmptyState title={emptyTitle} description={emptyDescription} />;
  }
  return (
    <Stack spacing={1}>
      {tasks.map((task) => (
        <TaskCard key={task.id} card={task} onClick={() => onOpenTask(task)} />
      ))}
    </Stack>
  );
}

export function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const { data, isLoading } = useDashboard();
  // Single source of truth for the unread badge: reuse the same hook the AppShell's
  // NotificationBell uses, rather than the dashboard payload's own count, so the two
  // never disagree.
  const { data: unreadData } = useUnreadNotificationCount();

  const openTask = (task: DashboardTaskCard) => {
    if (task.projectId) {
      navigate(`/projects/${task.projectId}/board?task=${task.id}`);
    } else {
      navigate("/projects");
    }
  };

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Welcome back{user ? `, ${user.firstName}` : ""}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Here's what needs your attention today.
          </Typography>
        </Stack>

        {isLoading ? (
          <SkeletonCard count={4} />
        ) : (
          <>
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <StatTile
                  label="Assigned to you"
                  value={data?.assignedTaskCount ?? 0}
                  icon={<AssignmentOutlinedIcon color="action" />}
                  color="text.primary"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <StatTile
                  label="Overdue"
                  value={data?.overdueTasks.length ?? 0}
                  icon={
                    <ErrorOutlineIcon
                      color={(data?.overdueTasks.length ?? 0) > 0 ? "error" : "disabled"}
                    />
                  }
                  color={(data?.overdueTasks.length ?? 0) > 0 ? "error" : "text.primary"}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <StatTile
                  label="Active sprints"
                  value={data?.activeSprints.length ?? 0}
                  icon={<SpeedOutlinedIcon color="action" />}
                  color="text.primary"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <StatTile
                  label="Unread notifications"
                  value={unreadData?.count ?? 0}
                  icon={<NotificationsOutlinedIcon color="action" />}
                  color="text.primary"
                />
              </Grid>
            </Grid>

            <Grid container spacing={2}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Paper variant="outlined" sx={{ p: 2, height: "100%" }}>
                  <Stack spacing={1.5}>
                    <Typography variant="subtitle1" fontWeight={700}>
                      Today's Tasks
                    </Typography>
                    <TaskList
                      tasks={data?.todaysTasks ?? []}
                      emptyTitle="No tasks due today"
                      emptyDescription="Enjoy the breathing room — nothing on your plate is due today."
                      onOpenTask={openTask}
                    />
                  </Stack>
                </Paper>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Paper
                  variant="outlined"
                  sx={{
                    p: 2,
                    height: "100%",
                    borderColor: (data?.overdueTasks.length ?? 0) > 0 ? "error.main" : "divider",
                  }}
                >
                  <Stack spacing={1.5}>
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography variant="subtitle1" fontWeight={700} color="error.main">
                        Overdue
                      </Typography>
                      {(data?.overdueTasks.length ?? 0) > 0 && (
                        <Chip size="small" color="error" label={data?.overdueTasks.length} />
                      )}
                    </Stack>
                    <TaskList
                      tasks={data?.overdueTasks ?? []}
                      emptyTitle="Nothing overdue"
                      emptyDescription="Everything assigned to you is on track."
                      onOpenTask={openTask}
                    />
                  </Stack>
                </Paper>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Paper variant="outlined" sx={{ p: 2, height: "100%" }}>
                  <Stack spacing={1.5}>
                    <Typography variant="subtitle1" fontWeight={700}>
                      Active Sprints
                    </Typography>
                    {(data?.activeSprints.length ?? 0) === 0 ? (
                      <EmptyState
                        title="No active sprints"
                        description="Sprints you're part of will show their progress here once started."
                      />
                    ) : (
                      <Stack spacing={2}>
                        {data?.activeSprints.map((sprint) => (
                          <Stack
                            key={sprint.sprintId}
                            spacing={0.75}
                            sx={{ cursor: "pointer" }}
                            onClick={() =>
                              navigate(`/projects/${sprint.projectId}/sprints/${sprint.sprintId}/board`)
                            }
                          >
                            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                              <Typography variant="body2" fontWeight={600}>
                                {sprint.sprintName}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {sprint.projectName}
                              </Typography>
                            </Stack>
                            <LinearProgress
                              variant="determinate"
                              value={Math.min(100, Math.max(0, sprint.progressPercent))}
                              sx={{ height: 6, borderRadius: 3 }}
                            />
                            <Typography variant="caption" color="text.secondary">
                              {Math.round(sprint.progressPercent)}% complete ·{" "}
                              {sprint.daysRemaining === 1
                                ? "1 day remaining"
                                : `${sprint.daysRemaining} days remaining`}
                            </Typography>
                          </Stack>
                        ))}
                      </Stack>
                    )}
                  </Stack>
                </Paper>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Paper variant="outlined" sx={{ p: 2, height: "100%" }}>
                  <Stack spacing={1.5}>
                    <Typography variant="subtitle1" fontWeight={700}>
                      Recent Activity
                    </Typography>
                    {(data?.recentActivity.length ?? 0) === 0 ? (
                      <EmptyState
                        title="No recent activity"
                        description="Activity across your projects will show up here."
                      />
                    ) : (
                      <List dense disablePadding>
                        {data?.recentActivity.map((item, i) => (
                          <div key={item.id}>
                            {i > 0 && <Divider component="li" />}
                            <ListItemButton
                              disableGutters
                              disabled={!item.linkUrl}
                              onClick={() => item.linkUrl && navigate(item.linkUrl)}
                              sx={{ borderRadius: 1 }}
                            >
                              <ListItemText
                                primary={item.message}
                                secondary={formatDistanceToNow(new Date(item.createdAt), {
                                  addSuffix: true,
                                })}
                              />
                            </ListItemButton>
                          </div>
                        ))}
                      </List>
                    )}
                  </Stack>
                </Paper>
              </Grid>
            </Grid>
          </>
        )}
      </Stack>
    </AppShell>
  );
}
