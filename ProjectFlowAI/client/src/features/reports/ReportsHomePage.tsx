import AssessmentOutlinedIcon from "@mui/icons-material/AssessmentOutlined";
import AttachMoneyOutlinedIcon from "@mui/icons-material/AttachMoneyOutlined";
import FavoriteBorderOutlinedIcon from "@mui/icons-material/FavoriteBorderOutlined";
import GroupOutlinedIcon from "@mui/icons-material/GroupOutlined";
import ShowChartOutlinedIcon from "@mui/icons-material/ShowChartOutlined";
import SpeedOutlinedIcon from "@mui/icons-material/SpeedOutlined";
import TimelineOutlinedIcon from "@mui/icons-material/TimelineOutlined";
import { Card, CardActionArea, CardContent, Grid, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { AppShell } from "../../components/AppShell";

interface ReportLink {
  label: string;
  description: string;
  path: string;
  icon: ReactNode;
  scope: "Organization" | "Project";
}

const REPORT_LINKS: ReportLink[] = [
  {
    label: "Executive Dashboard",
    description: "Org-wide KPIs and a color-coded project health overview.",
    path: "/reports/executive",
    icon: <AssessmentOutlinedIcon fontSize="large" color="primary" />,
    scope: "Organization",
  },
  {
    label: "Sprint Dashboard",
    description: "Burndown, blocked items, and open retro action items for a sprint.",
    path: "/reports/sprint",
    icon: <SpeedOutlinedIcon fontSize="large" color="primary" />,
    scope: "Project",
  },
  {
    label: "Cycle Time",
    description: "Average lead and cycle time, broken down by work item.",
    path: "/reports/cycle-time",
    icon: <TimelineOutlinedIcon fontSize="large" color="primary" />,
    scope: "Project",
  },
  {
    label: "Project Health",
    description: "A focused health check with the reasons spelled out.",
    path: "/reports/project-health",
    icon: <FavoriteBorderOutlinedIcon fontSize="large" color="primary" />,
    scope: "Project",
  },
  {
    label: "Productivity",
    description: "Completed items and story points per week or month.",
    path: "/reports/productivity",
    icon: <ShowChartOutlinedIcon fontSize="large" color="primary" />,
    scope: "Project",
  },
  {
    label: "Resource Utilization",
    description: "Estimated vs. logged hours and utilization per team member.",
    path: "/reports/resource-utilization",
    icon: <GroupOutlinedIcon fontSize="large" color="primary" />,
    scope: "Organization",
  },
  {
    label: "Cost Analysis",
    description: "Billable and non-billable hours with a per-user cost breakdown.",
    path: "/reports/cost-analysis",
    icon: <AttachMoneyOutlinedIcon fontSize="large" color="primary" />,
    scope: "Project",
  },
];

export function ReportsHomePage() {
  const navigate = useNavigate();

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack>
          <Typography variant="h5" fontWeight={700}>
            Reports
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Analytics across your organization and projects. Project-scoped reports are also
            reachable from a project's own Reports tab, pre-selected for that project.
          </Typography>
        </Stack>

        <Grid container spacing={2}>
          {REPORT_LINKS.map((report) => (
            <Grid key={report.path} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ height: "100%" }}>
                <CardActionArea
                  onClick={() => navigate(report.path)}
                  sx={{ height: "100%", p: 0.5 }}
                >
                  <CardContent>
                    <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                      {report.icon}
                      <Typography variant="caption" color="text.secondary">
                        {report.scope}
                      </Typography>
                    </Stack>
                    <Typography variant="subtitle1" fontWeight={700} sx={{ mt: 1 }}>
                      {report.label}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {report.description}
                    </Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </AppShell>
  );
}
