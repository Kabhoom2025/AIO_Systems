import DownloadOutlinedIcon from "@mui/icons-material/DownloadOutlined";
import AddIcon from "@mui/icons-material/Add";
import RouteOutlinedIcon from "@mui/icons-material/RouteOutlined";
import {
  Button,
  FormControlLabel,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  Switch,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import { useBaseline, useBaselines } from "../../hooks/useBaselines";
import { useCriticalPath } from "../../hooks/useCriticalPath";
import { useGanttChart } from "../../hooks/useGanttChart";
import { useProject } from "../../hooks/useProject";
import { ganttApi } from "../../api/gantt";
import { CreateBaselineDialog } from "./CreateBaselineDialog";
import { GanttTimeline } from "./GanttTimeline";
import type { GanttZoom } from "./ganttMeta";

export function GanttChartPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const { data: project } = useProject(projectId);
  const { data: chart, isLoading } = useGanttChart(projectId);
  const [zoom, setZoom] = useState<GanttZoom>("week");
  const [showCriticalPath, setShowCriticalPath] = useState(false);
  const [baselineId, setBaselineId] = useState<string>("");
  const [createBaselineOpen, setCreateBaselineOpen] = useState(false);
  const [exporting, setExporting] = useState(false);

  const { data: criticalPath } = useCriticalPath(projectId, showCriticalPath);
  const { data: baselines } = useBaselines(projectId);
  const { data: baselineDetail } = useBaseline(projectId, baselineId || undefined);

  const criticalPathIds = showCriticalPath && criticalPath ? new Set(criticalPath.workItemIds) : null;

  const handleExport = async () => {
    if (!projectId) return;
    setExporting(true);
    try {
      await ganttApi.exportCsv(projectId, project?.key ?? "project");
    } finally {
      setExporting(false);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={1.5}>
        <Typography variant="h6" fontWeight={700}>
          Gantt chart
        </Typography>
        <Stack direction="row" spacing={1.5} alignItems="center" flexWrap="wrap" useFlexGap>
          <ToggleButtonGroup size="small" exclusive value={zoom} onChange={(_, v) => v && setZoom(v)}>
            <ToggleButton value="day">Day</ToggleButton>
            <ToggleButton value="week">Week</ToggleButton>
            <ToggleButton value="month">Month</ToggleButton>
          </ToggleButtonGroup>

          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={showCriticalPath}
                onChange={(e) => setShowCriticalPath(e.target.checked)}
              />
            }
            label="Critical path"
          />

          <Select
            size="small"
            displayEmpty
            value={baselineId}
            onChange={(e) => setBaselineId(e.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">No baseline</MenuItem>
            {(baselines ?? []).map((b) => (
              <MenuItem key={b.id} value={b.id}>
                {b.name}
              </MenuItem>
            ))}
          </Select>

          <Button size="small" startIcon={<AddIcon fontSize="small" />} onClick={() => setCreateBaselineOpen(true)}>
            Create baseline
          </Button>

          <Button
            size="small"
            variant="outlined"
            startIcon={<DownloadOutlinedIcon fontSize="small" />}
            disabled={exporting}
            onClick={handleExport}
          >
            Export CSV
          </Button>
        </Stack>
      </Stack>

      {showCriticalPath && criticalPathIds && (
        <Stack direction="row" spacing={1} alignItems="center">
          <RouteOutlinedIcon fontSize="small" color="error" />
          <Typography variant="caption" color="text.secondary">
            {criticalPathIds.size} item{criticalPathIds.size === 1 ? "" : "s"} on the critical path are
            outlined in red.
          </Typography>
        </Stack>
      )}

      {isLoading || !chart ? (
        <Skeleton variant="rounded" height={480} />
      ) : chart.items.length === 0 ? (
        <EmptyState
          title="Nothing to schedule yet"
          description="Add start and end dates to work items to see them on the Gantt chart."
        />
      ) : (
        <GanttTimeline
          chart={chart}
          zoom={zoom}
          criticalPathIds={criticalPathIds}
          baseline={baselineDetail ?? null}
          onItemClick={(id) => navigate(`/projects/${projectId}/gantt?task=${id}`)}
        />
      )}

      {projectId && (
        <CreateBaselineDialog
          projectId={projectId}
          open={createBaselineOpen}
          onClose={() => setCreateBaselineOpen(false)}
        />
      )}
    </Stack>
  );
}
