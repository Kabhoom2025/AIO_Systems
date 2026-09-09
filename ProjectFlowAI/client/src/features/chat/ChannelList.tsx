import AddIcon from "@mui/icons-material/Add";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import TagIcon from "@mui/icons-material/Tag";
import {
  Autocomplete,
  Badge,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useAuthStore } from "../../store/authStore";
import { useChatChannels, useChatConversations, useCreateChatChannel, useCreateConversation } from "../../hooks/useChatChannels";
import { useUsers } from "../../hooks/useUsers";
import { useChatSocket } from "../../providers/ChatSocketProvider";
import type { ChatThreadRef, User } from "../../types";

interface ChannelListProps {
  selected: ChatThreadRef | null;
  onSelect: (thread: ChatThreadRef, otherUserName?: string) => void;
}

export function ChannelList({ selected, onSelect }: ChannelListProps) {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";
  const { data: channels } = useChatChannels(organizationId);
  const { data: conversations } = useChatConversations(organizationId);
  const { onlineUserIds } = useChatSocket();

  const createChannel = useCreateChatChannel(organizationId);
  const createConversation = useCreateConversation(organizationId);

  const { data: usersPage } = useUsers({ organizationId, page: 1, pageSize: 100 });

  const [channelDialogOpen, setChannelDialogOpen] = useState(false);
  const [dmDialogOpen, setDmDialogOpen] = useState(false);
  const [newChannelName, setNewChannelName] = useState("");
  const [newChannelPrivate, setNewChannelPrivate] = useState(false);
  const [newChannelMembers, setNewChannelMembers] = useState<User[]>([]);
  const [dmTarget, setDmTarget] = useState<User | null>(null);

  const handleCreateChannel = () => {
    if (!newChannelName.trim()) return;
    createChannel.mutate(
      {
        organizationId,
        name: newChannelName.trim(),
        isPrivate: newChannelPrivate,
        memberUserIds: newChannelMembers.map((m) => m.id),
      },
      {
        onSuccess: (channel) => {
          setChannelDialogOpen(false);
          setNewChannelName("");
          setNewChannelPrivate(false);
          setNewChannelMembers([]);
          onSelect({ kind: "channel", id: channel.id });
        },
      }
    );
  };

  const handleStartDm = () => {
    if (!dmTarget) return;
    createConversation.mutate(
      { otherUserId: dmTarget.id },
      {
        onSuccess: (conversation) => {
          setDmDialogOpen(false);
          setDmTarget(null);
          onSelect({ kind: "conversation", id: conversation.id }, conversation.otherUserName);
        },
      }
    );
  };

  return (
    <Box sx={{ width: 280, flexShrink: 0, borderRight: 1, borderColor: "divider", display: "flex", flexDirection: "column" }}>
      <Box sx={{ flex: 1, overflowY: "auto" }}>
        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, py: 1.5 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Channels
          </Typography>
          <Tooltip title="New channel">
            <IconButton size="small" onClick={() => setChannelDialogOpen(true)}>
              <AddIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
        <List dense disablePadding>
          {(channels ?? []).map((c) => (
            <ListItemButton
              key={c.id}
              selected={selected?.kind === "channel" && selected.id === c.id}
              onClick={() => onSelect({ kind: "channel", id: c.id })}
              sx={{ px: 2 }}
            >
              <ListItemIcon sx={{ minWidth: 28 }}>
                {c.isPrivate ? <LockOutlinedIcon fontSize="small" /> : <TagIcon fontSize="small" />}
              </ListItemIcon>
              <ListItemText primary={c.name} slotProps={{ primary: { noWrap: true } }} />
              {c.unreadCount > 0 && (
                <Badge badgeContent={c.unreadCount} color="error" max={99} sx={{ mr: 1 }} />
              )}
            </ListItemButton>
          ))}
        </List>

        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, py: 1.5, mt: 1 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Direct messages
          </Typography>
          <Tooltip title="New direct message">
            <IconButton size="small" onClick={() => setDmDialogOpen(true)}>
              <AddIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
        <List dense disablePadding>
          {(conversations ?? []).map((c) => {
            const online = onlineUserIds.has(c.otherUserId) || c.otherUserOnline;
            return (
              <ListItemButton
                key={c.id}
                selected={selected?.kind === "conversation" && selected.id === c.id}
                onClick={() => onSelect({ kind: "conversation", id: c.id }, c.otherUserName)}
                sx={{ px: 2 }}
              >
                <ListItemIcon sx={{ minWidth: 28 }}>
                  <Box
                    sx={{
                      width: 8,
                      height: 8,
                      borderRadius: "50%",
                      bgcolor: online ? "success.main" : "text.disabled",
                    }}
                  />
                </ListItemIcon>
                <ListItemText primary={c.otherUserName} slotProps={{ primary: { noWrap: true } }} />
                {c.unreadCount > 0 && (
                  <Badge badgeContent={c.unreadCount} color="error" max={99} sx={{ mr: 1 }} />
                )}
              </ListItemButton>
            );
          })}
        </List>
      </Box>

      <Dialog open={channelDialogOpen} onClose={() => setChannelDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>New channel</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 0.5 }}>
            <TextField
              autoFocus
              label="Channel name"
              value={newChannelName}
              onChange={(e) => setNewChannelName(e.target.value)}
              fullWidth
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={newChannelPrivate}
                  onChange={(e) => setNewChannelPrivate(e.target.checked)}
                />
              }
              label="Private channel"
            />
            <Autocomplete
              multiple
              options={usersPage?.items ?? []}
              getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
              value={newChannelMembers}
              onChange={(_, value) => setNewChannelMembers(value)}
              renderInput={(params) => <TextField {...params} label="Members" size="small" />}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setChannelDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" disabled={createChannel.isPending} onClick={handleCreateChannel}>
            Create
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={dmDialogOpen} onClose={() => setDmDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>New direct message</DialogTitle>
        <DialogContent>
          <Autocomplete
            options={(usersPage?.items ?? []).filter((u) => u.id !== user?.id)}
            getOptionLabel={(u) => `${u.firstName} ${u.lastName}`}
            value={dmTarget}
            onChange={(_, value) => setDmTarget(value)}
            sx={{ mt: 1 }}
            renderInput={(params) => <TextField {...params} label="Person" autoFocus size="small" />}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDmDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" disabled={!dmTarget || createConversation.isPending} onClick={handleStartDm}>
            Start
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
