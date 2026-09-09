import { Component, Inject, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { ChatService } from '../../../core/services/chat.service';
import { VoiceRecognitionService } from '../../../core/services/voice-recognition.service';
import { TextToSpeechService } from '../../../core/services/text-to-speech.service';
import { SettingsService } from '../../../core/services/settings.service';

type VoiceStatus = 'idle' | 'listening' | 'processing' | 'speaking' | 'paused';

export interface VoiceConversationDialogData {
  conversationId: string | null;
}

@Component({
  selector: 'app-voice-conversation',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './voice-conversation.component.html',
  styleUrl: './voice-conversation.component.scss'
})
export class VoiceConversationComponent implements OnInit, OnDestroy {
  readonly status = signal<VoiceStatus>('idle');
  readonly muted = signal(false);
  readonly lastUserText = signal('');
  readonly lastAssistantText = signal('');
  private readonly dialogRef = inject(MatDialogRef<VoiceConversationComponent>);
  private readonly chatService = inject(ChatService);
  private readonly voiceRecognitionService = inject(VoiceRecognitionService);
  private readonly textToSpeechService = inject(TextToSpeechService);
  private readonly settingsService = inject(SettingsService);
  private readonly snackBar = inject(MatSnackBar);

  readonly interimTranscript = this.voiceRecognitionService.interimTranscript;
  readonly isVoiceSupported = this.voiceRecognitionService.isSupported;
  readonly isTtsSupported = this.textToSpeechService.isSupported;

  private conversationId: string | null;
  private readonly subscriptions = new Subscription();

  constructor(@Inject(MAT_DIALOG_DATA) data: VoiceConversationDialogData) {
    this.conversationId = data.conversationId;
  }

  ngOnInit(): void {
    this.chatService.voiceModeActive.set(true);

    this.subscriptions.add(
      this.chatService.messageStarted$.subscribe((event) => {
        this.conversationId = event.conversationId;
        if (event.userMessage) {
          this.lastUserText.set(event.userMessage.content);
        }
        this.status.set('processing');
      })
    );

    this.subscriptions.add(
      this.chatService.messageCompleted$.subscribe((event) => {
        this.lastAssistantText.set(event.message.content);
        if (this.status() === 'paused') {
          return;
        }
        if (this.muted()) {
          this.beginListening();
        } else {
          this.status.set('speaking');
          const settings = this.settingsService.settings();
          this.textToSpeechService.speak(event.message.content, {
            voiceName: settings.voice === 'default' ? undefined : settings.voice,
            rate: settings.speechRate,
            pitch: settings.pitch,
            volume: settings.volume,
            lang: settings.language
          });
        }
      })
    );

    this.subscriptions.add(
      this.chatService.error$.subscribe(() => {
        if (this.status() !== 'paused') {
          this.beginListening();
        }
      })
    );

    this.subscriptions.add(
      this.textToSpeechService.speechEnded$.subscribe(() => {
        if (this.status() === 'speaking') {
          this.beginListening();
        }
      })
    );

    this.subscriptions.add(
      this.voiceRecognitionService.finalResult$.subscribe((text) => {
        this.lastUserText.set(text);
        this.status.set('processing');
        this.chatService.sendMessage(this.conversationId, text).catch(() => this.beginListening());
      })
    );

    this.subscriptions.add(
      this.textToSpeechService.speechError$.subscribe((message) =>
        this.snackBar.open(message, 'Dismiss', { duration: 6000 })
      )
    );

    this.beginListening();
  }

  ngOnDestroy(): void {
    this.chatService.voiceModeActive.set(false);
    this.voiceRecognitionService.cancelListening();
    this.textToSpeechService.stop();
    this.subscriptions.unsubscribe();
  }

  toggleMic(): void {
    switch (this.status()) {
      case 'speaking':
        this.textToSpeechService.stop();
        this.beginListening();
        break;
      case 'processing':
        this.chatService.stopGeneration().catch(() => void 0);
        this.beginListening();
        break;
      case 'listening':
        this.voiceRecognitionService.stopListening();
        this.status.set('paused');
        break;
      default:
        this.beginListening();
    }
  }

  togglePause(): void {
    if (this.status() === 'paused') {
      this.beginListening();
    } else {
      this.voiceRecognitionService.cancelListening();
      this.textToSpeechService.stop();
      this.status.set('paused');
    }
  }

  toggleMute(): void {
    this.muted.set(!this.muted());
    if (this.muted() && this.status() === 'speaking') {
      this.textToSpeechService.stop();
      this.beginListening();
    }
  }

  endConversation(): void {
    this.dialogRef.close();
  }

  private beginListening(): void {
    if (!this.isVoiceSupported) {
      return;
    }
    this.status.set('listening');
    const language = this.settingsService.settings().language;
    this.voiceRecognitionService.startListening(language);
  }
}
