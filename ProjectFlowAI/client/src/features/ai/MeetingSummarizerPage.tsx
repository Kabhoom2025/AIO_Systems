import NotesOutlinedIcon from "@mui/icons-material/NotesOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { useSummarizeMeeting } from "../../hooks/useSummarizeMeeting";
import { AiUnavailableState } from "./AiUnavailableState";

export function MeetingSummarizerPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [rawNotes, setRawNotes] = useState("");
  const [saveAsWikiPage, setSaveAsWikiPage] = useState(true);

  const summarize = useSummarizeMeeting();

  const handleSummarize = () => {
    if (!projectId || !rawNotes.trim()) return;
    summarize.mutate({ rawNotes: rawNotes.trim(), projectId, saveAsWikiPage });
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Meeting Summarizer
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Paste raw meeting notes and get a clean summary with extracted action items.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <TextField
              label="Raw meeting notes"
              placeholder="Paste your notes, transcript, or bullet points here..."
              multiline
              minRows={8}
              fullWidth
              value={rawNotes}
              onChange={(e) => setRawNotes(e.target.value)}
            />
            <FormControlLabel
              control={
                <Checkbox checked={saveAsWikiPage} onChange={(e) => setSaveAsWikiPage(e.target.checked)} />
              }
              label="Save summary as a wiki page"
            />
            <Box>
              <Button
                variant="contained"
                startIcon={
                  summarize.isPending ? <CircularProgress size={16} color="inherit" /> : <NotesOutlinedIcon />
                }
                disabled={!rawNotes.trim() || summarize.isPending}
                onClick={handleSummarize}
              >
                {summarize.isPending ? "Summarizing..." : "Summarize"}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {summarize.isError && <AiUnavailableState error={summarize.error} />}

      {summarize.data && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              <Box>
                <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                  Summary
                </Typography>
                <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                  {summarize.data.summary}
                </Typography>
              </Box>

              {summarize.data.actionItems.length > 0 && (
                <Box>
                  <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                    Action items
                  </Typography>
                  <List dense>
                    {summarize.data.actionItems.map((a, i) => (
                      <ListItem key={i} disableGutters>
                        <ListItemIcon sx={{ minWidth: 32 }}>
                          <CheckCircleOutlineIcon fontSize="small" color="primary" />
                        </ListItemIcon>
                        <ListItemText primary={a.text} />
                      </ListItem>
                    ))}
                  </List>
                </Box>
              )}

              {summarize.data.wikiPageId && (
                <Alert severity="success">Saved as a wiki page.</Alert>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
