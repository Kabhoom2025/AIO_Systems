import { FormControl, InputLabel, MenuItem, Select } from "@mui/material";
import { useProjects } from "../../hooks/useProjects";
import { useAuthStore } from "../../store/authStore";

interface ProjectPickerFieldProps {
  value: string;
  onChange: (projectId: string) => void;
  label?: string;
  /** When set, adds a "clear" option (e.g. "All projects") so the filter can be optional. */
  clearLabel?: string;
}

/** Plain controlled project picker reused across report pages that need a project
 * selected before they can query — kept separate from FormSelect since these pages
 * use local component state rather than react-hook-form. */
export function ProjectPickerField({
  value,
  onChange,
  label = "Project",
  clearLabel,
}: ProjectPickerFieldProps) {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";
  const { data, isLoading } = useProjects({ organizationId, page: 1, pageSize: 200 });
  const projects = data?.items ?? [];

  const labelId = "project-picker-label";
  return (
    <FormControl size="small" sx={{ minWidth: 220 }}>
      <InputLabel id={labelId}>{label}</InputLabel>
      <Select
        labelId={labelId}
        label={label}
        value={value}
        disabled={isLoading}
        onChange={(e) => onChange(e.target.value)}
        displayEmpty
      >
        {clearLabel && <MenuItem value="">{clearLabel}</MenuItem>}
        {projects.length === 0 && (
          <MenuItem value="" disabled>
            {isLoading ? "Loading…" : "No projects"}
          </MenuItem>
        )}
        {projects.map((p) => (
          <MenuItem key={p.id} value={p.id}>
            {p.name}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
