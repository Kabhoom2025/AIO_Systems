import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { FeatureToggleApiService, FeatureToggleDto } from '../../core/feature-toggle-api.service';

@Component({
  selector: 'app-feature-toggles',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ToastModule, TagModule, ToggleSwitchModule, HasPermissionDirective],
  templateUrl: './feature-toggles.component.html',
  styleUrl: './feature-toggles.component.scss'
})
export class FeatureTogglesComponent implements OnInit {
  toggles: FeatureToggleDto[] = [];
  loading = false;
  savingKey: string | null = null;

  constructor(private api: FeatureToggleApiService, private notify: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.toggles = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load feature toggles.');
      }
    });
  }

  get activeModules(): FeatureToggleDto[] {
    return this.toggles.filter(t => t.isEnabled);
  }

  get inactiveModules(): FeatureToggleDto[] {
    return this.toggles.filter(t => !t.isEnabled);
  }

  toggle(row: FeatureToggleDto, enabled: boolean) {
    const previous = row.isEnabled;
    row.isEnabled = enabled;
    this.savingKey = row.moduleKey;
    this.api.update(row.moduleKey, { moduleKey: row.moduleKey, isEnabled: enabled }).subscribe({
      next: updated => {
        row.isEnabled = updated.isEnabled;
        this.savingKey = null;
        this.notify.success(`${row.moduleKey} ${updated.isEnabled ? 'enabled' : 'disabled'}.`);
      },
      error: err => {
        row.isEnabled = previous;
        this.savingKey = null;
        this.notify.error(err.error?.message ?? 'Failed to update feature toggle.');
      }
    });
  }
}
