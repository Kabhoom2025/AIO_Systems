import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { Box, Chip, IconButton, Skeleton, Stack, Tab, Tabs, Typography } from "@mui/material";
import { Outlet, useLocation, useNavigate, useParams } from "react-router-dom";
import { AppShell } from "../../components/AppShell";
import { useProject } from "../../hooks/useProject";
import { WorkItemDetailDrawer } from "./WorkItemDetailDrawer";

const TABS = [
  { label: "Board", segment: "board" },
  { label: "List", segment: "list" },
  { label: "Sprints", segment: "sprints" },
  { label: "Gantt", segment: "gantt" },
  { label: "Calendar", segment: "calendar" },
  { label: "Milestones", segment: "milestones" },
  { label: "Documents", segment: "documents" },
  { label: "Reports", segment: "reports" },
  { label: "AI Tools", segment: "ai" },
  { label: "Automation", segment: "automation" },
  { label: "Settings", segment: "settings" },
];

export function ProjectWorkspaceLayout() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { data: project, isLoading } = useProject(projectId);

  // Match against the path segment immediately after the projectId (not a substring search),
  // so a nested route like /projects/:id/sprints/:sprintId/board doesn't get misread as the
  // top-level "Board" tab just because it also contains "/board".
  const prefix = `/projects/${projectId}/`;
  const firstSegment = location.pathname.startsWith(prefix)
    ? location.pathname.slice(prefix.length).split("/")[0]
    : "";
  const activeSegment = TABS.find((t) => t.segment === firstSegment)?.segment ?? "board";

  return (
    <AppShell>
      <Stack spacing={2}>
        <Stack direction="row" alignItems="center" spacing={1.5}>
          <IconButton aria-label="Back to projects" onClick={() => navigate("/projects")}>
            <ArrowBackIcon />
          </IconButton>
          {isLoading || !project ? (
            <Skeleton variant="text" width={220} height={36} />
          ) : (
            <Stack direction="row" spacing={1.5} alignItems="center">
              <Typography variant="h5" fontWeight={700}>
                {project.name}
              </Typography>
              <Chip size="small" label={project.key} variant="outlined" />
              <Chip size="small" label={project.status} color="primary" variant="outlined" />
            </Stack>
          )}
        </Stack>

        <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
          <Tabs
            value={activeSegment}
            onChange={(_, value) => navigate(`/projects/${projectId}/${value}`)}
          >
            {TABS.map((t) => (
              <Tab key={t.segment} label={t.label} value={t.segment} />
            ))}
          </Tabs>
        </Box>

        <Outlet />
      </Stack>
      {projectId && <WorkItemDetailDrawer projectId={projectId} />}
    </AppShell>
  );
}
