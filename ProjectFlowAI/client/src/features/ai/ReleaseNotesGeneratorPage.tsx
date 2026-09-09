import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { useGenerateReleaseNotes } from "../../hooks/useGenerateReleaseNotes";
import { AiUnavailableState } from "./AiUnavailableState";

function today() {
  return new Date().toISOString().slice(0, 10);
}

function thirtyDaysAgo() {
  const d = new Date();
  d.setDate(d.getDate() - 30);
  return d.toISOString().slice(0, 10);
}

export function ReleaseNotesGeneratorPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const [from, setFrom] = useState(thirtyDaysAgo());
  const [to, setTo] = useState(today());
  const [saveAsWikiPage, setSaveAsWikiPage] = useState(true);

  const generate = useGenerateReleaseNotes();

  const handleGenerate = () => {
    if (!projectId) return;
    generate.mutate({ projectId, from, to, saveAsWikiPage });
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Release Notes Generator
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Generate polished release notes for a date range, ready to publish to the wiki.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <Stack direction="row" spacing={2}>
              <TextField
                label="From"
                type="date"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <TextField
                label="To"
                type="date"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Stack>
            <FormControlLabel
              control={
                <Checkbox checked={saveAsWikiPage} onChange={(e) => setSaveAsWikiPage(e.target.checked)} />
              }
              label="Save as a wiki page"
            />
            <Box>
              <Button
                variant="contained"
                startIcon={
                  generate.isPending ? <CircularProgress size={16} color="inherit" /> : <DescriptionOutlinedIcon />
                }
                disabled={generate.isPending}
                onClick={handleGenerate}
              >
                {generate.isPending ? "Generating..." : "Generate release notes"}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {generate.isError && <AiUnavailableState error={generate.error} />}

      {generate.data && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              {generate.data.wikiPageId && <Alert severity="success">Saved as a wiki page.</Alert>}
              <Box
                component="pre"
                sx={{
                  whiteSpace: "pre-wrap",
                  wordBreak: "break-word",
                  fontFamily: "monospace",
                  fontSize: 13,
                  m: 0,
                  p: 2,
                  bgcolor: "action.hover",
                  borderRadius: 1,
                  maxHeight: 600,
                  overflowY: "auto",
                }}
              >
                {generate.data.markdown}
              </Box>
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
