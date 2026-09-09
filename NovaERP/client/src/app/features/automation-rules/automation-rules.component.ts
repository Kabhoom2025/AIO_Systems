import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  AutomationRuleApiService, AutomationRuleDto, CreateAutomationRuleDto, UpdateAutomationRuleDto,
  AUTOMATION_TRIGGER_EVENTS
} from '../../core/automation-rule-api.service';
import { RoleApiService, RoleDto } from '../../core/role-api.service';

@Component({
  selector: 'app-automation-rules',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, TextareaModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './automation-rules.component.html',
  styleUrl: './automation-rules.component.scss'
})
export class AutomationRulesComponent implements OnInit {
  rules: AutomationRuleDto[] = [];
  roles: RoleDto[] = [];
  loading = false;

  triggerEvents = AUTOMATION_TRIGGER_EVENTS;
  actionTypes = ['Notify'];

  showDialog = false;
  editing: AutomationRuleDto | null = null;
  saving = false;

  form: CreateAutomationRuleDto = this.emptyForm();

  constructor(
    private api: AutomationRuleApiService,
    private roleApi: RoleApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.roleApi.getAll().subscribe({ next: rows => (this.roles = rows), error: () => (this.roles = []) });
  }

  private emptyForm(): CreateAutomationRuleDto {
    return {
      name: '', triggerEvent: '', actionType: 'Notify',
      notifyRoleId: null, notifyMessageTemplate: '', isEnabled: true
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.rules = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load automation rules.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(rule: AutomationRuleDto) {
    this.editing = rule;
    this.form = {
      name: rule.name,
      triggerEvent: rule.triggerEvent,
      actionType: rule.actionType,
      notifyRoleId: rule.notifyRoleId ?? null,
      notifyMessageTemplate: rule.notifyMessageTemplate,
      isEnabled: rule.isEnabled
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || !this.form.triggerEvent || !this.form.notifyMessageTemplate) {
      this.notify.warn('Name, trigger event and message template are required.');
      return;
    }
    this.saving = true;
    const req$ = this.editing
      ? this.api.update(this.editing.id, this.form as UpdateAutomationRuleDto)
      : this.api.create(this.form);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Automation rule ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save automation rule.');
      }
    });
  }

  delete(rule: AutomationRuleDto) {
    this.confirm.confirm({
      message: `Delete automation rule "${rule.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(rule.id).subscribe({
          next: () => {
            this.notify.success('Automation rule deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete automation rule.')
        });
      }
    });
  }
}
