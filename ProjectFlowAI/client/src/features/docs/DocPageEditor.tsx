import { Button, MenuItem, Stack, TextField } from "@mui/material";
import { useState } from "react";
import {
  DOC_PAGE_CATEGORIES,
  DOC_PAGE_CATEGORY_LABELS,
  type DocPageCategory,
  type DocPageDetail,
} from "../../types";
import { RichTextEditor } from "./RichTextEditor";
import { useUploadDocImage } from "../../hooks/useDocPages";

interface DocPageEditorProps {
  page: DocPageDetail;
  saving?: boolean;
  onSave: (values: { title: string; content: string; category: DocPageCategory | null }) => void;
  onCancel: () => void;
}

export function DocPageEditor({ page, saving, onSave, onCancel }: DocPageEditorProps) {
  const [title, setTitle] = useState(page.title);
  const [content, setContent] = useState(page.content);
  const [category, setCategory] = useState<DocPageCategory | "">(page.category ?? "");
  const uploadImage = useUploadDocImage(page.id);

  const handleSave = () => {
    onSave({ title: title.trim() || "Untitled", content, category: category || null });
  };

  return (
    <Stack sx={{ flex: 1, minWidth: 0 }} spacing={2} p={3}>
      <Stack direction="row" spacing={2}>
        <TextField
          label="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          fullWidth
        />
        <TextField
          select
          label="Category"
          value={category}
          onChange={(e) => setCategory(e.target.value as DocPageCategory | "")}
          sx={{ minWidth: 220 }}
        >
          <MenuItem value="">None</MenuItem>
          {DOC_PAGE_CATEGORIES.map((c) => (
            <MenuItem key={c} value={c}>
              {DOC_PAGE_CATEGORY_LABELS[c]}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      <RichTextEditor
        content={content}
        onChange={setContent}
        onUploadImage={(file) => uploadImage.mutateAsync(file).then((r) => r.url)}
        minHeight={360}
      />

      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button variant="contained" onClick={handleSave} disabled={saving}>
          Save
        </Button>
      </Stack>
    </Stack>
  );
}
