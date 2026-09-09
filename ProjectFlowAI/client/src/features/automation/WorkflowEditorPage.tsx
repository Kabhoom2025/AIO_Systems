import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import {
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useLabels } from "../../hooks/useLabels";
import { useUsers } from "../../hooks/useUsers";
import { useCreateWorkflow, useUpdateWorkflow, useWorkflow } from "../../hooks/useWorkflow";
import { useAuthStore } from "../../store/authStore";
import {
  WORKFLOW_ACTION_LABELS,
  WORKFLOW_ACTION_TYPES,
  WORKFLOW_CONDITION_OPERATORS,
  WORKFLOW_TRIGGER_LABELS,
  WORKFLOW_TRIGGER_TYPES,
  type WorkflowAction,
  type WorkflowActionType,
  type WorkflowCondition,
  type WorkflowConditionOperator,
  type WorkflowTriggerType,
} from "../../types";
import {
  deserializeActionConfig,
  emptyActionConfig,
  serializeActionConfig,
  WorkflowActionConfigFields,
  type ActionConfigState,
} from "./WorkflowActionConfigFields";

interface ConditionRow {
  fieldPath: string;
  operator: WorkflowConditionOperator;
  value: string;
}

interface ActionRow {
  actionType: WorkflowActionType;
  config: ActionConfigState;
  order: number;
}

function toConditionRows(conditions: WorkflowCondition[]): ConditionRow[] {
  return conditions.map((c) => ({ fieldPath: c.fieldPath, operator: c.operator, value: c.value }));
}

function toActionRows(actions: WorkflowAction[]): ActionRow[] {
  return [...actions]
    .sort((a, b) => a.order - b.order)
    .map((a, i) => ({
      actionType: a.actionType,
      config: deserializeActionConfig(a.actionConfigJson),
      order: i,
    }));
}

export function WorkflowEditorPage() {
  const { projectId, workflowId } = useParams<{ projectId: string; workflowId: string }>();
  const isEdit = !!workflowId;
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const { data: existing, isLoading: loadingExisting } = useWorkflow(workflowId);
  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 200 });
  const { data: labels } = useLabels(projectId);
  const createWorkflow = useCreateWorkflow();
  const updateWorkflow = useUpdateWorkflow(workflowId ?? "");

  const [name, setName] = useState("");
  const [triggerType, setTriggerType] = useState<WorkflowTriggerType>("WorkItemCreated");
  const [cronExpression, setCronExpression] = useState("0 9 * * 1");
  const [conditions, setConditions] = useState<ConditionRow[]>([]);
  const [actions, setActions] = useState<ActionRow[]>([]);

  useEffect(() => {
    if (existing) {
      setName(existing.name);
      setTriggerType(existing.triggerType);
      setCronExpression(existing.cronExpression ?? "0 9 * * 1");
      setConditions(toConditionRows(existing.conditions));
      setActions(toActionRows(existing.actions));
    }
  }, [existing]);

  const orgUsers = usersPage?.items ?? [];

  const addCondition = () =>
    setConditions((prev) => [...prev, { fieldPath: "status", operator: "Equals", value: "" }]);
  const updateCondition = (index: number, patch: Partial<ConditionRow>) =>
    setConditions((prev) => prev.map((c, i) => (i === index ? { ...c, ...patch } : c)));
  const removeCondition = (index: number) => setConditions((prev) => prev.filter((_, i) => i !== index));

  const addAction = () =>
    setActions((prev) => [...prev, { actionType: "ChangeStatus", config: emptyActionConfig(), order: prev.length }]);
  const updateAction = (index: number, patch: Partial<ActionRow>) =>
    setActions((prev) => prev.map((a, i) => (i === index ? { ...a, ...patch } : a)));
  const removeAction = (index: number) =>
    setActions((prev) => prev.filter((_, i) => i !== index).map((a, i) => ({ ...a, order: i })));

  const saving = createWorkflow.isPending || updateWorkflow.isPending;
  const canSave = name.trim().length > 0 && actions.length > 0;

  const handleSave = () => {
    if (!projectId || !canSave) return;
    const conditionsPayload: WorkflowCondition[] = conditions
      .filter((c) => c.fieldPath.trim())
      .map((c) => ({ fieldPath: c.fieldPath.trim(), operator: c.operator, value: c.value }));
    const actionsPayload: WorkflowAction[] = actions.map((a, i) => ({
      actionType: a.actionType,
      actionConfigJson: serializeActionConfig(a.actionType, a.config),
      order: i,
    }));

    if (isEdit && workflowId) {
      updateWorkflow.mutate(
        { name: name.trim(), conditions: conditionsPayload, actions: actionsPayload },
        { onSuccess: () => navigate(`/projects/${projectId}/automation`) }
      );
    } else {
      createWorkflow.mutate(
        {
          organizationId,
          projectId,
          name: name.trim(),
          triggerType,
          cronExpression: triggerType === "Scheduled" ? cronExpression : null,
          conditions: conditionsPayload,
          actions: actionsPayload,
        },
        { onSuccess: () => navigate(`/projects/${projectId}/automation`) }
      );
    }
  };

  if (isEdit && loadingExisting) {
    return (
      <Stack spacing={2}>
        <Skeleton variant="text" width={240} height={40} />
        <Skeleton variant="rounded" height={300} />
      </Stack>
    );
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          {isEdit ? "Edit workflow" : "New workflow"}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Define when this workflow runs, what conditions must hold, and what it does.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2.5}>
            <TextField
              label="Workflow name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              fullWidth
            />

            <Stack direction="row" spacing={2} alignItems="center">
              <Select
                size="small"
                value={triggerType}
                disabled={isEdit}
                onChange={(e) => setTriggerType(e.target.value as WorkflowTriggerType)}
                sx={{ minWidth: 260 }}
              >
                {WORKFLOW_TRIGGER_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {WORKFLOW_TRIGGER_LABELS[t]}
                  </MenuItem>
                ))}
              </Select>
              {triggerType === "Scheduled" && (
                <TextField
                  size="small"
                  label="Cron expression"
                  disabled={isEdit}
                  value={cronExpression}
                  onChange={(e) => setCronExpression(e.target.value)}
                  helperText="e.g. 0 9 * * 1 = every Monday at 9am"
                  sx={{ minWidth: 260 }}
                />
              )}
              {isEdit && (
                <Typography variant="caption" color="text.secondary">
                  Trigger type can't be changed after creation.
                </Typography>
              )}
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="subtitle1" fontWeight={700}>
                Conditions
              </Typography>
              <Button size="small" startIcon={<AddIcon />} onClick={addCondition}>
                Add condition
              </Button>
            </Stack>
            {conditions.length === 0 && (
              <Typography variant="body2" color="text.secondary">
                No conditions — the workflow runs on every matching trigger event.
              </Typography>
            )}
            <Stack spacing={1.5}>
              {conditions.map((c, i) => (
                <Stack key={i} direction="row" spacing={1.5} alignItems="center">
                  <TextField
                    size="small"
                    label="Field path"
                    placeholder="e.g. status, priority, assigneeUserId"
                    value={c.fieldPath}
                    onChange={(e) => updateCondition(i, { fieldPath: e.target.value })}
                    sx={{ minWidth: 200 }}
                  />
                  <Select
                    size="small"
                    value={c.operator}
                    onChange={(e) =>
                      updateCondition(i, { operator: e.target.value as WorkflowConditionOperator })
                    }
                  >
                    {WORKFLOW_CONDITION_OPERATORS.map((op) => (
                      <MenuItem key={op} value={op}>
                        {op}
                      </MenuItem>
                    ))}
                  </Select>
                  <TextField
                    size="small"
                    label="Value"
                    value={c.value}
                    onChange={(e) => updateCondition(i, { value: e.target.value })}
                    sx={{ flex: 1 }}
                  />
                  <Button size="small" color="error" onClick={() => removeCondition(i)}>
                    <DeleteOutlineIcon fontSize="small" />
                  </Button>
                </Stack>
              ))}
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="subtitle1" fontWeight={700}>
                Actions
              </Typography>
              <Button size="small" startIcon={<AddIcon />} onClick={addAction}>
                Add action
              </Button>
            </Stack>
            {actions.length === 0 && (
              <Typography variant="body2" color="error">
                Add at least one action for this workflow to do something.
              </Typography>
            )}
            <Stack spacing={2}>
              {actions.map((a, i) => (
                <Box key={i}>
                  <Stack direction="row" spacing={1.5} alignItems="center">
                    <Select
                      size="small"
                      value={a.actionType}
                      onChange={(e) =>
                        updateAction(i, {
                          actionType: e.target.value as WorkflowActionType,
                          config: emptyActionConfig(),
                        })
                      }
                      sx={{ minWidth: 200 }}
                    >
                      {WORKFLOW_ACTION_TYPES.map((t) => (
                        <MenuItem key={t} value={t}>
                          {WORKFLOW_ACTION_LABELS[t]}
                        </MenuItem>
                      ))}
                    </Select>
                    <WorkflowActionConfigFields
                      actionType={a.actionType}
                      config={a.config}
                      onChange={(config) => updateAction(i, { config })}
                      orgUsers={orgUsers}
                      labels={labels ?? []}
                    />
                    <Button size="small" color="error" onClick={() => removeAction(i)}>
                      <DeleteOutlineIcon fontSize="small" />
                    </Button>
                  </Stack>
                  {i < actions.length - 1 && <Divider sx={{ mt: 2 }} />}
                </Box>
              ))}
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      <Stack direction="row" spacing={1.5}>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          startIcon={saving ? <CircularProgress size={16} color="inherit" /> : undefined}
          onClick={handleSave}
        >
          {isEdit ? "Save changes" : "Create workflow"}
        </Button>
        <Button onClick={() => navigate(`/projects/${projectId}/automation`)}>Cancel</Button>
      </Stack>
    </Stack>
  );
}
