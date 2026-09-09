import AutoAwesomeOutlinedIcon from "@mui/icons-material/AutoAwesomeOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { EmptyState } from "../../components/EmptyState";
import { useGenerateTasks } from "../../hooks/useGenerateTasks";
import { useCreateWorkItem } from "../../hooks/useWorkItems";
import type { TaskSuggestion } from "../../types";
import { AiUnavailableState } from "./AiUnavailableState";

export function TaskGeneratorPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [prompt, setPrompt] = useState("");
  const [suggestions, setSuggestions] = useState<TaskSuggestion[]>([]);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [createdCount, setCreatedCount] = useState(0);

  const generate = useGenerateTasks();
  const createWorkItem = useCreateWorkItem(projectId ?? "");

  const handleGenerate = () => {
    if (!projectId || !prompt.trim()) return;
    setCreatedCount(0);
    generate.mutate(
      { projectId, prompt: prompt.trim() },
      {
        onSuccess: (data) => {
          setSuggestions(data.suggestions);
          setSelected(new Set(data.suggestions.map((_, i) => i)));
        },
      }
    );
  };

  const toggle = (index: number) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return next;
    });
  };

  const allSelected = suggestions.length > 0 && selected.size === suggestions.length;

  const handleCreateSelected = async () => {
    if (!projectId) return;
    const toCreate = suggestions.filter((_, i) => selected.has(i));
    let count = 0;
    for (const s of toCreate) {
      await createWorkItem.mutateAsync({
        projectId,
        title: s.title,
        priority: s.priority,
        type: s.type,
        storyPoints: s.storyPoints,
      });
      count += 1;
    }
    setCreatedCount(count);
    setSuggestions((prev) => prev.filter((_, i) => !selected.has(i)));
    setSelected(new Set());
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Task Generator
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Describe the work in plain English. The assistant will draft a set of candidate tasks
          you can review and selectively create.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <TextField
              label="What needs to be done?"
              placeholder="e.g. Build a password-reset flow with email verification and rate limiting"
              multiline
              minRows={4}
              fullWidth
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
            />
            <Box>
              <Button
                variant="contained"
                startIcon={
                  generate.isPending ? <CircularProgress size={16} color="inherit" /> : <AutoAwesomeOutlinedIcon />
                }
                disabled={!prompt.trim() || generate.isPending}
                onClick={handleGenerate}
              >
                {generate.isPending ? "Thinking..." : "Generate tasks"}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {createdCount > 0 && (
        <Alert severity="success" onClose={() => setCreatedCount(0)}>
          Created {createdCount} task{createdCount === 1 ? "" : "s"}.
        </Alert>
      )}

      {generate.isError && <AiUnavailableState error={generate.error} />}

      {suggestions.length > 0 && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="subtitle1" fontWeight={700}>
                  Suggestions ({suggestions.length})
                </Typography>
                <Stack direction="row" spacing={1}>
                  <Button
                    size="small"
                    onClick={() =>
                      setSelected(allSelected ? new Set() : new Set(suggestions.map((_, i) => i)))
                    }
                  >
                    {allSelected ? "Deselect all" : "Select all"}
                  </Button>
                  <Button
                    size="small"
                    variant="contained"
                    disabled={selected.size === 0 || createWorkItem.isPending}
                    onClick={handleCreateSelected}
                    startIcon={createWorkItem.isPending ? <CircularProgress size={14} color="inherit" /> : undefined}
                  >
                    Create selected ({selected.size})
                  </Button>
                </Stack>
              </Stack>

              <Stack spacing={1.5}>
                {suggestions.map((s, i) => (
                  <Stack
                    key={`${s.title}-${i}`}
                    direction="row"
                    spacing={1.5}
                    sx={{ p: 1.5, border: 1, borderColor: "divider", borderRadius: 1 }}
                  >
                    <Checkbox checked={selected.has(i)} onChange={() => toggle(i)} sx={{ mt: -0.5 }} />
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="body1" fontWeight={600}>
                        {s.title}
                      </Typography>
                      {s.description && (
                        <Typography variant="body2" color="text.secondary">
                          {s.description}
                        </Typography>
                      )}
                      <Stack direction="row" spacing={1} sx={{ mt: 0.75 }}>
                        <Chip size="small" label={s.type} variant="outlined" />
                        <Chip size="small" label={s.priority} variant="outlined" />
                        {s.storyPoints != null && (
                          <Chip size="small" label={`${s.storyPoints} pts`} variant="outlined" />
                        )}
                      </Stack>
                    </Box>
                  </Stack>
                ))}
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      )}

      {!generate.isPending && !generate.isError && suggestions.length === 0 && (
        <EmptyState
          icon={<AutoAwesomeOutlinedIcon sx={{ fontSize: 48 }} />}
          title="No suggestions yet"
          description="Describe what you need above and generate a first batch of task suggestions."
        />
      )}
    </Stack>
  );
}
