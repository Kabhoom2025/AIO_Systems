import {
  AuthService
} from "./chunk-4KW6FJ4N.js";
import {
  ButtonDirective,
  ButtonModule
} from "./chunk-MDHJYRAE.js";
import {
  CommonModule,
  NgIf,
  Router,
  RouterOutlet,
  environment
} from "./chunk-MGCDEZWU.js";
import {
  Component,
  setClassMetadata,
  ɵsetClassDebugInfo,
  ɵɵadvance,
  ɵɵdefineComponent,
  ɵɵdirectiveInject,
  ɵɵelement,
  ɵɵelementEnd,
  ɵɵelementStart,
  ɵɵlistener,
  ɵɵnextContext,
  ɵɵproperty,
  ɵɵtemplate,
  ɵɵtext,
  ɵɵtextInterpolate
} from "./chunk-RUVEZ3RD.js";

// src/app/shell/shell.component.ts
function ShellComponent_span_4_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span", 6);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = \u0275\u0275nextContext();
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r0.userName);
  }
}
var ShellComponent = class _ShellComponent {
  auth;
  router;
  userName = "";
  constructor(auth, router) {
    this.auth = auth;
    this.router = router;
    const raw = localStorage.getItem(environment.userKey);
    this.userName = raw ? JSON.parse(raw).name : "";
  }
  logout() {
    this.auth.logout();
    this.router.navigateByUrl("/login");
  }
  static \u0275fac = function ShellComponent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ShellComponent)(\u0275\u0275directiveInject(AuthService), \u0275\u0275directiveInject(Router));
  };
  static \u0275cmp = /* @__PURE__ */ \u0275\u0275defineComponent({ type: _ShellComponent, selectors: [["app-shell"]], decls: 8, vars: 1, consts: [[1, "shell-topbar"], [1, "brand"], [1, "topbar-right"], ["class", "user-name", 4, "ngIf"], ["pButton", "", "type", "button", "label", "Log out", 1, "p-button-text", "p-button-sm", 3, "click"], [1, "shell-body"], [1, "user-name"]], template: function ShellComponent_Template(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275elementStart(0, "div", 0)(1, "span", 1);
      \u0275\u0275text(2, "Workflow Builder");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(3, "div", 2);
      \u0275\u0275template(4, ShellComponent_span_4_Template, 2, 1, "span", 3);
      \u0275\u0275elementStart(5, "button", 4);
      \u0275\u0275listener("click", function ShellComponent_Template_button_click_5_listener() {
        return ctx.logout();
      });
      \u0275\u0275elementEnd()()();
      \u0275\u0275elementStart(6, "div", 5);
      \u0275\u0275element(7, "router-outlet");
      \u0275\u0275elementEnd();
    }
    if (rf & 2) {
      \u0275\u0275advance(4);
      \u0275\u0275property("ngIf", ctx.userName);
    }
  }, dependencies: [CommonModule, NgIf, RouterOutlet, ButtonModule, ButtonDirective], styles: ["\n\n[_nghost-%COMP%] {\n  display: block;\n  height: 100vh;\n  display: flex;\n  flex-direction: column;\n}\n.shell-topbar[_ngcontent-%COMP%] {\n  height: 56px;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 0 1.25rem;\n  background: #111827;\n  color: #fff;\n}\n.shell-topbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%] {\n  font-weight: 700;\n  font-size: 1rem;\n}\n.shell-topbar[_ngcontent-%COMP%]   .topbar-right[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n}\n.shell-topbar[_ngcontent-%COMP%]   .topbar-right[_ngcontent-%COMP%]   .user-name[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n  color: #d1d5db;\n}\n.shell-topbar[_ngcontent-%COMP%]   .topbar-right[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  color: #fff;\n}\n.shell-body[_ngcontent-%COMP%] {\n  flex: 1;\n  min-height: 0;\n  overflow: auto;\n  padding: 1.25rem;\n}\n/*# sourceMappingURL=shell.component.css.map */"] });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ShellComponent, [{
    type: Component,
    args: [{ selector: "app-shell", standalone: true, imports: [CommonModule, RouterOutlet, ButtonModule], template: '<div class="shell-topbar">\n  <span class="brand">Workflow Builder</span>\n  <div class="topbar-right">\n    <span class="user-name" *ngIf="userName">{{ userName }}</span>\n    <button pButton type="button" label="Log out" class="p-button-text p-button-sm" (click)="logout()"></button>\n  </div>\n</div>\n<div class="shell-body">\n  <router-outlet></router-outlet>\n</div>\n', styles: ["/* src/app/shell/shell.component.scss */\n:host {\n  display: block;\n  height: 100vh;\n  display: flex;\n  flex-direction: column;\n}\n.shell-topbar {\n  height: 56px;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 0 1.25rem;\n  background: #111827;\n  color: #fff;\n}\n.shell-topbar .brand {\n  font-weight: 700;\n  font-size: 1rem;\n}\n.shell-topbar .topbar-right {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n}\n.shell-topbar .topbar-right .user-name {\n  font-size: 0.85rem;\n  color: #d1d5db;\n}\n.shell-topbar .topbar-right button {\n  color: #fff;\n}\n.shell-body {\n  flex: 1;\n  min-height: 0;\n  overflow: auto;\n  padding: 1.25rem;\n}\n/*# sourceMappingURL=shell.component.css.map */\n"] }]
  }], () => [{ type: AuthService }, { type: Router }], null);
})();
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && \u0275setClassDebugInfo(ShellComponent, { className: "ShellComponent", filePath: "app/shell/shell.component.ts", lineNumber: 15 });
})();
export {
  ShellComponent
};
//# sourceMappingURL=chunk-ABJED6GT.js.map
