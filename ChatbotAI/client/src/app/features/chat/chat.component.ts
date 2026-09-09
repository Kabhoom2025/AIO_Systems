import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { DatePipe } from '@angular/common';
import { v4 as uuid } from '../../shared/util/uuid';
import { ChatMessage } from '../../core/models/message.model';
import { ConversationService } from '../../core/services/conversation.service';
import { ChatService } from '../../core/services/chat.service';
import { VoiceRecognitionService } from '../../core/services/voice-recognition.service';
import { TextToSpeechService } from '../../core/services/text-to-speech.service';
import { SettingsService } from '../../core/services/settings.service';
import { MarkdownPipe } from '../../shared/pipes/markdown.pipe';
import { CodeCopyDirective } from '../../shared/directives/code-copy.directive';
import { VoiceConversationComponent } from './voice-conversation/voice-conversation.component';
import { AttachedFile, FolderUploadService } from '../../core/services/folder-upload.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    FormsModule,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatMenuModule,
    MarkdownPipe,
    CodeCopyDirective
  ],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  readonly conversationId = signal<string | null>(null);
  readonly messages = signal<ChatMessage[]>([]);
  readonly inputText = signal('');
  readonly isLoadingConversation = signal(false);
  readonly isGenerating = signal(false);
  readonly isAwaitingFirstChunk = signal(false);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly conversationService = inject(ConversationService);
  private readonly chatService = inject(ChatService);
  private readonly voiceRecognitionService = inject(VoiceRecognitionService);
  private readonly textToSpeechService = inject(TextToSpeechService);
  private readonly settingsService = inject(SettingsService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly folderUploadService = inject(FolderUploadService);

  readonly attachedFiles = signal<AttachedFile[]>([]);
  readonly isReadingFolder = signal(false);
  readonly attachedFileCount = computed(() => this.attachedFiles().length);
  readonly attachedTotalSize = computed(() => this.attachedFiles().reduce((sum, f) => sum + f.size, 0));

  readonly isListening = this.voiceRecognitionService.isListening;
  readonly interimTranscript = this.voiceRecognitionService.interimTranscript;
  readonly isVoiceSupported = this.voiceRecognitionService.isSupported;
  readonly isTtsSupported = this.textToSpeechService.isSupported;
  readonly isSpeaking = this.textToSpeechService.isSpeaking;
  readonly connectionState = this.chatService.connectionState;

  readonly hasMessages = computed(() => this.messages().length > 0);

  private streamingMessageId: string | null = null;
  private readonly subscriptions = new Subscription();

  ngOnInit(): void {
    this.subscriptions.add(
      this.route.paramMap.subscribe((params) => {
        const id = params.get('conversationId');
        this.conversationId.set(id);
        this.textToSpeechService.stop();
        if (id) {
          this.loadConversation(id);
        } else {
          this.messages.set([]);
        }
      })
    );

    this.chatService.connect().catch(() =>
      this.snackBar.open('Unable to connect for real-time chat. Retrying...', 'Dismiss', { duration: 4000 })
    );

    this.subscriptions.add(
      this.chatService.messageStarted$.subscribe((event) => {
        this.isAwaitingFirstChunk.set(true);

        if (!this.conversationId()) {
          this.conversationId.set(event.conversationId);
          this.router.navigate(['/chat', event.conversationId], { replaceUrl: true });
        }

        if (event.userMessage) {
          this.messages.update((list) => [...list, event.userMessage!]);
        }

        if (event.regeneratedMessageId) {
          this.streamingMessageId = event.regeneratedMessageId;
          this.messages.update((list) =>
            list.map((m) => (m.id === event.regeneratedMessageId ? { ...m, content: '', isStreaming: true } : m))
          );
        } else {
          const placeholderId = uuid();
          this.streamingMessageId = placeholderId;
          this.messages.update((list) => [
            ...list,
            {
              id: placeholderId,
              conversationId: event.conversationId,
              role: 'Assistant',
              content: '',
              createdAt: new Date().toISOString(),
              isStreaming: true
            }
          ]);
        }

        this.scrollToBottom();
      })
    );

    this.subscriptions.add(
      this.chatService.messageChunk$.subscribe((event) => {
        this.isAwaitingFirstChunk.set(false);
        if (!this.streamingMessageId) {
          return;
        }
        this.messages.update((list) =>
          list.map((m) => (m.id === this.streamingMessageId ? { ...m, content: m.content + event.delta } : m))
        );
        this.scrollToBottom();
      })
    );

    this.subscriptions.add(
      this.chatService.messageCompleted$.subscribe((event) => {
        this.isGenerating.set(false);
        this.isAwaitingFirstChunk.set(false);
        const finishedId = this.streamingMessageId;
        this.streamingMessageId = null;

        this.messages.update((list) =>
          list.map((m) => (m.id === finishedId ? { ...event.message, isStreaming: false } : m))
        );

        if (this.settingsService.settings().autoSpeak && !this.chatService.voiceModeActive()) {
          this.speak(event.message.content);
        }
      })
    );

    this.subscriptions.add(
      this.chatService.error$.subscribe((event) => {
        this.isGenerating.set(false);
        this.isAwaitingFirstChunk.set(false);
        if (this.streamingMessageId) {
          this.messages.update((list) => list.filter((m) => m.id !== this.streamingMessageId));
          this.streamingMessageId = null;
        }
        this.snackBar.open(event.message, 'Dismiss', { duration: 5000 });
      })
    );

    this.subscriptions.add(
      this.voiceRecognitionService.finalResult$.subscribe((text) => {
        if (this.chatService.voiceModeActive()) {
          return;
        }
        this.inputText.set(text);
        this.sendMessage();
      })
    );

    this.subscriptions.add(
      this.voiceRecognitionService.error$.subscribe((message) =>
        this.snackBar.open(message, 'Dismiss', { duration: 5000 })
      )
    );

    this.subscriptions.add(
      this.textToSpeechService.speechError$.subscribe((message) =>
        this.snackBar.open(message, 'Dismiss', { duration: 6000 })
      )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadConversation(id: string): void {
    this.isLoadingConversation.set(true);
    this.conversationService.getDetail(id).subscribe({
      next: (detail) => {
        this.messages.set(detail.messages);
        this.isLoadingConversation.set(false);
        this.scrollToBottom();
      },
      error: () => {
        this.isLoadingConversation.set(false);
        this.router.navigate(['/chat']);
      }
    });
  }

  sendMessage(): void {
    const text = this.inputText().trim();
    if (!text || this.isGenerating()) {
      return;
    }

    const attachments = this.attachedFiles();
    const messageToSend = attachments.length
      ? this.folderUploadService.buildContextBlock(attachments) + text
      : text;

    this.inputText.set('');
    this.attachedFiles.set([]);
    this.isGenerating.set(true);

    this.chatService.sendMessage(this.conversationId(), messageToSend).catch(() => {
      this.isGenerating.set(false);
      this.snackBar.open('Unable to send your message. Please check your connection.', 'Dismiss', { duration: 5000 });
    });
  }

  async onFolderSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const fileList = input.files;
    const hasFiles = !!fileList && fileList.length > 0;
    // input.files is a *live* FileList — it must be read in full before we reset the input's
    // value (which is how we allow re-selecting the same folder later), or the reset clears
    // the very list we're about to iterate.
    const files = hasFiles ? Array.from(fileList!) : [];
    input.value = '';
    if (!hasFiles) {
      return;
    }

    this.isReadingFolder.set(true);
    try {
      const result = await this.folderUploadService.readFiles(files);
      this.attachedFiles.set(result.files);

      if (result.files.length === 0) {
        this.snackBar.open('No readable text files were found in that folder.', 'Dismiss', { duration: 4000 });
      } else {
        const notes: string[] = [];
        if (result.skippedBinary) notes.push(`${result.skippedBinary} binary file(s) skipped`);
        if (result.skippedTooLarge) notes.push(`${result.skippedTooLarge} oversized file(s) skipped`);
        if (result.truncated) notes.push('content truncated to fit context limits');
        const suffix = notes.length ? ` (${notes.join(', ')})` : '';
        this.snackBar.open(`Attached ${result.files.length} file(s)${suffix}.`, 'Dismiss', { duration: 4000 });
      }
    } finally {
      this.isReadingFolder.set(false);
    }
  }

  removeAttachment(file: AttachedFile): void {
    this.attachedFiles.update((list) => list.filter((f) => f !== file));
  }

  clearAttachments(): void {
    this.attachedFiles.set([]);
  }

  stopGeneration(): void {
    this.chatService.stopGeneration().catch(() => void 0);
    this.isGenerating.set(false);
    this.isAwaitingFirstChunk.set(false);
    if (this.streamingMessageId) {
      this.messages.update((list) =>
        list.map((m) => (m.id === this.streamingMessageId ? { ...m, isStreaming: false } : m))
      );
      this.streamingMessageId = null;
    }
  }

  toggleMic(): void {
    if (this.isSpeaking()) {
      this.textToSpeechService.stop();
    }
    this.voiceRecognitionService.toggleListening(this.settingsService.settings().language);
  }

  regenerate(message: ChatMessage): void {
    if (this.isGenerating()) {
      return;
    }
    this.isGenerating.set(true);
    this.chatService.regenerateMessage(message.id).catch(() => {
      this.isGenerating.set(false);
      this.snackBar.open('Unable to regenerate this response.', 'Dismiss', { duration: 5000 });
    });
  }

  deleteMessage(message: ChatMessage): void {
    this.conversationService.deleteMessage(message.id).subscribe(() => {
      this.messages.update((list) => list.filter((m) => m.id !== message.id));
    });
  }

  copyMessage(message: ChatMessage): void {
    navigator.clipboard.writeText(message.content).then(() =>
      this.snackBar.open('Copied to clipboard', undefined, { duration: 1500 })
    );
  }

  speak(text: string): void {
    const settings = this.settingsService.settings();
    this.textToSpeechService.speak(text, {
      voiceName: settings.voice === 'default' ? undefined : settings.voice,
      rate: settings.speechRate,
      pitch: settings.pitch,
      volume: settings.volume,
      lang: settings.language
    });
  }

  stopSpeaking(): void {
    this.textToSpeechService.stop();
  }

  openVoiceConversation(): void {
    this.dialog.open(VoiceConversationComponent, {
      panelClass: 'voice-conversation-dialog',
      maxWidth: '100vw',
      width: '100%',
      height: '100%',
      data: { conversationId: this.conversationId() }
    });
  }

  onInputKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom(): void {
    queueMicrotask(() => {
      const el = this.scrollContainer?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    });
  }
}
