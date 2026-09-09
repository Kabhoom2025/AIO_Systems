import { Autocomplete, MenuItem, Select, Stack, TextField } from "@mui/material";
import {
  WORK_ITEM_STATUSES,
  WORK_ITEM_STATUS_LABELS,
  type WorkItemStatus,
  type WorkflowActionType,
} from "../../types";

export interface ActionConfigState {
  status: string;
  userId: string;
  labelId: string;
  title: string;
  body: string;
  url: string;
  method: string;
  approverUserId: string;
}

export function emptyActionConfig(): ActionConfigState {
  return { status: "", userId: "", labelId: "", title: "", body: "", url: "", method: "POST", approverUserId: "" };
}

/** JSON keys actually persisted for a given action type — the rest of ActionConfigState
 * is UI-only scratch space so switching action type doesn't lose unrelated fields. */
export function serializeActionConfig(actionType: WorkflowActionType, config: ActionConfigState): string {
  switch (actionType) {
    case "ChangeStatus":
      return JSON.stringify({ status: config.status });
    case "AssignUser":
      return JSON.stringify({ userId: config.userId });
    case "AddLabel":
      return JSON.stringify({ labelId: config.labelId });
    case "SendNotification":
      // Backend reads {title, body, userId?} — userId is optional and, if omitted, the
      // notification goes to the triggering WorkItem's assignee (falling back to its reporter).
      return JSON.stringify({
        title: config.title || undefined,
        body: config.body || undefined,
        userId: config.userId || undefined,
      });
    case "CallWebhook":
      return JSON.stringify({ url: config.url, method: config.method || "POST" });
    case "RequireApproval":
      return JSON.stringify({ approverUserId: config.approverUserId });
    default:
      return "{}";
  }
}

export function deserializeActionConfig(actionConfigJson: string): ActionConfigState {
  const base = emptyActionConfig();
  try {
    const parsed = JSON.parse(actionConfigJson || "{}") as Record<string, string>;
    return { ...base, ...parsed };
  } catch {
    return base;
  }
}

interface WorkflowActionConfigFieldsProps {
  actionType: WorkflowActionType;
  config: ActionConfigState;
  onChange: (config: ActionConfigState) => void;
  orgUsers: { id: string; firstName: string; lastName: string }[];
  labels: { id: string; name: string }[];
}

/** Renders the type-appropriate config form for a workflow action — this is what lets
 * someone build a working automation without reading the actionConfigJson shape: each
 * action type gets a real dropdown/field instead of a raw JSON textarea. */
export function WorkflowActionConfigFields({
  actionType,
  config,
  onChange,
  orgUsers,
  labels,
}: WorkflowActionConfigFieldsProps) {
  const set = (patch: Partial<ActionConfigState>) => onChange({ ...config, ...patch });

  if (actionType === "ChangeStatus") {
    return (
      <Select
        size="small"
        displayEmpty
        value={config.status}
        onChange={(e) => set({ status: e.target.value })}
        sx={{ minWidth: 180 }}
      >
        <MenuItem value="">
          <em>Choose status</em>
        </MenuItem>
        {WORK_ITEM_STATUSES.map((s: WorkItemStatus) => (
          <MenuItem key={s} value={s}>
            {WORK_ITEM_STATUS_LABELS[s]}
          </MenuItem>
        ))}
      </Select>
    );
  }

  if (actionType === "AssignUser") {
    return (
      <Autocomplete
        size="small"
        sx={{ minWidth: 240 }}
        options={orgUsers}
        getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
        value={orgUsers.find((u) => u.id === config.userId) ?? null}
        onChange={(_, value) => set({ userId: value?.id ?? "" })}
        renderInput={(params) => <TextField {...params} label="Assign to" />}
      />
    );
  }

  if (actionType === "AddLabel") {
    return (
      <Autocomplete
        size="small"
        sx={{ minWidth: 220 }}
        options={labels}
        getOptionLabel={(l) => l.name}
        value={labels.find((l) => l.id === config.labelId) ?? null}
        onChange={(_, value) => set({ labelId: value?.id ?? "" })}
        renderInput={(params) => <TextField {...params} label="Label" />}
      />
    );
  }

  if (actionType === "SendNotification") {
    return (
      <Stack direction="row" spacing={1} sx={{ flex: 1 }}>
        <TextField
          size="small"
          label="Title"
          placeholder="Workflow automation"
          value={config.title}
          onChange={(e) => set({ title: e.target.value })}
          sx={{ minWidth: 200 }}
        />
        <TextField
          size="small"
          fullWidth
          label="Message"
          placeholder="A workflow action fired."
          value={config.body}
          onChange={(e) => set({ body: e.target.value })}
        />
      </Stack>
    );
  }

  if (actionType === "CallWebhook") {
    return (
      <Stack direction="row" spacing={1} sx={{ flex: 1 }}>
        <Select
          size="small"
          value={config.method || "POST"}
          onChange={(e) => set({ method: e.target.value })}
        >
          {["GET", "POST", "PUT", "PATCH", "DELETE"].map((m) => (
            <MenuItem key={m} value={m}>
              {m}
            </MenuItem>
          ))}
        </Select>
        <TextField
          size="small"
          fullWidth
          label="Webhook URL"
          placeholder="https://example.com/hooks/..."
          value={config.url}
          onChange={(e) => set({ url: e.target.value })}
        />
      </Stack>
    );
  }

  // RequireApproval
  return (
    <Autocomplete
      size="small"
      sx={{ minWidth: 240 }}
      options={orgUsers}
      getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
      value={orgUsers.find((u) => u.id === config.approverUserId) ?? null}
      onChange={(_, value) => set({ approverUserId: value?.id ?? "" })}
      renderInput={(params) => <TextField {...params} label="Approver" />}
    />
  );
}
