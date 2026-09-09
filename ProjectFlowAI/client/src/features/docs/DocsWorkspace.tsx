import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { useEffect, useState } from "react";
import { EmptyState } from "../../components/EmptyState";
import { useDocPage, useDocPages, useCreateDocPage, useUpdateDocPage } from "../../hooks/useDocPages";
import {
  DOC_PAGE_CATEGORIES,
  DOC_PAGE_CATEGORY_LABELS,
  type DocPageCategory,
  type DocPageScope,
} from "../../types";
import { DocPageTreeSidebar } from "./DocPageTreeSidebar";
import { DocPageView } from "./DocPageView";
import { DocPageEditor } from "./DocPageEditor";

interface DocsWorkspaceProps {
  organizationId: string;
  scope: DocPageScope;
  projectId?: string;
  /** Whether to show the category filter chip row (used by the org-wide Wiki, not by
   * per-project Documents where categories are less relevant). */
  showCategoryFilter?: boolean;
}

export function DocsWorkspace({
  organizationId,
  scope,
  projectId,
  showCategoryFilter = false,
}: DocsWorkspaceProps) {
  const [categoryFilter, setCategoryFilter] = useState<DocPageCategory | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [mode, setMode] = useState<"view" | "edit">("view");
  const [createDialog, setCreateDialog] = useState<{ parentId: string | null } | null>(null);
  const [newTitle, setNewTitle] = useState("");

  const { data: pages, isLoading: pagesLoading } = useDocPages({
    organizationId,
    projectId,
    scope,
    category: categoryFilter ?? undefined,
  });

  const { data: page, isLoading: pageLoading } = useDocPage(selectedId ?? undefined);
  const createPage = useCreateDocPage();
  const updatePage = useUpdateDocPage(selectedId ?? "");

  useEffect(() => {
    if (!selectedId && pages && pages.length > 0) {
      setSelectedId(pages[0].id);
    }
  }, [pages, selectedId]);

  const handleCreate = () => {
    if (!createDialog) return;
    createPage.mutate(
      {
        organizationId,
        projectId,
        scope,
        title: newTitle.trim() || "Untitled",
        content: "",
        parentPageId: createDialog.parentId,
      },
      {
        onSuccess: (created) => {
          setSelectedId(created.id);
          setMode("edit");
          setCreateDialog(null);
          setNewTitle("");
        },
      }
    );
  };

  return (
    <Stack sx={{ flex: 1, minHeight: 0 }} spacing={showCategoryFilter ? 1.5 : 0}>
      {showCategoryFilter && (
        <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ px: 1 }}>
          <Chip
            label="All"
            size="small"
            color={categoryFilter === null ? "primary" : "default"}
            onClick={() => setCategoryFilter(null)}
          />
          {DOC_PAGE_CATEGORIES.map((c) => (
            <Chip
              key={c}
              label={DOC_PAGE_CATEGORY_LABELS[c]}
              size="small"
              color={categoryFilter === c ? "primary" : "default"}
              onClick={() => setCategoryFilter(c)}
            />
          ))}
        </Stack>
      )}

      <Stack direction="row" sx={{ flex: 1, minHeight: 0 }}>
        <DocPageTreeSidebar
          pages={pages ?? []}
          selectedId={selectedId ?? undefined}
          onSelect={(id) => {
            setSelectedId(id);
            setMode("view");
          }}
          onCreateTopLevel={() => setCreateDialog({ parentId: null })}
          onCreateChild={(parentId) => setCreateDialog({ parentId })}
        />

        {!selectedId ? (
          !pagesLoading && (
            <Box sx={{ flex: 1 }}>
              <EmptyState
                title="No pages yet"
                description="Create your first page to get started."
                actionLabel="New page"
                onAction={() => setCreateDialog({ parentId: null })}
              />
            </Box>
          )
        ) : mode === "view" ? (
          <DocPageView
            page={page}
            isLoading={pageLoading}
            onEdit={() => setMode("edit")}
            onDeleted={() => {
              setSelectedId(null);
              setMode("view");
            }}
          />
        ) : (
          page && (
            <DocPageEditor
              page={page}
              saving={updatePage.isPending}
              onCancel={() => setMode("view")}
              onSave={(values) =>
                updatePage.mutate(
                  { ...values, parentPageId: page.parentPageId },
                  { onSuccess: () => setMode("view") }
                )
              }
            />
          )
        )}
      </Stack>

      <Dialog open={!!createDialog} onClose={() => setCreateDialog(null)} maxWidth="xs" fullWidth>
        <DialogTitle>New page</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            fullWidth
            label="Title"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateDialog(null)}>Cancel</Button>
          <Button variant="contained" disabled={createPage.isPending} onClick={handleCreate}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
