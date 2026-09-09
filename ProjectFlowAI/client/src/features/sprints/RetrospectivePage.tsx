import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  IconButton,
  Skeleton,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import {
  useAddRetrospectiveNote,
  useDeleteRetrospectiveNote,
  useRetrospective,
} from "../../hooks/useRetrospective";
import { useSprint } from "../../hooks/useSprints";
import { RETRO_CATEGORIES, RETRO_CATEGORY_LABELS, type RetroCategory } from "../../types";

const CATEGORY_COLOR: Record<RetroCategory, string> = {
  WentWell: "#DFF7E9",
  WentWrong: "#FDE3E3",
  ActionItem: "#E3E8FD",
};

const CATEGORY_COLOR_DARK: Record<RetroCategory, string> = {
  WentWell: "#1E3B2D",
  WentWrong: "#3C2323",
  ActionItem: "#232A44",
};

function StickyNote({
  text,
  authorName,
  onDelete,
  dark,
  category,
}: {
  text: string;
  authorName: string;
  onDelete: () => void;
  dark: boolean;
  category: RetroCategory;
}) {
  return (
    <Card
      variant="outlined"
      sx={{
        bgcolor: dark ? CATEGORY_COLOR_DARK[category] : CATEGORY_COLOR[category],
        borderRadius: 1.5,
      }}
    >
      <CardContent sx={{ p: 1.5, "&:last-child": { pb: 1.5 } }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={1}>
          <Typography variant="body2" sx={{ whiteSpace: "pre-wrap", flex: 1 }}>
            {text}
          </Typography>
          <IconButton size="small" onClick={onDelete}>
            <DeleteOutlineIcon fontSize="small" />
          </IconButton>
        </Stack>
        <Typography variant="caption" color="text.secondary">
          — {authorName}
        </Typography>
      </CardContent>
    </Card>
  );
}

function RetroColumn({
  category,
  sprintId,
  notes,
  isDark,
}: {
  category: RetroCategory;
  sprintId: string;
  notes: { id: string; text: string; createdByName: string }[];
  isDark: boolean;
}) {
  const [text, setText] = useState("");
  const addNote = useAddRetrospectiveNote(sprintId);
  const deleteNote = useDeleteRetrospectiveNote(sprintId);

  const submit = () => {
    if (!text.trim()) return;
    addNote.mutate({ category, text: text.trim() }, { onSuccess: () => setText("") });
  };

  return (
    <Stack spacing={1.5}>
      <Typography variant="subtitle1" fontWeight={700}>
        {RETRO_CATEGORY_LABELS[category]}
      </Typography>
      <Stack direction="row" spacing={1}>
        <TextField
          size="small"
          fullWidth
          multiline
          minRows={2}
          placeholder="Add a note..."
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <Button
          variant="outlined"
          size="small"
          startIcon={<AddIcon fontSize="small" />}
          disabled={!text.trim() || addNote.isPending}
          onClick={submit}
        >
          Add
        </Button>
      </Stack>
      <Stack spacing={1.5}>
        {notes.map((n) => (
          <StickyNote
            key={n.id}
            text={n.text}
            authorName={n.createdByName}
            category={category}
            dark={isDark}
            onDelete={() => deleteNote.mutate(n.id)}
          />
        ))}
        {notes.length === 0 && (
          <EmptyState title="No notes yet" description="Be the first to add one." />
        )}
      </Stack>
    </Stack>
  );
}

export function RetrospectivePage() {
  const { projectId, sprintId } = useParams<{ projectId: string; sprintId: string }>();
  const navigate = useNavigate();
  const { data: sprint } = useSprint(sprintId);
  const { data, isLoading } = useRetrospective(sprintId);

  const notes = data?.notes ?? [];
  const isDark =
    typeof window !== "undefined" && window.matchMedia?.("(prefers-color-scheme: dark)").matches;

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" spacing={1.5}>
        <IconButton
          aria-label="Back to sprint board"
          onClick={() => navigate(`/projects/${projectId}/sprints/${sprintId}/board`)}
        >
          <ArrowBackIcon />
        </IconButton>
        {sprint ? (
          <Typography variant="h6" fontWeight={700}>
            {sprint.name} — Retrospective
          </Typography>
        ) : (
          <Skeleton variant="text" width={220} height={32} />
        )}
      </Stack>

      {isLoading ? (
        <Box>
          <Skeleton variant="rounded" height={280} />
        </Box>
      ) : (
        <Grid container spacing={2}>
          {RETRO_CATEGORIES.map((cat) => (
            <Grid key={cat} size={{ xs: 12, md: 4 }}>
              <RetroColumn
                category={cat}
                sprintId={sprintId ?? ""}
                notes={notes.filter((n) => n.category === cat)}
                isDark={isDark}
              />
            </Grid>
          ))}
        </Grid>
      )}
    </Stack>
  );
}
