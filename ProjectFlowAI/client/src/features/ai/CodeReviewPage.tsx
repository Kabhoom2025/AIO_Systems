import CodeOutlinedIcon from "@mui/icons-material/CodeOutlined";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { EmptyState } from "../../components/EmptyState";
import { useReviewCode } from "../../hooks/useReviewCode";
import type { CodeReviewSeverity } from "../../types";
import { AiUnavailableState } from "./AiUnavailableState";

const LANGUAGES = [
  "TypeScript",
  "JavaScript",
  "C#",
  "Python",
  "Java",
  "Go",
  "SQL",
  "Other",
];

const SEVERITY_COLOR: Record<CodeReviewSeverity, "info" | "warning" | "error"> = {
  Info: "info",
  Warning: "warning",
  Critical: "error",
};

export function CodeReviewPage() {
  const [code, setCode] = useState("");
  const [language, setLanguage] = useState("TypeScript");

  const review = useReviewCode();

  const handleReview = () => {
    if (!code.trim()) return;
    review.mutate({ code, language });
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Code Review
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Paste a snippet and get line-level review comments, severity-colored.
        </Typography>
      </Box>

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <Select size="small" value={language} onChange={(e) => setLanguage(e.target.value)} sx={{ width: 220 }}>
              {LANGUAGES.map((l) => (
                <MenuItem key={l} value={l}>
                  {l}
                </MenuItem>
              ))}
            </Select>
            <TextField
              label="Code"
              placeholder="Paste code to review..."
              multiline
              minRows={12}
              fullWidth
              value={code}
              onChange={(e) => setCode(e.target.value)}
              slotProps={{ input: { sx: { fontFamily: "monospace", fontSize: 13 } } }}
            />
            <Box>
              <Button
                variant="contained"
                startIcon={review.isPending ? <CircularProgress size={16} color="inherit" /> : <CodeOutlinedIcon />}
                disabled={!code.trim() || review.isPending}
                onClick={handleReview}
              >
                {review.isPending ? "Reviewing..." : "Review code"}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {review.isError && <AiUnavailableState error={review.error} />}

      {review.data && (
        <Card variant="outlined">
          <CardContent>
            <Stack spacing={2}>
              <Box>
                <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                  Summary
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {review.data.summary}
                </Typography>
              </Box>

              {review.data.suggestions.length === 0 ? (
                <EmptyState title="No issues found" description="The assistant didn't flag anything." />
              ) : (
                <Stack spacing={1}>
                  {review.data.suggestions.map((s, i) => (
                    <Stack
                      key={i}
                      direction="row"
                      spacing={1.5}
                      sx={{ p: 1.5, border: 1, borderColor: "divider", borderRadius: 1 }}
                    >
                      <Chip size="small" label={`L${s.line}`} variant="outlined" sx={{ fontFamily: "monospace" }} />
                      <Chip size="small" label={s.severity} color={SEVERITY_COLOR[s.severity]} />
                      <Typography variant="body2" sx={{ flex: 1 }}>
                        {s.comment}
                      </Typography>
                    </Stack>
                  ))}
                </Stack>
              )}
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
