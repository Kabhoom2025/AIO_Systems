import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { Conversation } from '../../core/models/conversation.model';
import { ConversationService } from '../../core/services/conversation.service';
import { ChatService } from '../../core/services/chat.service';
import { RenameConversationDialogComponent } from './rename-conversation-dialog.component';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './conversation-list.component.html',
  styleUrl: './conversation-list.component.scss'
})
export class ConversationListComponent {
  @Input() activeConversationId: string | null = null;
  @Output() conversationSelected = new EventEmitter<void>();

  readonly conversations = signal<Conversation[]>([]);
  readonly searchTerm = signal('');
  readonly isLoading = signal(false);

  constructor(
    private readonly conversationService: ConversationService,
    private readonly chatService: ChatService,
    private readonly router: Router,
    private readonly dialog: MatDialog
  ) {
    this.load();

    // Keep the list (titles, ordering, and newly-created conversations) in sync with
    // whatever the chat view or voice conversation mode is doing over SignalR.
    this.chatService.messageStarted$.subscribe(() => this.load());
    this.chatService.messageCompleted$.subscribe(() => this.load());
  }

  load(): void {
    this.isLoading.set(true);
    this.conversationService.getAll(this.searchTerm() || undefined).subscribe({
      next: (conversations) => {
        this.conversations.set(conversations);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.load();
  }

  startNewChat(): void {
    this.router.navigate(['/chat']);
    this.conversationSelected.emit();
  }

  openConversation(id: string): void {
    this.router.navigate(['/chat', id]);
    this.conversationSelected.emit();
  }

  rename(conversation: Conversation): void {
    const dialogRef = this.dialog.open(RenameConversationDialogComponent, { data: { title: conversation.title } });
    dialogRef.afterClosed().subscribe((newTitle: string | undefined) => {
      if (newTitle && newTitle.trim() && newTitle !== conversation.title) {
        this.conversationService.rename(conversation.id, newTitle.trim()).subscribe(() => this.load());
      }
    });
  }

  archive(conversation: Conversation): void {
    this.conversationService.archive(conversation.id, !conversation.isArchived).subscribe(() => this.load());
  }

  remove(conversation: Conversation): void {
    this.conversationService.delete(conversation.id).subscribe(() => {
      this.load();
      if (this.activeConversationId === conversation.id) {
        this.router.navigate(['/chat']);
      }
    });
  }
}
