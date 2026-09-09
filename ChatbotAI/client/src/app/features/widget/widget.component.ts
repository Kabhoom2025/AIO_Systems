import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DatePipe } from '@angular/common';
import { v4 as uuid } from '../../shared/util/uuid';
import { ChatMessage } from '../../core/models/message.model';
import { ApiErrorResponse } from '../../core/models/api-error.model';
import { AuthService } from '../../core/services/auth.service';
import { ConversationService } from '../../core/services/conversation.service';
import { ChatService } from '../../core/services/chat.service';
import { VoiceRecognitionService } from '../../core/services/voice-recognition.service';
import { TextToSpeechService } from '../../core/services/text-to-speech.service';
import { SettingsService } from '../../core/services/settings.service';
import { MarkdownPipe } from '../../shared/pipes/markdown.pipe';
import { CodeCopyDirective } from '../../shared/directives/code-copy.directive';

const WIDGET_CONVERSATION_KEY = 'chatbot.widget.conversationId';

type AuthView = 'login' | 'register';

/**
 * A compact, self-contained chat experience meant to be embedded in an <iframe> on a third-party
 * site (see public/embed/widget-loader.js) — no sidebar/routing, just auth + one ongoing
 * conversation. Deliberately duplicates a slimmed-down subset of ChatComponent's logic rather
 * than sharing a base class with it: the two have different auth/navigation assumptions (this
 * one has no router-guarded shell to rely on) and the widget's simplicity is the point.
 */
@Component({
  selector: 'app-widget',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MarkdownPipe,
    CodeCopyDirective
  ],
  templateUrl: './widget.component.html',
  styleUrl: './widget.component.scss'
})
export class WidgetChatComponent implements OnInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly conversationService = inject(ConversationService);
  private readonly chatService = inject(ChatService);
  private readonly voiceRecognitionService = inject(VoiceRecognitionService);
  private readonly textToSpeechService = inject(TextToSpeechService);
  private readonly settingsService = inject(SettingsService);
  private readonly snackBar = inject(MatSnackBar);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly currentUser = this.authService.currentUser;
  readonly authView = signal<AuthView>('login');
  readonly isSubmittingAuth = signal(false);
  /** Inline error shown on the login/register forms only — post-authentication errors (chat,
   *  voice, speech) go through the snackbar instead since this signal isn't rendered once the
   *  auth view is hidden. */
  readonly authError = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  readonly registerForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly conversationId = signal<string | null>(null);
  readonly messages = signal<ChatMessage[]>([]);
  readonly inputText = signal('');
  readonly isLoadingConversation = signal(false);
  readonly isGenerating = signal(false);

  readonly isListening = this.voiceRecognitionService.isListening;
  readonly interimTranscript = this.voiceRecognitionService.interimTranscript;
  readonly isVoiceSupported = this.voiceRecognitionService.isSupported;
  readonly isTtsSupported = this.textToSpeechService.isSupported;
  readonly isSpeaking = this.textToSpeechService.isSpeaking;
  readonly hasMessages = computed(() => this.messages().length > 0);

  private streamingMessageId: string | null = null;
  private readonly subscriptions = new Subscription();

  ngOnInit(): void {
    if (this.authService.getAccessToken()) {
      this.authService.loadCurrentUser().subscribe(() => this.afterAuthenticated());
    }

    this.subscriptions.add(
      this.chatService.messageStarted$.subscribe((event) => {
        if (!this.conversationId()) {
          this.conversationId.set(event.conversationId);
          this.persistConversationId(event.conversationId);
        }

        if (event.userMessage) {
          this.messages.update((list) => [...list, event.userMessage!]);
        }

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
        this.scrollToBottom();
      })
    );

    this.subscriptions.add(
      this.chatService.messageChunk$.subscribe((event) => {
        if (!this.streamingMessageId) return;
        this.messages.update((list) =>
          list.map((m) => (m.id === this.streamingMessageId ? { ...m, content: m.content + event.delta } : m))
        );
        this.scrollToBottom();
      })
    );

    this.subscriptions.add(
      this.chatService.messageCompleted$.subscribe((event) => {
        this.isGenerating.set(false);
        const finishedId = this.streamingMessageId;
        this.streamingMessageId = null;
        this.messages.update((list) =>
          list.map((m) => (m.id === finishedId ? { ...event.message, isStreaming: false } : m))
        );

        if (this.settingsService.settings().autoSpeak) {
          this.speak(event.message.content);
        }
      })
    );

    this.subscriptions.add(
      // Note: only shown once authenticated (see comment on `authError` below) — before that,
      // errors are surfaced through the login/register forms' own inline error instead.
      this.chatService.error$.subscribe((event) => {
        this.isGenerating.set(false);
        if (this.streamingMessageId) {
          this.messages.update((list) => list.filter((m) => m.id !== this.streamingMessageId));
          this.streamingMessageId = null;
        }
        this.snackBar.open(event.message, 'Dismiss', { duration: 5000 });
      })
    );

    this.subscriptions.add(
      this.voiceRecognitionService.finalResult$.subscribe((text) => {
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

  private afterAuthenticated(): void {
    this.chatService.connect().catch(() => void 0);

    const savedId = this.readPersistedConversationId();
    if (savedId) {
      this.conversationId.set(savedId);
      this.isLoadingConversation.set(true);
      this.conversationService.getDetail(savedId).subscribe({
        next: (detail) => {
          this.messages.set(detail.messages);
          this.isLoadingConversation.set(false);
          this.scrollToBottom();
        },
        error: () => {
          this.conversationId.set(null);
          this.isLoadingConversation.set(false);
        }
      });
    }
  }

  submitLogin(): void {
    if (this.loginForm.invalid || this.isSubmittingAuth()) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.isSubmittingAuth.set(true);
    this.authError.set(null);
    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.isSubmittingAuth.set(false);
        this.afterAuthenticated();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmittingAuth.set(false);
        const body = error.error as ApiErrorResponse | undefined;
        this.authError.set(body?.message ?? 'Unable to sign in. Please try again.');
      }
    });
  }

  submitRegister(): void {
    if (this.registerForm.invalid || this.isSubmittingAuth()) {
      this.registerForm.markAllAsTouched();
      return;
    }
    this.isSubmittingAuth.set(true);
    this.authError.set(null);
    this.authService.register(this.registerForm.getRawValue()).subscribe({
      next: () => {
        this.isSubmittingAuth.set(false);
        this.afterAuthenticated();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmittingAuth.set(false);
        const body = error.error as ApiErrorResponse | undefined;
        if (body?.errors) {
          const firstError = Object.values(body.errors)[0]?.[0];
          this.authError.set(firstError ?? body.message);
        } else {
          this.authError.set(body?.message ?? 'Unable to create your account. Please try again.');
        }
      }
    });
  }

  switchAuthView(view: AuthView): void {
    this.authView.set(view);
    this.authError.set(null);
  }

  sendMessage(): void {
    const text = this.inputText().trim();
    if (!text || this.isGenerating()) return;

    this.inputText.set('');
    this.isGenerating.set(true);
    this.chatService.sendMessage(this.conversationId(), text).catch(() => {
      this.isGenerating.set(false);
      this.snackBar.open('Unable to send your message. Please check your connection.', 'Dismiss', { duration: 5000 });
    });
  }

  onInputKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  toggleMic(): void {
    if (this.isSpeaking()) this.textToSpeechService.stop();
    this.voiceRecognitionService.toggleListening(this.settingsService.settings().language);
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

  copyMessage(message: ChatMessage): void {
    navigator.clipboard.writeText(message.content).catch(() => void 0);
  }

  startNewChat(): void {
    this.conversationId.set(null);
    this.messages.set([]);
    this.persistConversationId(null);
  }

  logout(): void {
    this.authService.logout(false);
    this.messages.set([]);
    this.conversationId.set(null);
    this.authView.set('login');
  }

  /** Tells the host page's loader script to collapse the widget back to just the bubble button. */
  closeWidget(): void {
    window.parent.postMessage({ type: 'ai-chat-widget:close' }, '*');
  }

  private persistConversationId(id: string | null): void {
    try {
      if (id) localStorage.setItem(WIDGET_CONVERSATION_KEY, id);
      else localStorage.removeItem(WIDGET_CONVERSATION_KEY);
    } catch {
      // ignore storage failures (private browsing, etc.) — the widget still works, just
      // without resuming the conversation on the next visit.
    }
  }

  private readPersistedConversationId(): string | null {
    try {
      return localStorage.getItem(WIDGET_CONVERSATION_KEY);
    } catch {
      return null;
    }
  }

  private scrollToBottom(): void {
    queueMicrotask(() => {
      const el = this.scrollContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
  }
}
