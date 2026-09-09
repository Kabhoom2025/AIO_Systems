import { Box } from "@mui/material";
import { useParams } from "react-router-dom";
import { useProject } from "../../hooks/useProject";
import { DocsWorkspace } from "../docs/DocsWorkspace";

export function ProjectDocumentsPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const { data: project } = useProject(projectId);

  if (!projectId || !project) return null;

  return (
    <Box sx={{ height: "calc(100vh - 220px)", display: "flex" }}>
      <DocsWorkspace
        organizationId={project.organizationId}
        scope="ProjectDocument"
        projectId={projectId}
      />
    </Box>
  );
}
