import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import { Avatar, Box, IconButton, Stack, Typography } from "@mui/material";
import { formatDistanceToNow } from "date-fns";
import { useState } from "react";
import { EmptyState } from "../../components/EmptyState";
import { useAuthStore } from "../../store/authStore";
import {
  useAddDocPageComment,
  useDocPageComments,
  useRemoveDocPageComment,
} from "../../hooks/useDocPageComments";
import { MentionCommentBox } from "../projects/MentionCommentBox";

interface DocPageCommentsProps {
  pageId: string;
}

export function DocPageComments({ pageId }: DocPageCommentsProps) {
  const { data: comments } = useDocPageComments(pageId);
  const addComment = useAddDocPageComment(pageId);
  const removeComment = useRemoveDocPageComment(pageId);
  const currentUserId = useAuthStore((s) => s.user?.id);
  const [busyId, setBusyId] = useState<string | null>(null);

  return (
    <Stack spacing={2}>
      <Typography variant="subtitle2">Comments</Typography>

      {comments && comments.length === 0 && (
        <EmptyState title="No comments yet" description="Start the discussion on this page." />
      )}

      <Stack spacing={1.5}>
        {(comments ?? []).map((c) => (
          <Stack key={c.id} direction="row" spacing={1.5}>
            <Avatar sx={{ width: 28, height: 28, fontSize: 12 }}>
              {c.authorName.slice(0, 2).toUpperCase()}
            </Avatar>
            <Box sx={{ flex: 1 }}>
              <Stack direction="row" spacing={1} alignItems="baseline">
                <Typography variant="body2" fontWeight={600}>
                  {c.authorName}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {formatDistanceToNow(new Date(c.createdAt), { addSuffix: true })}
                </Typography>
              </Stack>
              <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                {c.body}
              </Typography>
            </Box>
            {c.authorUserId === currentUserId && (
              <IconButton
                size="small"
                disabled={busyId === c.id && removeComment.isPending}
                onClick={() => {
                  setBusyId(c.id);
                  removeComment.mutate(c.id);
                }}
              >
                <DeleteOutlineIcon fontSize="small" />
              </IconButton>
            )}
          </Stack>
        ))}
      </Stack>

      <MentionCommentBox
        members={[]}
        submitting={addComment.isPending}
        placeholder="Add a comment..."
        onSubmit={(body) => addComment.mutate({ body })}
      />
    </Stack>
  );
}
