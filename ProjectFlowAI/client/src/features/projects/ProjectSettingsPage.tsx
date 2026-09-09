import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import {
  Autocomplete,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  IconButton,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import { useCreateCustomField, useCustomFields, useDeleteCustomField } from "../../hooks/useCustomFields";
import { useCreateLabel, useDeleteLabel, useLabels } from "../../hooks/useLabels";
import {
  useAddProjectMember,
  useProjectMembers,
  useRemoveProjectMember,
} from "../../hooks/useProjectMembers";
import { useUsers } from "../../hooks/useUsers";
import { useAuthStore } from "../../store/authStore";
import type { CustomFieldType } from "../../types";

const DEFAULT_LABEL_COLORS = [
  "#6355FF",
  "#00B8A9",
  "#F5A623",
  "#E85D75",
  "#3DD9CA",
  "#8B7CFF",
  "#4A3FCC",
];

const FIELD_TYPES: CustomFieldType[] = ["Text", "Number", "Date", "Dropdown", "Checkbox"];

export function ProjectSettingsPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const pid = projectId ?? "";
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 200 });
  const orgUsers = usersPage?.items ?? [];

  // Members
  const { data: members, isLoading: membersLoading } = useProjectMembers(pid);
  const addMemberMutation = useAddProjectMember(pid);
  const removeMemberMutation = useRemoveProjectMember(pid);
  const [newMemberId, setNewMemberId] = useState<string | null>(null);
  const [newMemberRole, setNewMemberRole] = useState("Contributor");

  // Labels
  const { data: labels, isLoading: labelsLoading } = useLabels(pid);
  const createLabelMutation = useCreateLabel(pid);
  const deleteLabelMutation = useDeleteLabel(pid);
  const [newLabelName, setNewLabelName] = useState("");
  const [newLabelColor, setNewLabelColor] = useState(DEFAULT_LABEL_COLORS[0]);

  // Custom fields
  const { data: customFields, isLoading: fieldsLoading } = useCustomFields(pid);
  const createFieldMutation = useCreateCustomField(pid);
  const deleteFieldMutation = useDeleteCustomField(pid);
  const [newFieldName, setNewFieldName] = useState("");
  const [newFieldType, setNewFieldType] = useState<CustomFieldType>("Text");
  const [newFieldOptions, setNewFieldOptions] = useState("");
  const [newFieldRequired, setNewFieldRequired] = useState(false);

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 6 }}>
        <Card variant="outlined">
          <CardContent>
            <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 2 }}>
              Project members
            </Typography>
            <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
              <Autocomplete
                sx={{ flex: 1 }}
                options={orgUsers.filter((u) => !members?.some((m) => m.userId === u.id))}
                getOptionLabel={(u) => `${u.firstName} ${u.lastName} (${u.email})`}
                value={orgUsers.find((u) => u.id === newMemberId) ?? null}
                onChange={(_, value) => setNewMemberId(value?.id ?? null)}
                renderInput={(params) => <TextField {...params} size="small" label="Add member" />}
              />
              <Select
                size="small"
                value={newMemberRole}
                onChange={(e) => setNewMemberRole(e.target.value)}
                sx={{ width: 150 }}
              >
                <MenuItem value="Contributor">Contributor</MenuItem>
                <MenuItem value="Lead">Lead</MenuItem>
                <MenuItem value="Viewer">Viewer</MenuItem>
              </Select>
              <Button
                variant="contained"
                disabled={!newMemberId || addMemberMutation.isPending}
                onClick={() => {
                  if (newMemberId) {
                    addMemberMutation.mutate(
                      { userId: newMemberId, roleInProject: newMemberRole },
                      { onSuccess: () => setNewMemberId(null) }
                    );
                  }
                }}
              >
                Add
              </Button>
            </Stack>
            {membersLoading ? null : !members || members.length === 0 ? (
              <EmptyState title="No members yet" description="Add org users to this project." />
            ) : (
              <Stack spacing={1}>
                {members.map((m) => {
                  const u = orgUsers.find((usr) => usr.id === m.userId);
                  return (
                    <Stack
                      key={m.userId}
                      direction="row"
                      justifyContent="space-between"
                      alignItems="center"
                      sx={{ p: 1, border: 1, borderColor: "divider", borderRadius: 1 }}
                    >
                      <Typography variant="body2">
                        {u ? `${u.firstName} ${u.lastName}` : m.userId} — {m.roleInProject}
                      </Typography>
                      <IconButton
                        size="small"
                        onClick={() => removeMemberMutation.mutate(m.userId)}
                      >
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  );
                })}
              </Stack>
            )}
          </CardContent>
        </Card>
      </Grid>

      <Grid size={{ xs: 12, md: 6 }}>
        <Card variant="outlined">
          <CardContent>
            <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 2 }}>
              Labels
            </Typography>
            <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
              <TextField
                size="small"
                label="Label name"
                value={newLabelName}
                onChange={(e) => setNewLabelName(e.target.value)}
                sx={{ flex: 1 }}
              />
              <Stack direction="row" spacing={0.5}>
                {DEFAULT_LABEL_COLORS.map((c) => (
                  <Box
                    key={c}
                    onClick={() => setNewLabelColor(c)}
                    sx={{
                      width: 22,
                      height: 22,
                      borderRadius: "50%",
                      bgcolor: c,
                      cursor: "pointer",
                      border: newLabelColor === c ? "2px solid" : "none",
                      borderColor: "text.primary",
                    }}
                  />
                ))}
              </Stack>
              <Button
                variant="contained"
                disabled={!newLabelName.trim() || createLabelMutation.isPending}
                onClick={() => {
                  createLabelMutation.mutate(
                    { projectId: pid, name: newLabelName.trim(), colorHex: newLabelColor },
                    { onSuccess: () => setNewLabelName("") }
                  );
                }}
              >
                Add
              </Button>
            </Stack>
            {labelsLoading ? null : !labels || labels.length === 0 ? (
              <EmptyState title="No labels yet" description="Create labels to tag tasks." />
            ) : (
              <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                {labels.map((l) => (
                  <Chip
                    key={l.id}
                    label={l.name}
                    sx={{ bgcolor: l.colorHex, color: "#fff" }}
                    onDelete={() => deleteLabelMutation.mutate(l.id)}
                  />
                ))}
              </Stack>
            )}
          </CardContent>
        </Card>
      </Grid>

      <Grid size={{ xs: 12 }}>
        <Card variant="outlined">
          <CardContent>
            <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 2 }}>
              Custom fields
            </Typography>
            <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: "wrap", gap: 1 }}>
              <TextField
                size="small"
                label="Field name"
                value={newFieldName}
                onChange={(e) => setNewFieldName(e.target.value)}
              />
              <Select
                size="small"
                value={newFieldType}
                onChange={(e) => setNewFieldType(e.target.value as CustomFieldType)}
                sx={{ width: 150 }}
              >
                {FIELD_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {t}
                  </MenuItem>
                ))}
              </Select>
              {newFieldType === "Dropdown" && (
                <TextField
                  size="small"
                  label="Options (comma-separated)"
                  value={newFieldOptions}
                  onChange={(e) => setNewFieldOptions(e.target.value)}
                  sx={{ minWidth: 220 }}
                />
              )}
              <Select
                size="small"
                value={newFieldRequired ? "yes" : "no"}
                onChange={(e) => setNewFieldRequired(e.target.value === "yes")}
                sx={{ width: 130 }}
              >
                <MenuItem value="no">Optional</MenuItem>
                <MenuItem value="yes">Required</MenuItem>
              </Select>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                disabled={!newFieldName.trim() || createFieldMutation.isPending}
                onClick={() => {
                  const optionsJson =
                    newFieldType === "Dropdown"
                      ? JSON.stringify(
                          newFieldOptions
                            .split(",")
                            .map((o) => o.trim())
                            .filter(Boolean)
                        )
                      : undefined;
                  createFieldMutation.mutate(
                    {
                      projectId: pid,
                      name: newFieldName.trim(),
                      fieldType: newFieldType,
                      optionsJson,
                      isRequired: newFieldRequired,
                    },
                    {
                      onSuccess: () => {
                        setNewFieldName("");
                        setNewFieldOptions("");
                        setNewFieldRequired(false);
                      },
                    }
                  );
                }}
              >
                Add field
              </Button>
            </Stack>
            {fieldsLoading ? null : !customFields || customFields.length === 0 ? (
              <EmptyState
                title="No custom fields yet"
                description="Define custom fields to capture extra data on tasks."
              />
            ) : (
              <Stack spacing={1}>
                {customFields.map((f) => (
                  <Stack
                    key={f.id}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                    sx={{ p: 1, border: 1, borderColor: "divider", borderRadius: 1 }}
                  >
                    <Typography variant="body2">
                      {f.name} — {f.fieldType} {f.isRequired && "(required)"}
                    </Typography>
                    <IconButton size="small" onClick={() => deleteFieldMutation.mutate(f.id)}>
                      <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                  </Stack>
                ))}
              </Stack>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}
