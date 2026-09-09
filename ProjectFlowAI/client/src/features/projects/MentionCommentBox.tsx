import SendIcon from "@mui/icons-material/Send";
import {
  Avatar,
  Box,
  Button,
  ClickAwayListener,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  TextField,
} from "@mui/material";
import { useMemo, useRef, useState } from "react";
import type { User } from "../../types";

interface MentionCommentBoxProps {
  members: User[];
  onSubmit: (body: string, mentionedUserIds: string[]) => void;
  submitting?: boolean;
  placeholder?: string;
}

/**
 * A comment textarea that, when the user types "@" followed by characters,
 * shows a filtered dropdown of org members. Picking one inserts a
 * "@First Last " token into the text and records the userId so it can be
 * sent alongside the comment body as mentionedUserIds.
 */
export function MentionCommentBox({
  members,
  onSubmit,
  submitting,
  placeholder = "Add a comment... use @ to mention someone",
}: MentionCommentBoxProps) {
  const [text, setText] = useState("");
  const [mentioned, setMentioned] = useState<Map<string, string>>(new Map()); // name -> userId
  const [query, setQuery] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement | null>(null);

  const suggestions = useMemo(() => {
    if (query === null) return [];
    const q = query.toLowerCase();
    return members
      .filter((m) => `${m.firstName} ${m.lastName}`.toLowerCase().includes(q))
      .slice(0, 6);
  }, [members, query]);

  const handleChange = (value: string) => {
    setText(value);
    const cursor = inputRef.current?.selectionStart ?? value.length;
    const upToCursor = value.slice(0, cursor);
    const match = /@([\w\s]{0,24})$/.exec(upToCursor);
    if (match) {
      setQuery(match[1]);
    } else {
      setQuery(null);
    }
  };

  const pickMention = (user: User) => {
    const cursor = inputRef.current?.selectionStart ?? text.length;
    const upToCursor = text.slice(0, cursor);
    const match = /@([\w\s]{0,24})$/.exec(upToCursor);
    const fullName = `${user.firstName} ${user.lastName}`;
    if (match) {
      const start = cursor - match[0].length;
      const newText = `${text.slice(0, start)}@${fullName} ${text.slice(cursor)}`;
      setText(newText);
    } else {
      setText(`${text}@${fullName} `);
    }
    setMentioned((prev) => new Map(prev).set(fullName, user.id));
    setQuery(null);
  };

  const handleSubmit = () => {
    if (!text.trim()) return;
    const usedIds = Array.from(mentioned.entries())
      .filter(([name]) => text.includes(`@${name}`))
      .map(([, id]) => id);
    onSubmit(text.trim(), usedIds);
    setText("");
    setMentioned(new Map());
    setQuery(null);
  };

  return (
    <Box sx={{ position: "relative" }}>
      <ClickAwayListener onClickAway={() => setQuery(null)}>
        <Box>
          <TextField
            inputRef={inputRef}
            fullWidth
            multiline
            minRows={2}
            placeholder={placeholder}
            value={text}
            onChange={(e) => handleChange(e.target.value)}
          />
          {query !== null && suggestions.length > 0 && (
            <Paper
              variant="outlined"
              sx={{ position: "absolute", zIndex: 10, mt: 0.5, width: 280, maxHeight: 220, overflowY: "auto" }}
            >
              <List dense>
                {suggestions.map((u) => (
                  <ListItemButton key={u.id} onClick={() => pickMention(u)}>
                    <Avatar sx={{ width: 24, height: 24, fontSize: 11, mr: 1.5 }}>
                      {u.firstName[0]}
                      {u.lastName[0]}
                    </Avatar>
                    <ListItemText
                      primary={`${u.firstName} ${u.lastName}`}
                      secondary={u.email}
                      slotProps={{ secondary: { sx: { fontSize: 11 } } }}
                    />
                  </ListItemButton>
                ))}
              </List>
            </Paper>
          )}
        </Box>
      </ClickAwayListener>
      <Stack direction="row" justifyContent="flex-end" sx={{ mt: 1 }}>
        <Button
          size="small"
          variant="contained"
          endIcon={<SendIcon fontSize="small" />}
          disabled={!text.trim() || submitting}
          onClick={handleSubmit}
        >
          Comment
        </Button>
      </Stack>
    </Box>
  );
}
