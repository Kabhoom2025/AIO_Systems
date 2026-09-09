import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import HistoryOutlinedIcon from "@mui/icons-material/HistoryOutlined";
import { Button, Chip, Divider, IconButton, Skeleton, Stack, Tooltip, Typography } from "@mui/material";
import { useState } from "react";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { DOC_PAGE_CATEGORY_LABELS, type DocPageDetail } from "../../types";
import { RichTextEditor } from "./RichTextEditor";
import { DocPageComments } from "./DocPageComments";
import { VersionHistoryPanel } from "./VersionHistoryPanel";
import { useDeleteDocPage } from "../../hooks/useDocPages";

interface DocPageViewProps {
  page: DocPageDetail | undefined;
  isLoading: boolean;
  onEdit: () => void;
  onDeleted: () => void;
}

export function DocPageView({ page, isLoading, onEdit, onDeleted }: DocPageViewProps) {
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [historyOpen, setHistoryOpen] = useState(false);
  const deletePage = useDeleteDocPage();

  if (isLoading || !page) {
    return (
      <Stack spacing={2} sx={{ flex: 1, p: 3 }}>
        <Skeleton variant="text" width="40%" height={40} />
        <Skeleton variant="rectangular" height={240} />
      </Stack>
    );
  }

  return (
    <Stack sx={{ flex: 1, minWidth: 0 }} spacing={2} p={3}>
      <Stack direction="row" alignItems="flex-start" justifyContent="space-between">
        <Stack spacing={0.5}>
          <Typography variant="h5" fontWeight={700}>
            {page.title}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center">
            {page.category && (
              <Chip size="small" label={DOC_PAGE_CATEGORY_LABELS[page.category]} variant="outlined" />
            )}
            <Typography variant="caption" color="text.secondary">
              Updated by {page.updatedByName} on {new Date(page.updatedAt).toLocaleString()} ·{" "}
              {page.versionCount} version{page.versionCount === 1 ? "" : "s"}
            </Typography>
          </Stack>
        </Stack>
        <Stack direction="row" spacing={1}>
          <Tooltip title="Version history">
            <IconButton onClick={() => setHistoryOpen(true)}>
              <HistoryOutlinedIcon />
            </IconButton>
          </Tooltip>
          <Button variant="outlined" startIcon={<EditOutlinedIcon />} onClick={onEdit}>
            Edit
          </Button>
          <Tooltip title="Delete page">
            <IconButton color="error" onClick={() => setConfirmDelete(true)}>
              <DeleteOutlineIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      </Stack>

      <RichTextEditor content={page.content} editable={false} />

      <Divider />

      <DocPageComments pageId={page.id} />

      <VersionHistoryPanel pageId={page.id} open={historyOpen} onClose={() => setHistoryOpen(false)} />

      <ConfirmDialog
        open={confirmDelete}
        title="Delete this page?"
        message={`"${page.title}" and its version history will be permanently removed. This can't be undone.`}
        confirmLabel="Delete"
        destructive
        loading={deletePage.isPending}
        onCancel={() => setConfirmDelete(false)}
        onConfirm={() =>
          deletePage.mutate(page.id, {
            onSuccess: () => {
              setConfirmDelete(false);
              onDeleted();
            },
          })
        }
      />
    </Stack>
  );
}
