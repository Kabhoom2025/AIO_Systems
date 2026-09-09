import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { TagModule } from 'primeng/tag';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  AiAssistantSettingsApiService, AiAssistantSettingsDto, AiProvider, UpdateAiAssistantSettingsDto
} from '../../core/ai-assistant-settings-api.service';

@Component({
  selector: 'app-ai-assistant-settings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ButtonModule, InputTextModule, PasswordModule, TagModule,
    SelectButtonModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './ai-assistant-settings.component.html',
  styleUrl: './ai-assistant-settings.component.scss'
})
export class AiAssistantSettingsComponent implements OnInit {
  loading = false;
  saving = false;
  isConfigured = false;

  providerOptions: { label: string; value: AiProvider }[] = [
    { label: 'Anthropic (Claude)', value: 'Anthropic' },
    { label: 'Groq (free)', value: 'Groq' }
  ];

  form: UpdateAiAssistantSettingsDto = this.emptyForm();

  constructor(
    private api: AiAssistantSettingsApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm(): UpdateAiAssistantSettingsDto {
    return {
      provider: 'Anthropic',
      anthropicApiKey: '',
      anthropicModel: 'claude-sonnet-4-5',
      groqApiKey: '',
      groqModel: 'llama-3.3-70b-versatile'
    };
  }

  load() {
    this.loading = true;
    this.api.get().subscribe({
      next: (settings: AiAssistantSettingsDto) => {
        this.form = {
          provider: settings.provider,
          anthropicApiKey: '',
          anthropicModel: settings.anthropicModel ?? 'claude-sonnet-4-5',
          groqApiKey: '',
          groqModel: settings.groqModel ?? 'llama-3.3-70b-versatile'
        };
        this.isConfigured = settings.isConfigured;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load AI Assistant settings.');
      }
    });
  }

  save() {
    this.saving = true;
    this.api.update(this.form).subscribe({
      next: () => {
        this.saving = false;
        this.notify.success('AI Assistant settings updated.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save AI Assistant settings.');
      }
    });
  }
}
