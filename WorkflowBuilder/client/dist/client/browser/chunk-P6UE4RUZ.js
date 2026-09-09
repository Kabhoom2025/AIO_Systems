import {
  ConfirmDialog,
  ConfirmDialogModule,
  Dialog,
  DialogModule,
  Dropdown,
  DropdownModule,
  InputTextarea,
  Table,
  TableModule,
  Tag,
  TagModule,
  WorkflowApiService
} from "./chunk-5ZM5W3YJ.js";
import {
  DefaultValueAccessor,
  FormsModule,
  InputText,
  InputTextModule,
  NgControlStatus,
  NgModel,
  NotificationService,
  Toast,
  ToastModule
} from "./chunk-VL7VHOPB.js";
import "./chunk-XWE55AK6.js";
import {
  ButtonDirective,
  ButtonModule
} from "./chunk-MDHJYRAE.js";
import {
  CommonModule,
  ConfirmationService,
  DatePipe,
  NgIf,
  PrimeTemplate,
  Router
} from "./chunk-MGCDEZWU.js";
import {
  Component,
  setClassMetadata,
  ɵsetClassDebugInfo,
  ɵɵProvidersFeature,
  ɵɵadvance,
  ɵɵdefineComponent,
  ɵɵdirectiveInject,
  ɵɵelement,
  ɵɵelementEnd,
  ɵɵelementStart,
  ɵɵgetCurrentView,
  ɵɵlistener,
  ɵɵnextContext,
  ɵɵpipe,
  ɵɵpipeBind2,
  ɵɵproperty,
  ɵɵpureFunction0,
  ɵɵresetView,
  ɵɵrestoreView,
  ɵɵstyleMap,
  ɵɵtemplate,
  ɵɵtext,
  ɵɵtextInterpolate,
  ɵɵtextInterpolate1,
  ɵɵtwoWayBindingSet,
  ɵɵtwoWayListener,
  ɵɵtwoWayProperty
} from "./chunk-RUVEZ3RD.js";

// src/app/features/workflows/workflows-list.component.ts
var _c0 = () => ({ width: "450px" });
var _c1 = () => ({ width: "100%" });
function WorkflowsListComponent_ng_template_8_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "tr")(1, "th");
    \u0275\u0275text(2, "Name");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(3, "th");
    \u0275\u0275text(4, "Trigger");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "th");
    \u0275\u0275text(6, "Status");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(7, "th");
    \u0275\u0275text(8, "Active");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(9, "th");
    \u0275\u0275text(10, "Updated");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(11, "th", 13);
    \u0275\u0275text(12, "Actions");
    \u0275\u0275elementEnd()();
  }
}
function WorkflowsListComponent_ng_template_9_div_4_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 21);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const wf_r2 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(wf_r2.description);
  }
}
function WorkflowsListComponent_ng_template_9_span_10_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const wf_r2 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1("Published v", wf_r2.publishedVersionNumber, "");
  }
}
function WorkflowsListComponent_ng_template_9_span_11_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1, " \xA0\xB7\xA0");
    \u0275\u0275elementEnd();
  }
}
function WorkflowsListComponent_ng_template_9_span_12_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const wf_r2 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1("Draft v", wf_r2.draftVersionNumber, "");
  }
}
function WorkflowsListComponent_ng_template_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r1 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "tr")(1, "td")(2, "a", 14);
    \u0275\u0275listener("click", function WorkflowsListComponent_ng_template_9_Template_a_click_2_listener() {
      const wf_r2 = \u0275\u0275restoreView(_r1).$implicit;
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.openBuilder(wf_r2));
    });
    \u0275\u0275text(3);
    \u0275\u0275elementEnd();
    \u0275\u0275template(4, WorkflowsListComponent_ng_template_9_div_4_Template, 2, 1, "div", 15);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "td");
    \u0275\u0275text(6);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(7, "td");
    \u0275\u0275element(8, "p-tag", 16);
    \u0275\u0275elementStart(9, "div", 17);
    \u0275\u0275template(10, WorkflowsListComponent_ng_template_9_span_10_Template, 2, 1, "span", 18)(11, WorkflowsListComponent_ng_template_9_span_11_Template, 2, 0, "span", 18)(12, WorkflowsListComponent_ng_template_9_span_12_Template, 2, 1, "span", 18);
    \u0275\u0275elementEnd()();
    \u0275\u0275elementStart(13, "td");
    \u0275\u0275element(14, "p-tag", 16);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(15, "td");
    \u0275\u0275text(16);
    \u0275\u0275pipe(17, "date");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(18, "td")(19, "button", 19);
    \u0275\u0275listener("click", function WorkflowsListComponent_ng_template_9_Template_button_click_19_listener() {
      const wf_r2 = \u0275\u0275restoreView(_r1).$implicit;
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.openBuilder(wf_r2));
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(20, "button", 20);
    \u0275\u0275listener("click", function WorkflowsListComponent_ng_template_9_Template_button_click_20_listener() {
      const wf_r2 = \u0275\u0275restoreView(_r1).$implicit;
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.confirmDelete(wf_r2));
    });
    \u0275\u0275elementEnd()()();
  }
  if (rf & 2) {
    const wf_r2 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance(3);
    \u0275\u0275textInterpolate(wf_r2.name);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", wf_r2.description);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(wf_r2.triggerLabel);
    \u0275\u0275advance(2);
    \u0275\u0275property("value", ctx_r2.publishStatus(wf_r2).label)("severity", ctx_r2.publishStatus(wf_r2).severity);
    \u0275\u0275advance(2);
    \u0275\u0275property("ngIf", wf_r2.publishedVersionNumber);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", wf_r2.publishedVersionNumber && wf_r2.draftVersionNumber !== wf_r2.publishedVersionNumber);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", wf_r2.draftVersionNumber !== wf_r2.publishedVersionNumber);
    \u0275\u0275advance(2);
    \u0275\u0275property("value", wf_r2.isActive ? "Active" : "Inactive")("severity", wf_r2.isActive ? "success" : "secondary");
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(\u0275\u0275pipeBind2(17, 11, wf_r2.updatedDate, "medium"));
  }
}
function WorkflowsListComponent_ng_template_10_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "tr")(1, "td", 22);
    \u0275\u0275text(2, "No workflows yet. Create one to get started.");
    \u0275\u0275elementEnd()();
  }
}
function WorkflowsListComponent_ng_template_24_Template(rf, ctx) {
  if (rf & 1) {
    const _r4 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "button", 23);
    \u0275\u0275listener("click", function WorkflowsListComponent_ng_template_24_Template_button_click_0_listener() {
      \u0275\u0275restoreView(_r4);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.showCreateDialog = false);
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(1, "button", 24);
    \u0275\u0275listener("click", function WorkflowsListComponent_ng_template_24_Template_button_click_1_listener() {
      \u0275\u0275restoreView(_r4);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.saveWorkflow());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance();
    \u0275\u0275property("loading", ctx_r2.saving);
  }
}
var WorkflowsListComponent = class _WorkflowsListComponent {
  api;
  router;
  notify;
  confirmation;
  workflows = [];
  loading = false;
  triggerTypes = [];
  showCreateDialog = false;
  saving = false;
  createForm = this.emptyForm();
  constructor(api, router, notify, confirmation) {
    this.api = api;
    this.router = router;
    this.notify = notify;
    this.confirmation = confirmation;
  }
  ngOnInit() {
    this.loadWorkflows();
    this.loadTriggerTypes();
  }
  loadWorkflows() {
    this.loading = true;
    this.api.getWorkflows().subscribe({
      next: (data) => {
        this.workflows = data;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.notify.error(err.error?.message ?? "Failed to load workflows.");
      }
    });
  }
  loadTriggerTypes() {
    this.api.getTriggerTypes().subscribe({
      next: (data) => this.triggerTypes = data,
      error: () => this.triggerTypes = []
    });
  }
  openCreateDialog() {
    this.createForm = this.emptyForm();
    this.showCreateDialog = true;
  }
  saveWorkflow() {
    if (!this.createForm.name || !this.createForm.triggerType) {
      this.notify.warn("Please fill in name and trigger.");
      return;
    }
    this.saving = true;
    this.api.createWorkflow(this.createForm).subscribe({
      next: (wf) => {
        this.saving = false;
        this.showCreateDialog = false;
        this.notify.success("Workflow created \u2014 configure it in the builder.");
        this.router.navigate(["/workflows", wf.id]);
      },
      error: (err) => {
        this.saving = false;
        this.notify.error(err.error?.message ?? "Failed to create workflow.");
      }
    });
  }
  openBuilder(wf) {
    this.router.navigate(["/workflows", wf.id]);
  }
  confirmDelete(wf) {
    this.confirmation.confirm({
      message: `Delete workflow "${wf.name}"? This cannot be undone.`,
      header: "Confirm Delete",
      icon: "pi pi-exclamation-triangle",
      accept: () => this.deleteWorkflow(wf)
    });
  }
  deleteWorkflow(wf) {
    this.api.deleteWorkflow(wf.id).subscribe({
      next: () => {
        this.notify.success("Workflow deleted.");
        this.loadWorkflows();
      },
      error: (err) => this.notify.error(err.error?.message ?? "Failed to delete workflow.")
    });
  }
  publishStatus(wf) {
    if (!wf.publishedVersionId) {
      return { label: "Draft only", severity: "warn" };
    }
    if (wf.publishedVersionNumber === wf.draftVersionNumber) {
      return { label: "Published", severity: "success" };
    }
    return { label: "Published (older)", severity: "info" };
  }
  emptyForm() {
    return { name: "", description: "", triggerType: "" };
  }
  static \u0275fac = function WorkflowsListComponent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _WorkflowsListComponent)(\u0275\u0275directiveInject(WorkflowApiService), \u0275\u0275directiveInject(Router), \u0275\u0275directiveInject(NotificationService), \u0275\u0275directiveInject(ConfirmationService));
  };
  static \u0275cmp = /* @__PURE__ */ \u0275\u0275defineComponent({ type: _WorkflowsListComponent, selectors: [["app-workflows-list"]], features: [\u0275\u0275ProvidersFeature([ConfirmationService])], decls: 25, vars: 14, consts: [[1, "page-header"], ["pButton", "", "type", "button", "icon", "pi pi-plus", "label", "Create Workflow", 3, "click"], [1, "card"], ["dataKey", "id", "responsiveLayout", "scroll", 3, "value", "loading"], ["pTemplate", "header"], ["pTemplate", "body"], ["pTemplate", "emptymessage"], ["header", "Create Workflow", 3, "visibleChange", "visible", "modal"], [1, "form-field"], ["pInputText", "", "type", "text", "placeholder", "e.g. Order Follow-up", 3, "ngModelChange", "ngModel"], ["pInputTextarea", "", "rows", "3", "placeholder", "Optional description", 3, "ngModelChange", "ngModel"], ["optionLabel", "label", "optionValue", "key", "placeholder", "Select a trigger", 3, "ngModelChange", "options", "ngModel"], ["pTemplate", "footer"], [2, "width", "160px"], [1, "wf-link", 3, "click"], ["class", "wf-desc", 4, "ngIf"], [3, "value", "severity"], [1, "version-line"], [4, "ngIf"], ["pButton", "", "type", "button", "icon", "pi pi-pencil", 1, "p-button-text", "p-button-sm", 3, "click"], ["pButton", "", "type", "button", "icon", "pi pi-trash", 1, "p-button-text", "p-button-sm", "p-button-danger", 3, "click"], [1, "wf-desc"], ["colspan", "6", 1, "empty-cell"], ["pButton", "", "type", "button", "label", "Cancel", 1, "p-button-text", 3, "click"], ["pButton", "", "type", "button", "label", "Create", 3, "click", "loading"]], template: function WorkflowsListComponent_Template(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275element(0, "p-toast")(1, "p-confirmDialog");
      \u0275\u0275elementStart(2, "div", 0)(3, "h1");
      \u0275\u0275text(4, "Workflows");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(5, "button", 1);
      \u0275\u0275listener("click", function WorkflowsListComponent_Template_button_click_5_listener() {
        return ctx.openCreateDialog();
      });
      \u0275\u0275elementEnd()();
      \u0275\u0275elementStart(6, "div", 2)(7, "p-table", 3);
      \u0275\u0275template(8, WorkflowsListComponent_ng_template_8_Template, 13, 0, "ng-template", 4)(9, WorkflowsListComponent_ng_template_9_Template, 21, 14, "ng-template", 5)(10, WorkflowsListComponent_ng_template_10_Template, 3, 0, "ng-template", 6);
      \u0275\u0275elementEnd()();
      \u0275\u0275elementStart(11, "p-dialog", 7);
      \u0275\u0275twoWayListener("visibleChange", function WorkflowsListComponent_Template_p_dialog_visibleChange_11_listener($event) {
        \u0275\u0275twoWayBindingSet(ctx.showCreateDialog, $event) || (ctx.showCreateDialog = $event);
        return $event;
      });
      \u0275\u0275elementStart(12, "div", 8)(13, "label");
      \u0275\u0275text(14, "Name");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(15, "input", 9);
      \u0275\u0275twoWayListener("ngModelChange", function WorkflowsListComponent_Template_input_ngModelChange_15_listener($event) {
        \u0275\u0275twoWayBindingSet(ctx.createForm.name, $event) || (ctx.createForm.name = $event);
        return $event;
      });
      \u0275\u0275elementEnd()();
      \u0275\u0275elementStart(16, "div", 8)(17, "label");
      \u0275\u0275text(18, "Description");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(19, "textarea", 10);
      \u0275\u0275twoWayListener("ngModelChange", function WorkflowsListComponent_Template_textarea_ngModelChange_19_listener($event) {
        \u0275\u0275twoWayBindingSet(ctx.createForm.description, $event) || (ctx.createForm.description = $event);
        return $event;
      });
      \u0275\u0275elementEnd()();
      \u0275\u0275elementStart(20, "div", 8)(21, "label");
      \u0275\u0275text(22, "Trigger");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(23, "p-dropdown", 11);
      \u0275\u0275twoWayListener("ngModelChange", function WorkflowsListComponent_Template_p_dropdown_ngModelChange_23_listener($event) {
        \u0275\u0275twoWayBindingSet(ctx.createForm.triggerType, $event) || (ctx.createForm.triggerType = $event);
        return $event;
      });
      \u0275\u0275elementEnd()();
      \u0275\u0275template(24, WorkflowsListComponent_ng_template_24_Template, 2, 1, "ng-template", 12);
      \u0275\u0275elementEnd();
    }
    if (rf & 2) {
      \u0275\u0275advance(7);
      \u0275\u0275property("value", ctx.workflows)("loading", ctx.loading);
      \u0275\u0275advance(4);
      \u0275\u0275styleMap(\u0275\u0275pureFunction0(12, _c0));
      \u0275\u0275twoWayProperty("visible", ctx.showCreateDialog);
      \u0275\u0275property("modal", true);
      \u0275\u0275advance(4);
      \u0275\u0275twoWayProperty("ngModel", ctx.createForm.name);
      \u0275\u0275advance(4);
      \u0275\u0275twoWayProperty("ngModel", ctx.createForm.description);
      \u0275\u0275advance(4);
      \u0275\u0275styleMap(\u0275\u0275pureFunction0(13, _c1));
      \u0275\u0275property("options", ctx.triggerTypes);
      \u0275\u0275twoWayProperty("ngModel", ctx.createForm.triggerType);
    }
  }, dependencies: [
    CommonModule,
    NgIf,
    DatePipe,
    FormsModule,
    DefaultValueAccessor,
    NgControlStatus,
    NgModel,
    TableModule,
    Table,
    PrimeTemplate,
    DialogModule,
    Dialog,
    ButtonModule,
    ButtonDirective,
    InputTextModule,
    InputText,
    InputTextarea,
    DropdownModule,
    Dropdown,
    TagModule,
    Tag,
    ToastModule,
    Toast,
    ConfirmDialogModule,
    ConfirmDialog
  ], styles: ["\n\n.wf-link[_ngcontent-%COMP%] {\n  cursor: pointer;\n  color: #2563eb;\n  font-weight: 600;\n}\n.wf-link[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n.wf-desc[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  color: #6b7280;\n}\n.version-line[_ngcontent-%COMP%] {\n  font-size: 0.7rem;\n  color: #6b7280;\n  margin-top: 2px;\n}\n.empty-cell[_ngcontent-%COMP%] {\n  text-align: center;\n  color: #9ca3af;\n  padding: 2rem;\n}\n.form-field[_ngcontent-%COMP%] {\n  margin-bottom: 1rem;\n  display: flex;\n  flex-direction: column;\n  gap: 0.375rem;\n}\n.form-field[_ngcontent-%COMP%]   label[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  font-weight: 600;\n  color: #374151;\n}\n/*# sourceMappingURL=workflows-list.component.css.map */"] });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(WorkflowsListComponent, [{
    type: Component,
    args: [{ selector: "app-workflows-list", standalone: true, imports: [
      CommonModule,
      FormsModule,
      TableModule,
      DialogModule,
      ButtonModule,
      InputTextModule,
      InputTextarea,
      DropdownModule,
      TagModule,
      ToastModule,
      ConfirmDialogModule
    ], providers: [ConfirmationService], template: `<p-toast></p-toast>
<p-confirmDialog></p-confirmDialog>

<div class="page-header">
  <h1>Workflows</h1>
  <button pButton type="button" icon="pi pi-plus" label="Create Workflow" (click)="openCreateDialog()"></button>
</div>

<div class="card">
  <p-table [value]="workflows" [loading]="loading" dataKey="id" responsiveLayout="scroll">
    <ng-template pTemplate="header">
      <tr>
        <th>Name</th>
        <th>Trigger</th>
        <th>Status</th>
        <th>Active</th>
        <th>Updated</th>
        <th style="width: 160px">Actions</th>
      </tr>
    </ng-template>
    <ng-template pTemplate="body" let-wf>
      <tr>
        <td>
          <a class="wf-link" (click)="openBuilder(wf)">{{ wf.name }}</a>
          <div class="wf-desc" *ngIf="wf.description">{{ wf.description }}</div>
        </td>
        <td>{{ wf.triggerLabel }}</td>
        <td>
          <p-tag [value]="publishStatus(wf).label" [severity]="publishStatus(wf).severity"></p-tag>
          <div class="version-line">
            <span *ngIf="wf.publishedVersionNumber">Published v{{ wf.publishedVersionNumber }}</span>
            <span *ngIf="wf.publishedVersionNumber && wf.draftVersionNumber !== wf.publishedVersionNumber">
              &nbsp;&middot;&nbsp;</span
            >
            <span *ngIf="wf.draftVersionNumber !== wf.publishedVersionNumber">Draft v{{ wf.draftVersionNumber }}</span>
          </div>
        </td>
        <td>
          <p-tag [value]="wf.isActive ? 'Active' : 'Inactive'" [severity]="wf.isActive ? 'success' : 'secondary'"></p-tag>
        </td>
        <td>{{ wf.updatedDate | date: 'medium' }}</td>
        <td>
          <button pButton type="button" icon="pi pi-pencil" class="p-button-text p-button-sm" (click)="openBuilder(wf)"></button>
          <button pButton type="button" icon="pi pi-trash" class="p-button-text p-button-sm p-button-danger" (click)="confirmDelete(wf)"></button>
        </td>
      </tr>
    </ng-template>
    <ng-template pTemplate="emptymessage">
      <tr>
        <td colspan="6" class="empty-cell">No workflows yet. Create one to get started.</td>
      </tr>
    </ng-template>
  </p-table>
</div>

<p-dialog header="Create Workflow" [(visible)]="showCreateDialog" [modal]="true" [style]="{ width: '450px' }">
  <div class="form-field">
    <label>Name</label>
    <input pInputText type="text" [(ngModel)]="createForm.name" placeholder="e.g. Order Follow-up" />
  </div>
  <div class="form-field">
    <label>Description</label>
    <textarea pInputTextarea rows="3" [(ngModel)]="createForm.description" placeholder="Optional description"></textarea>
  </div>
  <div class="form-field">
    <label>Trigger</label>
    <p-dropdown
      [options]="triggerTypes"
      optionLabel="label"
      optionValue="key"
      [(ngModel)]="createForm.triggerType"
      placeholder="Select a trigger"
      [style]="{ width: '100%' }"
    ></p-dropdown>
  </div>
  <ng-template pTemplate="footer">
    <button pButton type="button" label="Cancel" class="p-button-text" (click)="showCreateDialog = false"></button>
    <button pButton type="button" label="Create" [loading]="saving" (click)="saveWorkflow()"></button>
  </ng-template>
</p-dialog>
`, styles: ["/* src/app/features/workflows/workflows-list.component.scss */\n.wf-link {\n  cursor: pointer;\n  color: #2563eb;\n  font-weight: 600;\n}\n.wf-link:hover {\n  text-decoration: underline;\n}\n.wf-desc {\n  font-size: 0.75rem;\n  color: #6b7280;\n}\n.version-line {\n  font-size: 0.7rem;\n  color: #6b7280;\n  margin-top: 2px;\n}\n.empty-cell {\n  text-align: center;\n  color: #9ca3af;\n  padding: 2rem;\n}\n.form-field {\n  margin-bottom: 1rem;\n  display: flex;\n  flex-direction: column;\n  gap: 0.375rem;\n}\n.form-field label {\n  font-size: 0.75rem;\n  font-weight: 600;\n  color: #374151;\n}\n/*# sourceMappingURL=workflows-list.component.css.map */\n"] }]
  }], () => [{ type: WorkflowApiService }, { type: Router }, { type: NotificationService }, { type: ConfirmationService }], null);
})();
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && \u0275setClassDebugInfo(WorkflowsListComponent, { className: "WorkflowsListComponent", filePath: "app/features/workflows/workflows-list.component.ts", lineNumber: 45 });
})();
export {
  WorkflowsListComponent
};
//# sourceMappingURL=chunk-P6UE4RUZ.js.map
