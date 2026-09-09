import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SettingsService } from '../../core/services/settings.service';
import { Settings } from '../../core/models/settings.model';

export interface IntegrationDef {
  id: string;
  name: string;
  icon: string;
  category: string;
  description: string;
  fields: IntegrationField[];
  connectedWhen: (keyof Settings)[];
}

export interface IntegrationField {
  label: string;
  key: keyof Settings;
  placeholder: string;
  type?: 'text' | 'password' | 'url';
  hint?: string;
}

const INTEGRATIONS: IntegrationDef[] = [
  {
    id: 'razorpay', name: 'Razorpay', icon: 'payments', category: 'Payment Gateway',
    description: 'Accept online payments via UPI, cards, net banking and wallets.',
    connectedWhen: ['razorpayKeyId'],
    fields: [
      { label: 'Key ID',     key: 'razorpayKeyId',     placeholder: 'rzp_live_xxxxxxxxxxxxx', type: 'text' },
      { label: 'Key Secret', key: 'razorpayKeySecret',  placeholder: 'Enter secret key',       type: 'password' },
    ],
  },
  {
    id: 'msg91', name: 'MSG91', icon: 'sms', category: 'SMS',
    description: 'Send SMS alerts and OTPs to customers via MSG91.',
    connectedWhen: ['msg91ApiKey'],
    fields: [
      { label: 'API Key',   key: 'msg91ApiKey',    placeholder: 'Enter MSG91 API key',  type: 'password' },
      { label: 'Sender ID', key: 'msg91SenderId',  placeholder: 'RESTNM (6 chars)',     type: 'text' },
    ],
  },
  {
    id: 'ultramsg', name: 'UltraMsg', icon: 'chat', category: 'WhatsApp',
    description: 'Send WhatsApp messages for order confirmations and updates.',
    connectedWhen: ['ultraMsgInstanceId'],
    fields: [
      { label: 'Instance ID', key: 'ultraMsgInstanceId', placeholder: 'instance12345',    type: 'text' },
      { label: 'Token',       key: 'ultraMsgToken',      placeholder: 'Enter token',       type: 'password' },
    ],
  },
  {
    id: 'dunzo', name: 'Dunzo', icon: 'electric_scooter', category: 'Delivery Partner',
    description: 'Integrate Dunzo for last-mile delivery of orders.',
    connectedWhen: ['dunzoApiKey'],
    fields: [
      { label: 'API Key', key: 'dunzoApiKey', placeholder: 'Enter Dunzo API key', type: 'password' },
    ],
  },
  {
    id: 'swiggy', name: 'Swiggy Genie', icon: 'delivery_dining', category: 'Delivery Partner',
    description: 'Use Swiggy Genie for on-demand delivery services.',
    connectedWhen: ['swiggyApiKey'],
    fields: [
      { label: 'API Key', key: 'swiggyApiKey', placeholder: 'Enter Swiggy Genie API key', type: 'password' },
    ],
  },
  {
    id: 'analytics', name: 'Google Analytics', icon: 'analytics', category: 'Analytics',
    description: 'Track website and order page traffic with Google Analytics 4.',
    connectedWhen: ['googleAnalyticsId'],
    fields: [
      { label: 'Measurement ID', key: 'googleAnalyticsId', placeholder: 'G-XXXXXXXXXX', type: 'text', hint: 'Find this in GA4 Admin → Data Streams' },
    ],
  },
  {
    id: 'mailchimp', name: 'Mailchimp', icon: 'email', category: 'Email Marketing',
    description: 'Sync customer emails to Mailchimp for marketing campaigns.',
    connectedWhen: ['mailchimpApiKey'],
    fields: [
      { label: 'API Key', key: 'mailchimpApiKey', placeholder: 'xxxxxxxxxxxxxxxxxxxxxxxx-us1', type: 'password' },
    ],
  },
  {
    id: 'slack', name: 'Slack', icon: 'notifications_active', category: 'Team Notifications',
    description: 'Post new order alerts and system notifications to a Slack channel.',
    connectedWhen: ['slackWebhookUrl'],
    fields: [
      { label: 'Webhook URL', key: 'slackWebhookUrl', placeholder: 'https://hooks.slack.com/services/...', type: 'url' },
    ],
  },
  {
    id: 'crm', name: 'CRM Integration', icon: 'people_alt', category: 'Customer Relationship',
    description: 'Sync customer data to HubSpot, Zoho CRM, or a custom CRM via webhook for lifecycle management and campaigns.',
    connectedWhen: ['crmApiKey'],
    fields: [
      { label: 'CRM Provider',  key: 'crmProvider',   placeholder: 'hubspot / zoho / custom', type: 'text',
        hint: 'Identifies the CRM platform for the sync adapter' },
      { label: 'API Key',       key: 'crmApiKey',     placeholder: 'Enter CRM API key',       type: 'password' },
      { label: 'Webhook URL',   key: 'crmWebhookUrl', placeholder: 'https://api.yourcrm.com/webhook', type: 'url',
        hint: 'Customer events (new order, churn risk) will be POSTed here' },
    ],
  },
  {
    id: 'accounting', name: 'Accounting', icon: 'account_balance', category: 'Finance & Accounting',
    description: 'Push daily sales, GST summaries, and journal entries to QuickBooks, Xero, or Tally automatically.',
    connectedWhen: ['accountingApiKey'],
    fields: [
      { label: 'Provider',      key: 'accountingProvider',   placeholder: 'quickbooks / xero / tally', type: 'text' },
      { label: 'API Key',       key: 'accountingApiKey',     placeholder: 'Enter accounting API key',   type: 'password' },
      { label: 'Webhook URL',   key: 'accountingWebhookUrl', placeholder: 'https://api.youraccounting.com/sync', type: 'url',
        hint: 'Sales data is posted here at end-of-day or on each order' },
    ],
  },
  {
    id: 'erp', name: 'ERP Integration', icon: 'hub', category: 'Enterprise',
    description: 'Sync inventory levels, supplier orders, and sales data to SAP, Oracle, NetSuite, or a custom ERP system.',
    connectedWhen: ['erpApiKey'],
    fields: [
      { label: 'ERP Provider',  key: 'erpProvider',   placeholder: 'sap / oracle / netsuite / custom', type: 'text' },
      { label: 'API Key',       key: 'erpApiKey',     placeholder: 'Enter ERP API key',                type: 'password' },
      { label: 'Sync Endpoint', key: 'erpWebhookUrl', placeholder: 'https://your-erp.com/api/sync',    type: 'url',
        hint: 'Inventory and order events will be synced to this endpoint' },
    ],
  },
];

@Component({
  selector: 'app-integration-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule,
            MatProgressSpinnerModule, MatInputModule, MatFormFieldModule, MatSnackBarModule],
  templateUrl: './integration-settings.component.html',
  styleUrl: './integration-settings.component.scss',
})
export class IntegrationSettingsComponent implements OnInit {
  private svc   = inject(SettingsService);
  private snack = inject(MatSnackBar);

  loading  = signal(true);
  saving   = signal<string | null>(null);
  settings = signal<Settings | null>(null);
  expanded = signal<string | null>(null);
  integrations = INTEGRATIONS;

  ngOnInit(): void {
    this.svc.getSettings().subscribe({
      next: s => { this.settings.set(s); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load settings', 'Close', { duration: 3000 }); },
    });
  }

  isConnected(intg: IntegrationDef): boolean {
    const s = this.settings();
    if (!s) return false;
    return intg.connectedWhen.every(k => !!(s[k]));
  }

  getField(key: keyof Settings): string {
    return (this.settings()?.[key] as string) ?? '';
  }

  setField(key: keyof Settings, value: string): void {
    this.settings.update(s => s ? { ...s, [key]: value || null } : s);
  }

  toggleExpand(id: string): void {
    this.expanded.update(cur => cur === id ? null : id);
  }

  saveIntegration(intg: IntegrationDef): void {
    const s = this.settings();
    if (!s) return;
    this.saving.set(intg.id);
    this.svc.updateSettings(s).subscribe({
      next: () => {
        this.saving.set(null);
        this.expanded.set(null);
        this.snack.open(`${intg.name} settings saved`, 'OK', { duration: 2500 });
      },
      error: () => {
        this.saving.set(null);
        this.snack.open('Failed to save settings', 'Close', { duration: 3000 });
      },
    });
  }

  disconnect(intg: IntegrationDef): void {
    this.settings.update(s => {
      if (!s) return s;
      const patch: Partial<Settings> = {};
      intg.fields.forEach(f => (patch as Record<string, unknown>)[f.key] = null);
      return { ...s, ...patch };
    });
    this.saveIntegration(intg);
  }
}
