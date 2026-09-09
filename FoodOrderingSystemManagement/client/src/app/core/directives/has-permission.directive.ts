import { Directive, Input, TemplateRef, ViewContainerRef, inject, signal, computed, effect } from '@angular/core';
import { PermissionStateService } from '../services/permission-state.service';

/** Structural directive: *hasPermission="'feature.key'" */
@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective {
  private permState   = inject(PermissionStateService);
  private vcr         = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<unknown>);

  private featureKey = signal('');
  private hasView    = false;

  constructor() {
    effect(() => {
      const allowed = this.permState.hasPermission(this.featureKey());
      if (allowed && !this.hasView) {
        this.vcr.createEmbeddedView(this.templateRef);
        this.hasView = true;
      } else if (!allowed && this.hasView) {
        this.vcr.clear();
        this.hasView = false;
      }
    });
  }

  @Input({ alias: 'hasPermission' }) set feature(value: string) {
    this.featureKey.set(value ?? '');
  }
}
