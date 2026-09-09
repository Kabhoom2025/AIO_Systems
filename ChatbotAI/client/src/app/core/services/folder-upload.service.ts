import { Injectable } from '@angular/core';

export interface AttachedFile {
  path: string;
  size: number;
  content: string;
}

export interface FolderReadResult {
  files: AttachedFile[];
  skippedBinary: number;
  skippedTooLarge: number;
  truncated: boolean;
}

const IGNORED_DIR_SEGMENTS = new Set([
  'node_modules', '.git', '.angular', 'dist', 'bin', 'obj', '.vs', '.vscode', 'coverage'
]);

const TEXT_EXTENSIONS = new Set([
  'ts', 'tsx', 'js', 'jsx', 'mjs', 'cjs', 'html', 'htm', 'css', 'scss', 'sass', 'less',
  'json', 'jsonc', 'md', 'mdx', 'txt', 'yml', 'yaml', 'xml', 'csv', 'py', 'cs', 'csproj',
  'sln', 'slnx', 'java', 'kt', 'go', 'rb', 'php', 'c', 'h', 'cpp', 'hpp', 'rs', 'sql',
  'sh', 'ps1', 'env', 'gitignore', 'editorconfig', 'toml', 'ini', 'config'
]);

/** Files worth including without a "real" extension. */
const TEXT_FILENAMES = new Set(['dockerfile', 'readme', 'license', 'makefile']);

const MAX_FILE_BYTES = 200 * 1024; // skip anything that looks like a huge generated/binary file
const MAX_TOTAL_CHARS = 60_000; // keep the combined attachment well within a reasonable AI context budget
const NUL_CHAR_CODE = 0;

/**
 * Reads a user-selected folder (via <input webkitdirectory>) into plain-text file contents
 * for use as one-shot chat context. Runs entirely client-side — files never touch the
 * server until the user actually sends the message they're attached to.
 */
@Injectable({ providedIn: 'root' })
export class FolderUploadService {
  async readFiles(files: File[]): Promise<FolderReadResult> {
    const result: FolderReadResult = { files: [], skippedBinary: 0, skippedTooLarge: 0, truncated: false };
    let remainingBudget = MAX_TOTAL_CHARS;

    for (const file of files) {
      const relativePath = (file as File & { webkitRelativePath?: string }).webkitRelativePath || file.name;

      if (this.isIgnoredPath(relativePath) || !this.looksLikeText(relativePath)) {
        continue;
      }

      if (file.size > MAX_FILE_BYTES) {
        result.skippedTooLarge++;
        continue;
      }

      if (remainingBudget <= 0) {
        result.truncated = true;
        break;
      }

      let content: string;
      try {
        content = await file.text();
      } catch {
        result.skippedBinary++;
        continue;
      }

      if (this.looksBinary(content)) {
        result.skippedBinary++;
        continue;
      }

      if (content.length > remainingBudget) {
        content = content.slice(0, remainingBudget) + '\n... (truncated)';
        result.truncated = true;
      }
      remainingBudget -= content.length;

      result.files.push({ path: relativePath, size: file.size, content });
    }

    return result;
  }

  /** Formats attached files as a markdown context block to prepend to the user's message. */
  buildContextBlock(files: AttachedFile[]): string {
    const parts = files.map((f) => `### ${f.path}\n\`\`\`\n${f.content}\n\`\`\``);
    return `Attached files for context:\n\n${parts.join('\n\n')}\n\n---\n\n`;
  }

  private isIgnoredPath(path: string): boolean {
    return path.split('/').some((segment) => IGNORED_DIR_SEGMENTS.has(segment.toLowerCase()));
  }

  private looksLikeText(path: string): boolean {
    const name = path.split('/').pop() ?? path;
    const withoutLeadingDot = name.startsWith('.') ? name.slice(1) : name;
    const ext = withoutLeadingDot.includes('.') ? withoutLeadingDot.split('.').pop()!.toLowerCase() : '';
    return TEXT_EXTENSIONS.has(ext) || TEXT_FILENAMES.has(name.toLowerCase()) || TEXT_FILENAMES.has(withoutLeadingDot.toLowerCase());
  }

  /**
   * A NUL byte anywhere in a text-decoded sample is a reliable signal that file.text()
   * decoded something that isn't really text (e.g. an image or compiled binary that
   * happens to carry a text-like extension).
   */
  private looksBinary(content: string): boolean {
    const sampleLength = Math.min(content.length, 4000);
    for (let i = 0; i < sampleLength; i++) {
      if (content.charCodeAt(i) === NUL_CHAR_CODE) {
        return true;
      }
    }
    return false;
  }
}
