import { Box, Tab, Tabs } from "@mui/material";
import { useParams, useSearchParams } from "react-router-dom";
import { CostAnalysisContent } from "../reports/CostAnalysisPage";
import { CycleTimeContent } from "../reports/CycleTimeReportPage";
import { ProjectHealthContent } from "../reports/ProjectHealthPage";
import { SprintDashboardContent } from "../reports/SprintDashboardPage";

const SUB_TABS = [
  { label: "Sprint Dashboard", value: "sprint" },
  { label: "Cycle Time", value: "cycle-time" },
  { label: "Health", value: "health" },
  { label: "Cost Analysis", value: "cost" },
];

// Project-scoped reports pre-select the current project — a middle ground between
// burying reports only in the global /reports section and only in the workspace:
// this tab deep-links into the same content components the global pages use.
export function ProjectReportsTab() {
  const { projectId } = useParams<{ projectId: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const activeSubTab = searchParams.get("view") ?? "sprint";

  const handleChange = (value: string) => {
    const next = new URLSearchParams(searchParams);
    next.set("view", value);
    setSearchParams(next, { replace: true });
  };

  return (
    <Box>
      <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 2 }}>
        <Tabs value={activeSubTab} onChange={(_, value) => handleChange(value)}>
          {SUB_TABS.map((t) => (
            <Tab key={t.value} label={t.label} value={t.value} />
          ))}
        </Tabs>
      </Box>

      {activeSubTab === "sprint" && <SprintDashboardContent projectId={projectId} />}
      {activeSubTab === "cycle-time" && <CycleTimeContent projectId={projectId} />}
      {activeSubTab === "health" && <ProjectHealthContent projectId={projectId} />}
      {activeSubTab === "cost" && <CostAnalysisContent projectId={projectId} />}
    </Box>
  );
}
