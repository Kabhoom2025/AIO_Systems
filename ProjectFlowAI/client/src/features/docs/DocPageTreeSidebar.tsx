import AddIcon from "@mui/icons-material/Add";
import ArticleOutlinedIcon from "@mui/icons-material/ArticleOutlined";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Box,
  Collapse,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import { useMemo, useState } from "react";
import type { DocPageSummary } from "../../types";

interface TreeNode extends DocPageSummary {
  children: TreeNode[];
}

function buildTree(pages: DocPageSummary[]): TreeNode[] {
  const byId = new Map<string, TreeNode>();
  pages.forEach((p) => byId.set(p.id, { ...p, children: [] }));
  const roots: TreeNode[] = [];
  byId.forEach((node) => {
    if (node.parentPageId && byId.has(node.parentPageId)) {
      byId.get(node.parentPageId)!.children.push(node);
    } else {
      roots.push(node);
    }
  });
  return roots;
}

function TreeItem({
  node,
  depth,
  selectedId,
  onSelect,
  onAddChild,
}: {
  node: TreeNode;
  depth: number;
  selectedId?: string;
  onSelect: (id: string) => void;
  onAddChild: (parentId: string) => void;
}) {
  const [open, setOpen] = useState(true);
  const hasChildren = node.children.length > 0;

  return (
    <>
      <ListItemButton
        selected={node.id === selectedId}
        onClick={() => onSelect(node.id)}
        sx={{ pl: 1.5 + depth * 2, borderRadius: 1, py: 0.5 }}
      >
        <Box
          sx={{ width: 20, display: "flex", alignItems: "center", justifyContent: "center" }}
          onClick={(e) => {
            e.stopPropagation();
            setOpen((v) => !v);
          }}
        >
          {hasChildren ? (
            open ? (
              <ExpandMoreIcon fontSize="small" />
            ) : (
              <ChevronRightIcon fontSize="small" />
            )
          ) : null}
        </Box>
        <ListItemIcon sx={{ minWidth: 28 }}>
          <ArticleOutlinedIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText
          primary={node.title}
          slotProps={{ primary: { noWrap: true, fontSize: 14 } }}
        />
        <Tooltip title="Add sub-page">
          <IconButton
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              onAddChild(node.id);
            }}
          >
            <AddIcon fontSize="inherit" />
          </IconButton>
        </Tooltip>
      </ListItemButton>
      {hasChildren && (
        <Collapse in={open}>
          {node.children.map((child) => (
            <TreeItem
              key={child.id}
              node={child}
              depth={depth + 1}
              selectedId={selectedId}
              onSelect={onSelect}
              onAddChild={onAddChild}
            />
          ))}
        </Collapse>
      )}
    </>
  );
}

interface DocPageTreeSidebarProps {
  pages: DocPageSummary[];
  selectedId?: string;
  onSelect: (id: string) => void;
  onCreateTopLevel: () => void;
  onCreateChild: (parentId: string) => void;
}

export function DocPageTreeSidebar({
  pages,
  selectedId,
  onSelect,
  onCreateTopLevel,
  onCreateChild,
}: DocPageTreeSidebarProps) {
  const tree = useMemo(() => buildTree(pages), [pages]);

  return (
    <Box sx={{ width: 280, flexShrink: 0, borderRight: 1, borderColor: "divider", pr: 1 }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 1, py: 1 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Pages
        </Typography>
        <Tooltip title="New page">
          <IconButton size="small" onClick={onCreateTopLevel}>
            <AddIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>
      <List dense sx={{ maxHeight: "calc(100vh - 260px)", overflowY: "auto" }}>
        {tree.map((node) => (
          <TreeItem
            key={node.id}
            node={node}
            depth={0}
            selectedId={selectedId}
            onSelect={onSelect}
            onAddChild={onCreateChild}
          />
        ))}
      </List>
    </Box>
  );
}
