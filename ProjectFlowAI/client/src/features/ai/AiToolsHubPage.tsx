import AutoAwesomeOutlinedIcon from "@mui/icons-material/AutoAwesomeOutlined";
import BoltOutlinedIcon from "@mui/icons-material/BoltOutlined";
import BugReportOutlinedIcon from "@mui/icons-material/BugReportOutlined";
import ChatOutlinedIcon from "@mui/icons-material/ChatOutlined";
import CodeOutlinedIcon from "@mui/icons-material/CodeOutlined";
import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import GroupsOutlinedIcon from "@mui/icons-material/GroupsOutlined";
import NotesOutlinedIcon from "@mui/icons-material/NotesOutlined";
import SsidChartOutlinedIcon from "@mui/icons-material/SsidChartOutlined";
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Grid,
  Skeleton,
  Stack,
  Typography,
} from "@mui/material";
import type { ReactNode } from "react";
import { useParams } from "react-router-dom";
import { usePredictDeadline } from "../../hooks/usePredictDeadline";
import { useRiskPrediction } from "../../hooks/useRiskPrediction";
import { RiskPredictionWidget } from "./RiskPredictionWidget";
import { DeadlinePredictionWidget } from "./DeadlinePredictionWidget";
import { useNavigate } from "react-router-dom";

interface ToolTile {
  label: string;
  description: string;
  icon: ReactNode;
  segment: string;
}

const TOOLS: ToolTile[] = [
  {
    label: "Task Generator",
    description: "Describe what you need in plain English and get draft work items.",
    icon: <AutoAwesomeOutlinedIcon fontSize="large" color="primary" />,
    segment: "generate-tasks",
  },
  {
    label: "Sprint Planner",
    description: "Give a sprint goal and capacity — AI picks the backlog items that fit.",
    icon: <BoltOutlinedIcon fontSize="large" color="primary" />,
    segment: "sprint-planner",
  },
  {
    label: "Meeting Summarizer",
    description: "Paste raw meeting notes and get a clean summary with action items.",
    icon: <NotesOutlinedIcon fontSize="large" color="primary" />,
    segment: "meeting-summarizer",
  },
  {
    label: "AI Assistant Chat",
    description: "Ask questions about this project in a real conversation.",
    icon: <ChatOutlinedIcon fontSize="large" color="primary" />,
    segment: "chat",
  },
  {
    label: "Resource Allocation",
    description: "See AI recommendations for how to balance work across the team.",
    icon: <GroupsOutlinedIcon fontSize="large" color="primary" />,
    segment: "resource-allocation",
  },
  {
    label: "Task Prioritizer",
    description: "Re-rank the backlog by AI-suggested priority, then accept the changes.",
    icon: <SsidChartOutlinedIcon fontSize="large" color="primary" />,
    segment: "prioritize",
  },
  {
    label: "Code Review",
    description: "Paste a snippet and get line-level review comments.",
    icon: <CodeOutlinedIcon fontSize="large" color="primary" />,
    segment: "code-review",
  },
  {
    label: "Release Notes Generator",
    description: "Generate polished release notes for a date range, ready for the wiki.",
    icon: <DescriptionOutlinedIcon fontSize="large" color="primary" />,
    segment: "release-notes",
  },
  {
    label: "Bug Analyzer",
    description: "Open a Bug-type task and use \"AI: Analyze Bug\" in its detail panel.",
    icon: <BugReportOutlinedIcon fontSize="large" color="primary" />,
    segment: "",
  },
];

export function AiToolsHubPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const risk = useRiskPrediction(projectId);
  const deadline = usePredictDeadline(projectId);

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Tools
        </Typography>
        <Typography variant="body2" color="text.secondary">
          AI-assisted planning, analysis, and automation for this project. Every call to the
          assistant takes a few seconds — please be patient while it thinks.
        </Typography>
      </Box>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <RiskPredictionWidget data={risk.data} isLoading={risk.isLoading} error={risk.error} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <DeadlinePredictionWidget
            data={deadline.data}
            isLoading={deadline.isLoading}
            error={deadline.error}
          />
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        {TOOLS.map((tool) => (
          <Grid key={tool.label} size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined" sx={{ height: "100%" }}>
              <CardActionArea
                disabled={!tool.segment}
                sx={{ height: "100%" }}
                onClick={() => tool.segment && navigate(`/projects/${projectId}/ai/${tool.segment}`)}
              >
                <CardContent>
                  <Stack spacing={1.5}>
                    {tool.icon}
                    <Typography variant="subtitle1" fontWeight={700}>
                      {tool.label}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {tool.description}
                    </Typography>
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Stack>
  );
}

export function AiPageHeader({ title, description }: { title: string; description: string }) {
  return (
    <Box>
      <Typography variant="h5" fontWeight={700}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {description}
      </Typography>
    </Box>
  );
}

export function AiSectionSkeleton() {
  return (
    <Stack spacing={1.5}>
      <Skeleton variant="text" width="40%" height={32} />
      <Skeleton variant="rounded" height={140} />
    </Stack>
  );
}
