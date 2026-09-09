import AddIcon from "@mui/icons-material/Add";
import CloseIcon from "@mui/icons-material/Close";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import DownloadOutlinedIcon from "@mui/icons-material/DownloadOutlined";
import UploadFileOutlinedIcon from "@mui/icons-material/UploadFileOutlined";
import PersonAddOutlinedIcon from "@mui/icons-material/PersonAddOutlined";
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Autocomplete,
  Avatar,
  Box,
  Button,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
  Drawer,
  FormControlLabel,
  IconButton,
  LinearProgress,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { formatDistanceToNow } from "date-fns";
import { useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { workItemsApi } from "../../api/workItems";
import { useAttachments } from "../../hooks/useAttachments";
import { useChecklist } from "../../hooks/useChecklist";
import { useComments } from "../../hooks/useComments";
import { useCustomFields, useSetCustomFieldValue } from "../../hooks/useCustomFields";
import { useLabels } from "../../hooks/useLabels";
import { useTimeLogs } from "../../hooks/useTimeLogs";
import { useSetTimeLogBillable } from "../../hooks/useTimesheet";
import { useUsers } from "../../hooks/useUsers";
import { useDeleteWorkItem, useSetWorkItemSprint, useUpdateWorkItem } from "../../hooks/useWorkItems";
import { useWorkItem } from "../../hooks/useWorkItem";
import { useWorkItems } from "../../hooks/useWorkItems";
import { useSprints } from "../../hooks/useSprints";
import { useAuthStore } from "../../store/authStore";
import {
  WORK_ITEM_PRIORITIES,
  WORK_ITEM_STATUSES,
  WORK_ITEM_STATUS_LABELS,
  WORK_ITEM_TYPES,
  type WorkItemPriority,
  type WorkItemStatus,
  type WorkItemType,
} from "../../types";
import { BugAnalyzerPanel } from "../ai/BugAnalyzerPanel";
import { StoryPointEstimatorButton } from "../ai/StoryPointEstimatorButton";
import { MentionCommentBox } from "./MentionCommentBox";

interface WorkItemDetailDrawerProps {
  projectId: string;
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function humanizeActivity(action: string, fieldName?: string | null, oldValue?: string | null, newValue?: string | null) {
  if (fieldName) {
    return `changed ${fieldName}${oldValue ? ` from ${oldValue}` : ""}${newValue ? ` to ${newValue}` : ""}`;
  }
  return action;
}

export function WorkItemDetailDrawer({ projectId }: WorkItemDetailDrawerProps) {
  const [searchParams, setSearchParams] = useSearchParams();
  const taskId = searchParams.get("task") ?? undefined;
  const open = !!taskId;

  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: item, isLoading } = useWorkItem(taskId);
  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 200 });
  const { data: labels } = useLabels(projectId);
  const { data: siblingItemsPage } = useWorkItems({ projectId, page: 1, pageSize: 200 });
  const { data: customFieldDefs } = useCustomFields(projectId);

  const updateMutation = useUpdateWorkItem(projectId);
  const deleteMutation = useDeleteWorkItem(projectId);
  const checklist = useChecklist(taskId ?? "");
  const comments = useComments(taskId ?? "");
  const attachments = useAttachments(taskId ?? "");
  const timeLogs = useTimeLogs(taskId ?? "");
  const setCustomFieldValue = useSetCustomFieldValue(taskId ?? "");
  const setTimeLogBillable = useSetTimeLogBillable();
  const setSprintMutation = useSetWorkItemSprint(projectId);
  const { data: sprintsPage } = useSprints({ projectId, page: 1, pageSize: 100 });

  const queryClient = useQueryClient();
  const invalidateDetail = () =>
    queryClient.invalidateQueries({ queryKey: ["workItems", "detail", taskId] });

  const addLabelMutation = useMutation({
    mutationFn: (labelId: string) => workItemsApi.addLabel(taskId as string, labelId),
    onSuccess: invalidateDetail,
  });
  const removeLabelMutation = useMutation({
    mutationFn: (labelId: string) => workItemsApi.removeLabel(taskId as string, labelId),
    onSuccess: invalidateDetail,
  });
  const addFollowerMutation = useMutation({
    mutationFn: (userId: string) => workItemsApi.addFollower(taskId as string, userId),
    onSuccess: invalidateDetail,
  });
  const removeFollowerMutation = useMutation({
    mutationFn: (userId: string) => workItemsApi.removeFollower(taskId as string, userId),
    onSuccess: invalidateDetail,
  });
  const addDependencyMutation = useMutation({
    mutationFn: (dependsOnWorkItemId: string) =>
      workItemsApi.addDependency(taskId as string, { dependsOnWorkItemId, dependencyType: "Blocks" }),
    onSuccess: invalidateDetail,
  });
  const removeDependencyMutation = useMutation({
    mutationFn: (depId: string) => workItemsApi.removeDependency(taskId as string, depId),
    onSuccess: invalidateDetail,
  });

  const [newChecklistText, setNewChecklistText] = useState("");
  const [newDependencyId, setNewDependencyId] = useState<string | null>(null);
  const [newFollowerId, setNewFollowerId] = useState<string | null>(null);
  const [newLabelId, setNewLabelId] = useState<string | null>(null);
  const [timeLogMinutes, setTimeLogMinutes] = useState("");
  const [timeLogNote, setTimeLogNote] = useState("");
  const [timeLogDate, setTimeLogDate] = useState(new Date().toISOString().slice(0, 10));
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const orgUsers = usersPage?.items ?? [];
  const availableParentCandidates = (siblingItemsPage?.items ?? []).filter((w) => w.id !== taskId);

  const close = () => {
    searchParams.delete("task");
    setSearchParams(searchParams, { replace: true });
  };

  const handleFieldChange = (field: string, value: unknown) => {
    if (!taskId) return;
    updateMutation.mutate({ id: taskId, payload: { [field]: value } });
  };

  const checklistProgress = useMemo(() => {
    if (!item || item.checklistItems.length === 0) return 0;
    const done = item.checklistItems.filter((c) => c.isDone).length;
    return Math.round((done / item.checklistItems.length) * 100);
  }, [item]);

  return (
    <Drawer anchor="right" open={open} onClose={close}>
      <Box sx={{ width: { xs: "100vw", sm: 520 }, p: 3, height: "100%", overflowY: "auto" }}>
        {isLoading || !item ? (
          <Stack spacing={2}>
            <Skeleton variant="text" width="60%" height={40} />
            <Skeleton variant="rounded" height={200} />
          </Stack>
        ) : (
          <Stack spacing={2.5}>
            <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
              <TextField
                variant="standard"
                fullWidth
                defaultValue={item.title}
                onBlur={(e) => {
                  if (e.target.value !== item.title) handleFieldChange("title", e.target.value);
                }}
                slotProps={{ input: { style: { fontSize: "1.25rem", fontWeight: 700 } } }}
              />
              <Stack direction="row" spacing={0.5}>
                <Tooltip title="Delete task">
                  <IconButton
                    size="small"
                    onClick={() => {
                      if (taskId) {
                        deleteMutation.mutate(taskId, { onSuccess: close });
                      }
                    }}
                  >
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
                <IconButton size="small" aria-label="Close" onClick={close}>
                  <CloseIcon fontSize="small" />
                </IconButton>
              </Stack>
            </Stack>

            <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", gap: 2 }}>
              <Select
                size="small"
                value={item.status}
                onChange={(e) => handleFieldChange("status", e.target.value as WorkItemStatus)}
              >
                {WORK_ITEM_STATUSES.map((s) => (
                  <MenuItem key={s} value={s}>
                    {WORK_ITEM_STATUS_LABELS[s]}
                  </MenuItem>
                ))}
              </Select>
              <Select
                size="small"
                value={item.priority}
                onChange={(e) => handleFieldChange("priority", e.target.value as WorkItemPriority)}
              >
                {WORK_ITEM_PRIORITIES.map((p) => (
                  <MenuItem key={p} value={p}>
                    {p}
                  </MenuItem>
                ))}
              </Select>
              <Select
                size="small"
                value={item.type}
                onChange={(e) => handleFieldChange("type", e.target.value as WorkItemType)}
              >
                {WORK_ITEM_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {t}
                  </MenuItem>
                ))}
              </Select>
              <Select
                size="small"
                displayEmpty
                value={item.sprintId ?? ""}
                onChange={(e) =>
                  setSprintMutation.mutate({ id: item.id, sprintId: e.target.value || null })
                }
                sx={{ minWidth: 160 }}
              >
                <MenuItem value="">Backlog (no sprint)</MenuItem>
                {(sprintsPage?.items ?? [])
                  .filter((s) => s.status !== "Completed" || s.id === item.sprintId)
                  .map((s) => (
                    <MenuItem key={s.id} value={s.id}>
                      {s.name}
                    </MenuItem>
                  ))}
              </Select>
            </Stack>

            <TextField
              label="Description"
              multiline
              minRows={3}
              fullWidth
              defaultValue={item.description ?? ""}
              onBlur={(e) => {
                if (e.target.value !== (item.description ?? "")) {
                  handleFieldChange("description", e.target.value);
                }
              }}
            />

            <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", gap: 2 }}>
              <Autocomplete
                sx={{ minWidth: 220 }}
                options={orgUsers}
                value={orgUsers.find((u) => u.id === item.assigneeUserId) ?? null}
                getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
                onChange={(_, value) => handleFieldChange("assigneeUserId", value?.id ?? null)}
                renderInput={(params) => <TextField {...params} label="Assignee" size="small" />}
              />
              <TextField
                label="Due date"
                type="date"
                size="small"
                defaultValue={item.dueDate ? item.dueDate.slice(0, 10) : ""}
                slotProps={{ inputLabel: { shrink: true } }}
                onBlur={(e) => handleFieldChange("dueDate", e.target.value || null)}
              />
              <Stack direction="row" spacing={0.5} alignItems="center">
                <TextField
                  key={item.storyPoints ?? "none"}
                  label="Story points"
                  type="number"
                  size="small"
                  sx={{ width: 130 }}
                  defaultValue={item.storyPoints ?? ""}
                  onBlur={(e) =>
                    handleFieldChange(
                      "storyPoints",
                      e.target.value === "" ? null : Number(e.target.value)
                    )
                  }
                />
                <StoryPointEstimatorButton
                  title={item.title}
                  description={item.description}
                  onEstimate={(points) => handleFieldChange("storyPoints", points)}
                />
              </Stack>
            </Stack>

            <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", gap: 2 }}>
              <TextField
                label="Estimated hours"
                type="number"
                size="small"
                sx={{ width: 160 }}
                defaultValue={item.estimatedHours ?? ""}
                onBlur={(e) =>
                  handleFieldChange(
                    "estimatedHours",
                    e.target.value === "" ? null : Number(e.target.value)
                  )
                }
              />
              <Box sx={{ display: "flex", alignItems: "center" }}>
                <Typography variant="body2" color="text.secondary">
                  Actual hours:&nbsp;
                </Typography>
                <Typography variant="subtitle2" fontWeight={700}>
                  {item.actualHours}
                </Typography>
              </Box>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={item.isRecurring}
                    onChange={(e) => handleFieldChange("isRecurring", e.target.checked)}
                  />
                }
                label="Recurring"
              />
            </Stack>

            <Autocomplete
              options={availableParentCandidates}
              value={availableParentCandidates.find((w) => w.id === item.parentWorkItemId) ?? null}
              getOptionLabel={(w) => w.title}
              onChange={(_, value) => handleFieldChange("parentWorkItemId", value?.id ?? null)}
              renderInput={(params) => (
                <TextField {...params} label="Parent task" size="small" />
              )}
            />

            <Divider />

            {/* Checklist */}
            <Accordion defaultExpanded disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Checklist ({item.checklistItems.filter((c) => c.isDone).length}/
                  {item.checklistItems.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  {item.checklistItems.length > 0 && (
                    <LinearProgress variant="determinate" value={checklistProgress} />
                  )}
                  {item.checklistItems.map((ci) => (
                    <Stack key={ci.id} direction="row" alignItems="center" spacing={1}>
                      <Checkbox
                        size="small"
                        checked={ci.isDone}
                        onChange={(e) =>
                          checklist.toggleItem.mutate({ itemId: ci.id, isDone: e.target.checked })
                        }
                      />
                      <Typography
                        variant="body2"
                        sx={{ flex: 1, textDecoration: ci.isDone ? "line-through" : "none" }}
                      >
                        {ci.text}
                      </Typography>
                      <IconButton size="small" onClick={() => checklist.removeItem.mutate(ci.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  ))}
                  <Stack direction="row" spacing={1}>
                    <TextField
                      size="small"
                      fullWidth
                      placeholder="Add checklist item"
                      value={newChecklistText}
                      onChange={(e) => setNewChecklistText(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && newChecklistText.trim()) {
                          checklist.addItem.mutate(newChecklistText.trim());
                          setNewChecklistText("");
                        }
                      }}
                    />
                    <Button
                      size="small"
                      startIcon={<AddIcon fontSize="small" />}
                      disabled={!newChecklistText.trim()}
                      onClick={() => {
                        checklist.addItem.mutate(newChecklistText.trim());
                        setNewChecklistText("");
                      }}
                    >
                      Add
                    </Button>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* AI Bug Analysis — Bug-type work items only */}
            {item.type === "Bug" && (
              <Accordion disableGutters>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="subtitle2" fontWeight={700}>
                    AI Bug Analysis
                  </Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <BugAnalyzerPanel workItemId={item.id} />
                </AccordionDetails>
              </Accordion>
            )}

            {/* Labels */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Labels ({item.labels.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                    {item.labels.map((l) => (
                      <Chip
                        key={l.id}
                        label={l.name}
                        onDelete={() => removeLabelMutation.mutate(l.id)}
                        sx={{ bgcolor: l.colorHex, color: "#fff" }}
                      />
                    ))}
                    {item.labels.length === 0 && (
                      <Typography variant="body2" color="text.secondary">
                        No labels yet
                      </Typography>
                    )}
                  </Stack>
                  <Stack direction="row" spacing={1}>
                    <Autocomplete
                      sx={{ flex: 1 }}
                      options={(labels ?? []).filter((l) => !item.labels.some((il) => il.id === l.id))}
                      getOptionLabel={(l) => l.name}
                      value={(labels ?? []).find((l) => l.id === newLabelId) ?? null}
                      onChange={(_, value) => setNewLabelId(value?.id ?? null)}
                      renderInput={(params) => (
                        <TextField {...params} size="small" label="Add existing label" />
                      )}
                    />
                    <Button
                      size="small"
                      disabled={!newLabelId || addLabelMutation.isPending}
                      onClick={() => {
                        if (newLabelId) addLabelMutation.mutate(newLabelId);
                        setNewLabelId(null);
                      }}
                    >
                      Add
                    </Button>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Followers */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Followers ({item.followers.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                    {item.followers.map((f) => (
                      <Chip
                        key={f.userId}
                        avatar={<Avatar>{f.name[0]}</Avatar>}
                        label={f.name}
                        onDelete={() => removeFollowerMutation.mutate(f.userId)}
                      />
                    ))}
                    {item.followers.length === 0 && (
                      <Typography variant="body2" color="text.secondary">
                        No followers yet
                      </Typography>
                    )}
                  </Stack>
                  <Stack direction="row" spacing={1}>
                    <Autocomplete
                      sx={{ flex: 1 }}
                      options={orgUsers.filter((u) => !item.followers.some((f) => f.userId === u.id))}
                      getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
                      value={orgUsers.find((u) => u.id === newFollowerId) ?? null}
                      onChange={(_, value) => setNewFollowerId(value?.id ?? null)}
                      renderInput={(params) => (
                        <TextField {...params} size="small" label="Add follower" />
                      )}
                    />
                    <Button
                      size="small"
                      startIcon={<PersonAddOutlinedIcon fontSize="small" />}
                      disabled={!newFollowerId || addFollowerMutation.isPending}
                      onClick={() => {
                        if (newFollowerId) addFollowerMutation.mutate(newFollowerId);
                        setNewFollowerId(null);
                      }}
                    >
                      Add
                    </Button>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Dependencies */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Dependencies ({item.dependencies.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  <Stack spacing={1}>
                    {item.dependencies.map((d) => (
                      <Stack key={d.id} direction="row" alignItems="center" spacing={1}>
                        <Chip
                          label={`${d.dependsOnTitle} — ${WORK_ITEM_STATUS_LABELS[d.dependsOnStatus]}`}
                          variant="outlined"
                          onDelete={() => removeDependencyMutation.mutate(d.id)}
                        />
                        <Typography variant="caption" color="text.secondary">
                          {d.dependencyType}
                        </Typography>
                      </Stack>
                    ))}
                    {item.dependencies.length === 0 && (
                      <Typography variant="body2" color="text.secondary">
                        No dependencies yet
                      </Typography>
                    )}
                  </Stack>
                  <Stack direction="row" spacing={1}>
                    <Autocomplete
                      sx={{ flex: 1 }}
                      options={availableParentCandidates}
                      getOptionLabel={(w) => w.title}
                      value={availableParentCandidates.find((w) => w.id === newDependencyId) ?? null}
                      onChange={(_, value) => setNewDependencyId(value?.id ?? null)}
                      renderInput={(params) => (
                        <TextField {...params} size="small" label="Depends on" />
                      )}
                    />
                    <Button
                      size="small"
                      disabled={!newDependencyId || addDependencyMutation.isPending}
                      onClick={() => {
                        if (newDependencyId) addDependencyMutation.mutate(newDependencyId);
                        setNewDependencyId(null);
                      }}
                    >
                      Add
                    </Button>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Attachments */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Attachments ({item.attachments.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  {item.attachments.map((a) => (
                    <Stack key={a.id} direction="row" alignItems="center" spacing={1}>
                      <Typography variant="body2" sx={{ flex: 1 }}>
                        {a.fileName}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {formatBytes(a.fileSizeBytes)}
                      </Typography>
                      <IconButton size="small" component="a" href={a.downloadUrl} target="_blank">
                        <DownloadOutlinedIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" onClick={() => attachments.remove.mutate(a.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  ))}
                  <Button
                    size="small"
                    startIcon={<UploadFileOutlinedIcon fontSize="small" />}
                    onClick={() => fileInputRef.current?.click()}
                    disabled={attachments.upload.isPending}
                  >
                    {attachments.upload.isPending ? "Uploading..." : "Upload file"}
                  </Button>
                  <input
                    ref={fileInputRef}
                    type="file"
                    hidden
                    onChange={(e) => {
                      const file = e.target.files?.[0];
                      if (file) attachments.upload.mutate(file);
                      e.target.value = "";
                    }}
                  />
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Time logs */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Time logs
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  {item.timeLogs.map((t) => (
                    <Stack key={t.id} direction="row" alignItems="center" spacing={1}>
                      <Typography variant="body2" sx={{ flex: 1 }}>
                        {t.userName} logged {t.minutes} min {t.note ? `— ${t.note}` : ""} (
                        {t.loggedDate.slice(0, 10)})
                      </Typography>
                      <Tooltip title={t.isBillable ? "Billable" : "Non-billable"}>
                        <FormControlLabel
                          sx={{ mr: 0 }}
                          control={
                            <Checkbox
                              size="small"
                              checked={!!t.isBillable}
                              onChange={(e) =>
                                setTimeLogBillable.mutate({
                                  timeLogId: t.id,
                                  isBillable: e.target.checked,
                                })
                              }
                            />
                          }
                          label={
                            <Typography variant="caption" color="text.secondary">
                              Billable
                            </Typography>
                          }
                        />
                      </Tooltip>
                      <IconButton size="small" onClick={() => timeLogs.removeTimeLog.mutate(t.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  ))}
                  <Stack direction="row" spacing={1}>
                    <TextField
                      size="small"
                      label="Minutes"
                      type="number"
                      sx={{ width: 100 }}
                      value={timeLogMinutes}
                      onChange={(e) => setTimeLogMinutes(e.target.value)}
                    />
                    <TextField
                      size="small"
                      label="Note"
                      value={timeLogNote}
                      onChange={(e) => setTimeLogNote(e.target.value)}
                    />
                    <TextField
                      size="small"
                      type="date"
                      value={timeLogDate}
                      onChange={(e) => setTimeLogDate(e.target.value)}
                    />
                    <Button
                      size="small"
                      disabled={!timeLogMinutes || timeLogs.addTimeLog.isPending}
                      onClick={() => {
                        timeLogs.addTimeLog.mutate(
                          {
                            minutes: Number(timeLogMinutes),
                            note: timeLogNote || undefined,
                            loggedDate: timeLogDate,
                          },
                          {
                            onSuccess: () => {
                              setTimeLogMinutes("");
                              setTimeLogNote("");
                            },
                          }
                        );
                      }}
                    >
                      Log
                    </Button>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Custom fields */}
            {customFieldDefs && customFieldDefs.length > 0 && (
              <Accordion disableGutters>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="subtitle2" fontWeight={700}>
                    Custom fields
                  </Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Stack spacing={2}>
                    {customFieldDefs.map((def) => {
                      const existing = item.customFieldValues.find(
                        (v) => v.customFieldDefinitionId === def.id
                      );
                      const options: string[] = def.optionsJson ? JSON.parse(def.optionsJson) : [];
                      if (def.fieldType === "Dropdown") {
                        return (
                          <Select
                            key={def.id}
                            size="small"
                            value={existing ? JSON.parse(existing.valueJson) : ""}
                            displayEmpty
                            onChange={(e) =>
                              setCustomFieldValue.mutate({
                                definitionId: def.id,
                                valueJson: JSON.stringify(e.target.value),
                              })
                            }
                          >
                            <MenuItem value="">
                              <em>{def.name}</em>
                            </MenuItem>
                            {options.map((o) => (
                              <MenuItem key={o} value={o}>
                                {o}
                              </MenuItem>
                            ))}
                          </Select>
                        );
                      }
                      if (def.fieldType === "Checkbox") {
                        return (
                          <FormControlLabel
                            key={def.id}
                            control={
                              <Checkbox
                                checked={existing ? JSON.parse(existing.valueJson) : false}
                                onChange={(e) =>
                                  setCustomFieldValue.mutate({
                                    definitionId: def.id,
                                    valueJson: JSON.stringify(e.target.checked),
                                  })
                                }
                              />
                            }
                            label={def.name}
                          />
                        );
                      }
                      return (
                        <TextField
                          key={def.id}
                          size="small"
                          label={def.name}
                          type={def.fieldType === "Number" ? "number" : def.fieldType === "Date" ? "date" : "text"}
                          slotProps={def.fieldType === "Date" ? { inputLabel: { shrink: true } } : undefined}
                          defaultValue={existing ? JSON.parse(existing.valueJson) : ""}
                          onBlur={(e) =>
                            setCustomFieldValue.mutate({
                              definitionId: def.id,
                              valueJson: JSON.stringify(
                                def.fieldType === "Number" ? Number(e.target.value) : e.target.value
                              ),
                            })
                          }
                        />
                      );
                    })}
                  </Stack>
                </AccordionDetails>
              </Accordion>
            )}

            {/* Comments */}
            <Accordion defaultExpanded disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Comments ({item.comments.length})
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={2}>
                  {item.comments.map((c) => (
                    <Stack key={c.id} direction="row" spacing={1.5}>
                      <Avatar sx={{ width: 28, height: 28, fontSize: 12 }}>{c.authorName[0]}</Avatar>
                      <Box sx={{ flex: 1 }}>
                        <Stack direction="row" spacing={1} alignItems="baseline">
                          <Typography variant="body2" fontWeight={600}>
                            {c.authorName}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {formatDistanceToNow(new Date(c.createdAt), { addSuffix: true })}
                          </Typography>
                        </Stack>
                        <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                          {c.body}
                        </Typography>
                      </Box>
                      <IconButton size="small" onClick={() => comments.removeComment.mutate(c.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  ))}
                  <MentionCommentBox
                    members={orgUsers}
                    submitting={comments.addComment.isPending}
                    onSubmit={(body, mentionedUserIds) =>
                      comments.addComment.mutate({ body, mentionedUserIds })
                    }
                  />
                </Stack>
              </AccordionDetails>
            </Accordion>

            {/* Activity */}
            <Accordion disableGutters>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" fontWeight={700}>
                  Activity
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  {[...item.activity]
                    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                    .map((a) => (
                      <Typography key={a.id} variant="body2" color="text.secondary">
                        <strong>{a.userName}</strong>{" "}
                        {humanizeActivity(a.action, a.fieldName, a.oldValue, a.newValue)} —{" "}
                        {formatDistanceToNow(new Date(a.createdAt), { addSuffix: true })}
                      </Typography>
                    ))}
                  {item.activity.length === 0 && (
                    <Typography variant="body2" color="text.secondary">
                      No activity recorded yet.
                    </Typography>
                  )}
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Stack>
        )}
        {(updateMutation.isPending || deleteMutation.isPending) && (
          <Box sx={{ position: "fixed", bottom: 16, right: 16 }}>
            <CircularProgress size={20} />
          </Box>
        )}
      </Box>
    </Drawer>
  );
}
