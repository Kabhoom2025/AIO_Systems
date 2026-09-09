import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { AdminApiService } from '../../core/services/admin-api.service';
import { ApiClient, AuditLog, BrandProfile, RiskFactorCategory, RiskRule } from '../../core/models/admin.models';

type AdminTab = 'brands' | 'riskRules' | 'apiClients' | 'auditLogs';

const RISK_FACTOR_CATEGORIES: RiskFactorCategory[] = [
  'UrlAnalysis', 'DomainIntelligence', 'DnsAnalysis', 'SslAnalysis',
  'RedirectAnalysis', 'ThreatIntelligence', 'BrandImpersonation', 'MachineLearning'
];

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent {
  private readonly adminApi = inject(AdminApiService);
  private readonly fb = inject(FormBuilder);

  readonly activeTab = signal<AdminTab>('brands');
  readonly categories = RISK_FACTOR_CATEGORIES;

  readonly brands = signal<BrandProfile[]>([]);
  readonly riskRules = signal<RiskRule[]>([]);
  readonly auditLogs = signal<AuditLog[]>([]);
  readonly apiClients = signal<ApiClient[]>([]);
  readonly newlyIssuedKey = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly brandForm = this.fb.nonNullable.group({
    brandName: ['', Validators.required],
    officialDomain: ['', Validators.required]
  });

  readonly apiClientForm = this.fb.nonNullable.group({
    organizationName: ['', Validators.required],
    contactEmail: ['', [Validators.required, Validators.email]],
    dailyQuota: [1000, [Validators.required, Validators.min(1)]],
    requestsPerMinute: [60, [Validators.required, Validators.min(1)]]
  });

  readonly riskRuleForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
    category: this.fb.nonNullable.control<RiskFactorCategory>('UrlAnalysis'),
    weight: [0.1, [Validators.required, Validators.min(0), Validators.max(1)]],
    conditionExpression: ['', Validators.required],
    scoreContribution: [5, [Validators.required, Validators.min(0), Validators.max(100)]]
  });

  constructor() {
    this.loadBrands();
    this.loadRiskRules();
    this.loadAuditLogs();
    this.loadApiClients();
  }

  setTab(tab: AdminTab): void {
    this.activeTab.set(tab);
  }

  loadBrands(): void {
    this.adminApi.getBrands().subscribe({
      next: (brands) => this.brands.set(brands),
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  loadRiskRules(): void {
    this.adminApi.getRiskRules().subscribe({
      next: (rules) => this.riskRules.set(rules),
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  loadAuditLogs(): void {
    this.adminApi.getAuditLogs(1, 50).subscribe({
      next: (result) => this.auditLogs.set(result.items),
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  submitBrand(): void {
    if (this.brandForm.invalid) {
      this.brandForm.markAllAsTouched();
      return;
    }
    this.adminApi.createBrand(this.brandForm.getRawValue()).subscribe({
      next: () => {
        this.brandForm.reset();
        this.loadBrands();
      },
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  submitRiskRule(): void {
    if (this.riskRuleForm.invalid) {
      this.riskRuleForm.markAllAsTouched();
      return;
    }
    this.adminApi.createRiskRule(this.riskRuleForm.getRawValue()).subscribe({
      next: () => {
        this.riskRuleForm.reset({ category: 'UrlAnalysis', weight: 0.1, scoreContribution: 5 });
        this.loadRiskRules();
      },
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  toggleRiskRuleEnabled(rule: RiskRule): void {
    this.adminApi
      .updateRiskRule(rule.id, {
        name: rule.name,
        description: rule.description,
        weight: rule.weight,
        isEnabled: !rule.isEnabled,
        conditionExpression: rule.conditionExpression,
        scoreContribution: rule.scoreContribution
      })
      .subscribe({
        next: () => this.loadRiskRules(),
        error: (err: HttpErrorResponse) => this.setError(err)
      });
  }

  loadApiClients(): void {
    this.adminApi.getApiClients().subscribe({
      next: (clients) => this.apiClients.set(clients),
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  submitApiClient(): void {
    if (this.apiClientForm.invalid) {
      this.apiClientForm.markAllAsTouched();
      return;
    }
    this.adminApi.createApiClient(this.apiClientForm.getRawValue()).subscribe({
      next: () => {
        this.apiClientForm.reset({ dailyQuota: 1000, requestsPerMinute: 60 });
        this.loadApiClients();
      },
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  issueApiKey(client: ApiClient): void {
    const label = prompt(`Label for the new key on ${client.organizationName}:`, 'default');
    if (!label) return;

    this.adminApi.createApiKey(client.id, { label }).subscribe({
      next: (created) => {
        this.newlyIssuedKey.set(created.rawKey);
        this.loadApiClients();
      },
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  revokeApiKey(keyId: string): void {
    this.adminApi.revokeApiKey(keyId).subscribe({
      next: () => this.loadApiClients(),
      error: (err: HttpErrorResponse) => this.setError(err)
    });
  }

  dismissNewKey(): void {
    this.newlyIssuedKey.set(null);
  }

  private setError(err: HttpErrorResponse): void {
    this.errorMessage.set(err.error?.message ?? 'Request failed.');
  }
}
