import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { PasswordModule } from 'primeng/password';
import { FieldsetModule } from 'primeng/fieldset';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  NotificationChannelSettingsApiService, NotificationChannelSettingsDto, UpdateNotificationChannelSettingsDto
} from '../../core/notification-channel-settings-api.service';
import { NotificationApiService } from '../../core/notification-api.service';

type Channel = 'Email' | 'Sms' | 'Push';

@Component({
  selector: 'app-notification-channel-settings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ButtonModule, InputTextModule, InputNumberModule,
    CheckboxModule, PasswordModule, FieldsetModule, DialogModule, TextareaModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './notification-channel-settings.component.html',
  styleUrl: './notification-channel-settings.component.scss'
})
export class NotificationChannelSettingsComponent implements OnInit {
  loading = false;
  saving = false;

  form: UpdateNotificationChannelSettingsDto = this.emptyForm();

  showTestDialog = false;
  testChannel: Channel | null = null;
  testTo = '';
  testMessage = '';
  testSubject = '';
  testing = false;

  constructor(
    private api: NotificationChannelSettingsApiService,
    private notificationApi: NotificationApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm(): UpdateNotificationChannelSettingsDto {
    return {
      smtpHost: '', smtpPort: null, smtpUsername: '', smtpPassword: '',
      smtpFromEmail: '', smtpFromName: '', smtpUseSsl: true,
      smsApiUrl: '', smsApiKey: '', smsSenderId: '',
      pushApiUrl: '', pushServerKey: ''
    };
  }

  load() {
    this.loading = true;
    this.api.get().subscribe({
      next: (settings: NotificationChannelSettingsDto) => {
        this.form = {
          smtpHost: settings.smtpHost ?? '',
          smtpPort: settings.smtpPort ?? null,
          smtpUsername: settings.smtpUsername ?? '',
          smtpPassword: '',
          smtpFromEmail: settings.smtpFromEmail ?? '',
          smtpFromName: settings.smtpFromName ?? '',
          smtpUseSsl: settings.smtpUseSsl,
          smsApiUrl: settings.smsApiUrl ?? '',
          smsApiKey: '',
          smsSenderId: settings.smsSenderId ?? '',
          pushApiUrl: settings.pushApiUrl ?? '',
          pushServerKey: ''
        };
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load notification channel settings.');
      }
    });
  }

  save() {
    this.saving = true;
    this.api.update(this.form).subscribe({
      next: () => {
        this.saving = false;
        this.notify.success('Notification channel settings updated.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save notification channel settings.');
      }
    });
  }

  openTest(channel: Channel) {
    this.testChannel = channel;
    this.testTo = '';
    this.testMessage = '';
    this.testSubject = '';
    this.showTestDialog = true;
  }

  sendTest() {
    if (!this.testChannel || !this.testTo || !this.testMessage) {
      this.notify.warn('Recipient and message are required.');
      return;
    }
    this.testing = true;
    this.notificationApi.testSend({
      channel: this.testChannel,
      to: this.testTo,
      message: this.testMessage,
      subject: this.testSubject || null
    }).subscribe({
      next: result => {
        this.testing = false;
        this.showTestDialog = false;
        if (result.success) {
          this.notify.success(`Test ${this.testChannel} send succeeded (status: ${result.status}).`);
        } else {
          this.notify.error(result.error ?? `Test ${this.testChannel} send failed (status: ${result.status}).`);
        }
      },
      error: err => {
        this.testing = false;
        this.notify.error(err.error?.message ?? 'Failed to send test notification.');
      }
    });
  }
}
