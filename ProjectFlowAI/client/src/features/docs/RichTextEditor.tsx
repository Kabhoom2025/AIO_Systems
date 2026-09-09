import FormatBoldIcon from "@mui/icons-material/FormatBold";
import FormatItalicIcon from "@mui/icons-material/FormatItalic";
import StrikethroughSIcon from "@mui/icons-material/StrikethroughS";
import FormatListBulletedIcon from "@mui/icons-material/FormatListBulleted";
import FormatListNumberedIcon from "@mui/icons-material/FormatListNumbered";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";
import CodeIcon from "@mui/icons-material/Code";
import LinkIcon from "@mui/icons-material/Link";
import ImageOutlinedIcon from "@mui/icons-material/ImageOutlined";
import TableChartOutlinedIcon from "@mui/icons-material/TableChartOutlined";
import UndoIcon from "@mui/icons-material/Undo";
import RedoIcon from "@mui/icons-material/Redo";
import { Box, Divider, IconButton, Stack, ToggleButton, Tooltip } from "@mui/material";
import { EditorContent, useEditor, type Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import Image from "@tiptap/extension-image";
import Link from "@tiptap/extension-link";
import Placeholder from "@tiptap/extension-placeholder";
import { Table } from "@tiptap/extension-table";
import TableRow from "@tiptap/extension-table-row";
import TableCell from "@tiptap/extension-table-cell";
import TableHeader from "@tiptap/extension-table-header";
import { useEffect, useRef } from "react";

interface RichTextEditorProps {
  content: string;
  editable?: boolean;
  placeholder?: string;
  onChange?: (html: string) => void;
  onBlur?: (html: string) => void;
  /** Called when the user picks an image file via the toolbar button; should upload it and
   * resolve to the hosted URL to insert into the document. */
  onUploadImage?: (file: File) => Promise<string>;
  minHeight?: number;
}

function ToolbarButton({
  active,
  disabled,
  onClick,
  title,
  children,
}: {
  active?: boolean;
  disabled?: boolean;
  onClick: () => void;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <Tooltip title={title}>
      <span>
        <ToggleButton
          value={title}
          selected={!!active}
          disabled={disabled}
          onClick={onClick}
          size="small"
          sx={{ border: "none", p: 0.75 }}
        >
          {children}
        </ToggleButton>
      </span>
    </Tooltip>
  );
}

function EditorToolbar({
  editor,
  onUploadImage,
}: {
  editor: Editor;
  onUploadImage?: (file: File) => Promise<string>;
}) {
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const handleImagePick = () => fileInputRef.current?.click();

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file || !onUploadImage) return;
    const url = await onUploadImage(file);
    editor.chain().focus().setImage({ src: url }).run();
  };

  const setLink = () => {
    const previousUrl = editor.getAttributes("link").href as string | undefined;
    const url = window.prompt("Link URL", previousUrl ?? "https://");
    if (url === null) return;
    if (url === "") {
      editor.chain().focus().extendMarkRange("link").unsetLink().run();
      return;
    }
    editor.chain().focus().extendMarkRange("link").setLink({ href: url }).run();
  };

  return (
    <Stack
      direction="row"
      alignItems="center"
      flexWrap="wrap"
      sx={{
        px: 1,
        py: 0.5,
        borderBottom: 1,
        borderColor: "divider",
        bgcolor: "background.default",
        rowGap: 0.5,
      }}
    >
      <ToolbarButton
        title="Bold"
        active={editor.isActive("bold")}
        onClick={() => editor.chain().focus().toggleBold().run()}
      >
        <FormatBoldIcon fontSize="small" />
      </ToolbarButton>
      <ToolbarButton
        title="Italic"
        active={editor.isActive("italic")}
        onClick={() => editor.chain().focus().toggleItalic().run()}
      >
        <FormatItalicIcon fontSize="small" />
      </ToolbarButton>
      <ToolbarButton
        title="Strikethrough"
        active={editor.isActive("strike")}
        onClick={() => editor.chain().focus().toggleStrike().run()}
      >
        <StrikethroughSIcon fontSize="small" />
      </ToolbarButton>
      <ToolbarButton
        title="Code"
        active={editor.isActive("code")}
        onClick={() => editor.chain().focus().toggleCode().run()}
      >
        <CodeIcon fontSize="small" />
      </ToolbarButton>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5, my: 0.5 }} />
      <ToolbarButton
        title="Heading 1"
        active={editor.isActive("heading", { level: 1 })}
        onClick={() => editor.chain().focus().toggleHeading({ level: 1 }).run()}
      >
        H1
      </ToolbarButton>
      <ToolbarButton
        title="Heading 2"
        active={editor.isActive("heading", { level: 2 })}
        onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}
      >
        H2
      </ToolbarButton>
      <ToolbarButton
        title="Heading 3"
        active={editor.isActive("heading", { level: 3 })}
        onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}
      >
        H3
      </ToolbarButton>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5, my: 0.5 }} />
      <ToolbarButton
        title="Bulleted list"
        active={editor.isActive("bulletList")}
        onClick={() => editor.chain().focus().toggleBulletList().run()}
      >
        <FormatListBulletedIcon fontSize="small" />
      </ToolbarButton>
      <ToolbarButton
        title="Numbered list"
        active={editor.isActive("orderedList")}
        onClick={() => editor.chain().focus().toggleOrderedList().run()}
      >
        <FormatListNumberedIcon fontSize="small" />
      </ToolbarButton>
      <ToolbarButton
        title="Quote"
        active={editor.isActive("blockquote")}
        onClick={() => editor.chain().focus().toggleBlockquote().run()}
      >
        <FormatQuoteIcon fontSize="small" />
      </ToolbarButton>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5, my: 0.5 }} />
      <ToolbarButton title="Link" active={editor.isActive("link")} onClick={setLink}>
        <LinkIcon fontSize="small" />
      </ToolbarButton>
      <Tooltip title="Insert image">
        <span>
          <IconButton size="small" onClick={handleImagePick} disabled={!onUploadImage}>
            <ImageOutlinedIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        hidden
        onChange={handleFileChange}
      />
      <Tooltip title="Insert table">
        <IconButton
          size="small"
          onClick={() =>
            editor.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()
          }
        >
          <TableChartOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      <Divider orientation="vertical" flexItem sx={{ mx: 0.5, my: 0.5 }} />
      <Tooltip title="Undo">
        <span>
          <IconButton size="small" onClick={() => editor.chain().focus().undo().run()}>
            <UndoIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
      <Tooltip title="Redo">
        <span>
          <IconButton size="small" onClick={() => editor.chain().focus().redo().run()}>
            <RedoIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
    </Stack>
  );
}

/**
 * Reusable TipTap-based rich text editor. `content` is an HTML string in both directions,
 * matching the backend's doc-page `content` field. Renders a read-only view (no toolbar, no
 * border) when `editable` is false.
 */
export function RichTextEditor({
  content,
  editable = true,
  placeholder = "Start writing...",
  onChange,
  onBlur,
  onUploadImage,
  minHeight = 240,
}: RichTextEditorProps) {
  const editor = useEditor({
    extensions: [
      StarterKit,
      Image,
      Link.configure({ openOnClick: false }),
      Placeholder.configure({ placeholder }),
      Table.configure({ resizable: true }),
      TableRow,
      TableHeader,
      TableCell,
    ],
    content,
    editable,
    onUpdate: ({ editor: e }) => onChange?.(e.getHTML()),
    onBlur: ({ editor: e }) => onBlur?.(e.getHTML()),
    editorProps: {
      attributes: {
        style: `min-height: ${minHeight}px; padding: 12px 16px; outline: none;`,
      },
    },
  });

  // Keep the editor's content in sync when the `content` prop changes from outside (e.g.
  // switching pages, or restoring a historical version) without fighting the user's own typing.
  useEffect(() => {
    if (!editor) return;
    if (editor.isFocused) return;
    const current = editor.getHTML();
    if (current !== content) {
      editor.commands.setContent(content, { emitUpdate: false });
    }
  }, [content, editor]);

  useEffect(() => {
    editor?.setEditable(editable);
  }, [editable, editor]);

  if (!editor) return null;

  return (
    <Box
      sx={{
        border: editable ? 1 : 0,
        borderColor: "divider",
        borderRadius: editable ? 1 : 0,
        overflow: "hidden",
        "& .ProseMirror": {
          minHeight,
        },
        "& .ProseMirror p.is-editor-empty:first-of-type::before": {
          content: "attr(data-placeholder)",
          color: "text.disabled",
          float: "left",
          height: 0,
          pointerEvents: "none",
        },
        "& .ProseMirror table": {
          borderCollapse: "collapse",
          width: "100%",
          my: 1.5,
        },
        "& .ProseMirror td, & .ProseMirror th": {
          border: "1px solid",
          borderColor: "divider",
          padding: "6px 10px",
          position: "relative",
        },
        "& .ProseMirror th": {
          bgcolor: "action.hover",
          fontWeight: 600,
        },
        "& .ProseMirror img": {
          maxWidth: "100%",
          borderRadius: 1,
        },
        "& .ProseMirror blockquote": {
          borderLeft: "3px solid",
          borderColor: "primary.main",
          pl: 2,
          ml: 0,
          color: "text.secondary",
        },
        "& .ProseMirror pre": {
          bgcolor: "action.hover",
          p: 1.5,
          borderRadius: 1,
          overflowX: "auto",
        },
      }}
    >
      {editable && <EditorToolbar editor={editor} onUploadImage={onUploadImage} />}
      <EditorContent editor={editor} />
    </Box>
  );
}
