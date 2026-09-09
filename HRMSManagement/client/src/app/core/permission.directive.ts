import { Directive, Input, TemplateRef, ViewContainerRef, OnInit } from '@angular/core';
import { PermissionsService } from './permissions.service';

@Directive({
  selector: '[appHasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit {
  @Input('appHasPermission') permission: string | string[] = '';

  constructor(
    private templateRef: TemplateRef<unknown>,
    private viewContainer: ViewContainerRef,
    private permissions: PermissionsService
  ) {}

  ngOnInit() {
    const keys = Array.isArray(this.permission) ? this.permission : [this.permission];
    const allowed = keys.length === 0 || this.permissions.hasAny(keys);
    this.viewContainer.clear();
    if (allowed) this.viewContainer.createEmbeddedView(this.templateRef);
  }
}
