import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatMessage } from '../models/message.model';
import { AuthService } from './auth.service';

export interface MessageStartedEvent {
  conversationId: string;
  userMessage?: ChatMessage;
  regeneratedMessageId?: string;
}

export interface MessageChunkEvent {
  conversationId: string;
  delta: string;
}

export interface MessageCompletedEvent {
  conversationId: string;
  message: ChatMessage;
}

export interface ChatErrorEvent {
  message: string;
  errorCode: string;
}

export type HubConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private hubConnection: signalR.HubConnection | null = null;
  private connectPromise: Promise<void> | null = null;

  readonly connectionState = signal<HubConnectionState>('disconnected');

  /** True while the full-screen Voice Conversation dialog owns speaking the AI's replies,
   *  so the regular chat view's "auto speak" setting doesn't also narrate them (double audio). */
  readonly voiceModeActive = signal(false);

  readonly messageStarted$ = new Subject<MessageStartedEvent>();
  readonly messageChunk$ = new Subject<MessageChunkEvent>();
  readonly messageCompleted$ = new Subject<MessageCompletedEvent>();
  readonly typingStarted$ = new Subject<{ conversationId: string }>();
  readonly typingStopped$ = new Subject<{ conversationId: string }>();
  readonly error$ = new Subject<ChatErrorEvent>();

  constructor(private readonly authService: AuthService) {}

  async connect(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }
    if (this.connectPromise) {
      return this.connectPromise;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubBaseUrl, {
        accessTokenFactory: () => this.authService.getAccessToken() ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('MessageStarted', (payload: MessageStartedEvent) => this.messageStarted$.next(payload));
    connection.on('MessageChunk', (payload: MessageChunkEvent) => this.messageChunk$.next(payload));
    connection.on('MessageCompleted', (payload: MessageCompletedEvent) => this.messageCompleted$.next(payload));
    connection.on('TypingStarted', (payload: { conversationId: string }) => this.typingStarted$.next(payload));
    connection.on('TypingStopped', (payload: { conversationId: string }) => this.typingStopped$.next(payload));
    connection.on('Error', (payload: ChatErrorEvent) => this.error$.next(payload));

    connection.onreconnecting(() => this.connectionState.set('reconnecting'));
    connection.onreconnected(() => this.connectionState.set('connected'));
    connection.onclose(() => this.connectionState.set('disconnected'));

    this.hubConnection = connection;
    this.connectionState.set('connecting');

    this.connectPromise = connection
      .start()
      .then(() => this.connectionState.set('connected'))
      .catch((error) => {
        this.connectionState.set('disconnected');
        throw error;
      })
      .finally(() => {
        this.connectPromise = null;
      });

    return this.connectPromise;
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
    this.connectionState.set('disconnected');
  }

  async sendMessage(conversationId: string | null, message: string): Promise<void> {
    await this.connect();
    await this.hubConnection?.invoke('SendMessage', conversationId, message);
  }

  async regenerateMessage(assistantMessageId: string): Promise<void> {
    await this.connect();
    await this.hubConnection?.invoke('RegenerateMessage', assistantMessageId);
  }

  async stopGeneration(): Promise<void> {
    await this.hubConnection?.invoke('StopGeneration');
  }
}
