import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../core/notification.service';
import {
  AiAssistantApiService, ConversationSummaryDto, ConversationDto, MessageDto
} from '../../core/ai-assistant-api.service';

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, ToastModule, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './ai-assistant.component.html',
  styleUrl: './ai-assistant.component.scss'
})
export class AiAssistantComponent implements OnInit {
  loading = false;
  sending = false;
  resolvingActionMessageId: number | null = null;
  conversations: ConversationSummaryDto[] = [];
  current: ConversationDto | null = null;

  draft = '';

  constructor(
    private api: AiAssistantApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getConversations().subscribe({
      next: rows => {
        this.conversations = rows;
        this.loading = false;
        if (rows.length) this.selectConversation(rows[0].id);
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load conversations.');
      }
    });
  }

  selectConversation(id: number) {
    this.api.getConversation(id).subscribe({
      next: c => (this.current = c),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load conversation.')
    });
  }

  newConversation() {
    this.api.createConversation({ title: 'New Conversation' }).subscribe({
      next: created => {
        this.conversations = [{ id: created.id, title: created.title, createdDate: new Date().toISOString() }, ...this.conversations];
        this.current = created;
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to create conversation.')
    });
  }

  deleteConversation(c: ConversationSummaryDto) {
    this.confirm.confirm({
      message: `Delete conversation "${c.title}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteConversation(c.id).subscribe({
          next: () => {
            this.conversations = this.conversations.filter(x => x.id !== c.id);
            if (this.current?.id === c.id) this.current = null;
            this.notify.success('Conversation deleted.');
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete conversation.')
        });
      }
    });
  }

  send() {
    if (!this.current || !this.draft.trim()) return;
    const content = this.draft.trim();
    this.draft = '';
    this.sending = true;

    // Optimistic user bubble so the UI feels responsive while waiting for the reply.
    this.current.messages = [...this.current.messages, { id: 0, role: 'user', content, createdDate: new Date().toISOString() }];

    this.api.sendMessage(this.current.id, { content }).subscribe({
      next: updated => {
        this.current = updated;
        this.sending = false;
      },
      error: err => {
        this.sending = false;
        this.notify.error(err.error?.message ?? 'Failed to send message.');
      }
    });
  }

  confirmAction(m: MessageDto) {
    if (!this.current) return;
    this.resolvingActionMessageId = m.id;
    this.api.confirmAction(this.current.id, m.id).subscribe({
      next: updated => {
        this.current = updated;
        this.resolvingActionMessageId = null;
      },
      error: err => {
        this.resolvingActionMessageId = null;
        this.notify.error(err.error?.message ?? 'Failed to confirm action.');
      }
    });
  }

  cancelAction(m: MessageDto) {
    if (!this.current) return;
    this.resolvingActionMessageId = m.id;
    this.api.cancelAction(this.current.id, m.id).subscribe({
      next: updated => {
        this.current = updated;
        this.resolvingActionMessageId = null;
      },
      error: err => {
        this.resolvingActionMessageId = null;
        this.notify.error(err.error?.message ?? 'Failed to cancel action.');
      }
    });
  }

  trackByMessage(_index: number, m: MessageDto) {
    return m.id || m.createdDate;
  }
}
