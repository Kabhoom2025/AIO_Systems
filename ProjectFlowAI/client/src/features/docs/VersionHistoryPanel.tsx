import HistoryOutlinedIcon from "@mui/icons-material/HistoryOutlined";
import RestoreOutlinedIcon from "@mui/icons-material/RestoreOutlined";
import {
  Box,
  Button,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { EmptyState } from "../../components/EmptyState";
import {
  useDocPageVersion,
  useDocPageVersions,
  useRestoreDocPageVersion,
} from "../../hooks/useDocPageVersions";
import { RichTextEditor } from "./RichTextEditor";

interface VersionHistoryPanelProps {
  pageId: string;
  open: boolean;
  onClose: () => void;
}

export function VersionHistoryPanel({ pageId, open, onClose }: VersionHistoryPanelProps) {
  const { data: versions } = useDocPageVersions(pageId);
  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null);
  const { data: versionDetail } = useDocPageVersion(pageId, selectedVersionId ?? undefined);
  const restoreVersion = useRestoreDocPageVersion(pageId);
  const [confirmRestore, setConfirmRestore] = useState(false);

  return (
    <Drawer anchor="right" open={open} onClose={onClose}>
      <Box sx={{ width: 480, p: 2, height: "100%", display: "flex", flexDirection: "column" }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Version history
        </Typography>

        {!selectedVersionId ? (
          <>
            {versions && versions.length === 0 && (
              <EmptyState
                icon={<HistoryOutlinedIcon sx={{ fontSize: 48 }} />}
                title="No previous versions"
                description="Edits to this page will appear here."
              />
            )}
            <List sx={{ overflowY: "auto" }}>
              {(versions ?? []).map((v) => (
                <ListItemButton key={v.id} onClick={() => setSelectedVersionId(v.id)}>
                  <ListItemText
                    primary={`Version ${v.versionNumber}`}
                    secondary={`${v.editedByName} · ${new Date(v.createdAt).toLocaleString()}`}
                  />
                </ListItemButton>
              ))}
            </List>
          </>
        ) : (
          <Stack spacing={2} sx={{ flex: 1, minHeight: 0 }}>
            <Stack direction="row" alignItems="center" justifyContent="space-between">
              <Typography variant="subtitle2">
                Version {versionDetail?.versionNumber} — {versionDetail?.editedByName}
              </Typography>
              <IconButton
                size="small"
                title="Restore this version"
                onClick={() => setConfirmRestore(true)}
              >
                <RestoreOutlinedIcon fontSize="small" />
              </IconButton>
            </Stack>
            <Box sx={{ flex: 1, overflowY: "auto", border: 1, borderColor: "divider", borderRadius: 1 }}>
              {versionDetail && <RichTextEditor content={versionDetail.content} editable={false} />}
            </Box>
            <Stack direction="row" justifyContent="space-between">
              <Button size="small" onClick={() => setSelectedVersionId(null)}>
                Back to list
              </Button>
            </Stack>
          </Stack>
        )}
      </Box>

      <ConfirmDialog
        open={confirmRestore}
        title="Restore this version?"
        message="This will overwrite the current content of the page with this historical version. The current content will itself be saved as a new version."
        confirmLabel="Restore"
        loading={restoreVersion.isPending}
        onCancel={() => setConfirmRestore(false)}
        onConfirm={() => {
          if (!selectedVersionId) return;
          restoreVersion.mutate(selectedVersionId, {
            onSuccess: () => {
              setConfirmRestore(false);
              setSelectedVersionId(null);
              onClose();
            },
          });
        }}
      />
    </Drawer>
  );
}
