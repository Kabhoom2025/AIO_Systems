import {
  ConfirmDialog,
  ConfirmDialogModule,
  Dialog,
  DialogModule,
  Dropdown,
  DropdownModule,
  InputTextarea,
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
  NumberValueAccessor,
  Toast,
  ToastModule
} from "./chunk-VL7VHOPB.js";
import "./chunk-XWE55AK6.js";
import {
  ButtonDirective,
  ButtonModule
} from "./chunk-MDHJYRAE.js";
import {
  ActivatedRoute,
  CommonModule,
  ConfirmationService,
  DatePipe,
  DecimalPipe,
  NgClass,
  NgForOf,
  NgIf,
  NgStyle,
  NgSwitch,
  NgSwitchCase,
  NgSwitchDefault,
  PrimeTemplate,
  Router,
  environment
} from "./chunk-MGCDEZWU.js";
import {
  Component,
  HostListener,
  Subject,
  ViewChild,
  __spreadProps,
  __spreadValues,
  debounceTime,
  forkJoin,
  setClassMetadata,
  ɵsetClassDebugInfo,
  ɵɵProvidersFeature,
  ɵɵadvance,
  ɵɵattribute,
  ɵɵclassMapInterpolate1,
  ɵɵclassProp,
  ɵɵdefineComponent,
  ɵɵdirectiveInject,
  ɵɵelement,
  ɵɵelementContainerEnd,
  ɵɵelementContainerStart,
  ɵɵelementEnd,
  ɵɵelementStart,
  ɵɵgetCurrentView,
  ɵɵlistener,
  ɵɵloadQuery,
  ɵɵnamespaceHTML,
  ɵɵnamespaceSVG,
  ɵɵnextContext,
  ɵɵpipe,
  ɵɵpipeBind2,
  ɵɵproperty,
  ɵɵpureFunction0,
  ɵɵpureFunction4,
  ɵɵqueryRefresh,
  ɵɵreference,
  ɵɵresetView,
  ɵɵresolveDocument,
  ɵɵrestoreView,
  ɵɵstyleMap,
  ɵɵstyleProp,
  ɵɵtemplate,
  ɵɵtemplateRefExtractor,
  ɵɵtext,
  ɵɵtextInterpolate,
  ɵɵtextInterpolate1,
  ɵɵtextInterpolate2,
  ɵɵtwoWayBindingSet,
  ɵɵtwoWayListener,
  ɵɵtwoWayProperty,
  ɵɵviewQuery
} from "./chunk-RUVEZ3RD.js";

// src/app/features/workflows/node-palette.component.ts
function NodePaletteComponent_div_13_Template(rf, ctx) {
  if (rf & 1) {
    const _r1 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 10);
    \u0275\u0275listener("dragstart", function NodePaletteComponent_div_13_Template_div_dragstart_0_listener($event) {
      const card_r2 = \u0275\u0275restoreView(_r1).$implicit;
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onDragStart($event, card_r2.kind));
    });
    \u0275\u0275element(1, "i");
    \u0275\u0275elementStart(2, "div", 4)(3, "div", 5);
    \u0275\u0275text(4);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "div", 6);
    \u0275\u0275text(6);
    \u0275\u0275elementEnd()()();
  }
  if (rf & 2) {
    const card_r2 = ctx.$implicit;
    \u0275\u0275advance();
    \u0275\u0275classMapInterpolate1("pi ", card_r2.icon, " card-icon");
    \u0275\u0275advance(3);
    \u0275\u0275textInterpolate(card_r2.title);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(card_r2.subtitle);
  }
}
var NodePaletteComponent = class _NodePaletteComponent {
  triggerCard = { icon: "pi-bolt", title: "Trigger", subtitle: "Initiate workflows" };
  cards = [
    { kind: "action", icon: "pi-play-circle", title: "Action", subtitle: "Perform actions based on triggers" },
    { kind: "delay", icon: "pi-clock", title: "Delay", subtitle: "Pause the workflow" },
    { kind: "decision", icon: "pi-sitemap", title: "Decision", subtitle: "Route the workflow" },
    { kind: "notification", icon: "pi-bell", title: "Notification", subtitle: "Send alerts or notifications" },
    { kind: "end", icon: "pi-flag", title: "End", subtitle: "Terminate the workflow" }
  ];
  onDragStart(event, kind) {
    event.dataTransfer?.setData("text/kind", kind);
    if (event.dataTransfer)
      event.dataTransfer.effectAllowed = "copy";
  }
  static \u0275fac = function NodePaletteComponent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _NodePaletteComponent)();
  };
  static \u0275cmp = /* @__PURE__ */ \u0275\u0275defineComponent({ type: _NodePaletteComponent, selectors: [["app-node-palette"]], decls: 16, vars: 6, consts: [[1, "palette-root"], [1, "palette-header"], [1, "palette-hint"], [1, "palette-card", "disabled"], [1, "card-text"], [1, "card-title"], [1, "card-subtitle"], ["class", "palette-card", "draggable", "true", 3, "dragstart", 4, "ngFor", "ngForOf"], [1, "palette-footer"], ["pButton", "", "type", "button", "label", "Templates", "disabled", "", 1, "p-button-outlined", "p-button-sm", "templates-btn"], ["draggable", "true", 1, "palette-card", 3, "dragstart"]], template: function NodePaletteComponent_Template(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275elementStart(0, "div", 0)(1, "div", 1)(2, "span");
      \u0275\u0275text(3, "Nodes Library");
      \u0275\u0275elementEnd()();
      \u0275\u0275elementStart(4, "div", 2);
      \u0275\u0275text(5, "Drag a node onto the canvas, then draw a connection from its port.");
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(6, "div", 3);
      \u0275\u0275element(7, "i");
      \u0275\u0275elementStart(8, "div", 4)(9, "div", 5);
      \u0275\u0275text(10);
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(11, "div", 6);
      \u0275\u0275text(12);
      \u0275\u0275elementEnd()()();
      \u0275\u0275template(13, NodePaletteComponent_div_13_Template, 7, 5, "div", 7);
      \u0275\u0275elementStart(14, "div", 8);
      \u0275\u0275element(15, "button", 9);
      \u0275\u0275elementEnd()();
    }
    if (rf & 2) {
      \u0275\u0275advance(7);
      \u0275\u0275classMapInterpolate1("pi ", ctx.triggerCard.icon, " card-icon");
      \u0275\u0275advance(3);
      \u0275\u0275textInterpolate(ctx.triggerCard.title);
      \u0275\u0275advance(2);
      \u0275\u0275textInterpolate(ctx.triggerCard.subtitle);
      \u0275\u0275advance();
      \u0275\u0275property("ngForOf", ctx.cards);
    }
  }, dependencies: [CommonModule, NgForOf, ButtonModule, ButtonDirective], styles: ["\n\n[_nghost-%COMP%] {\n  display: block;\n  height: 100%;\n  flex-shrink: 0;\n}\n.palette-root[_ngcontent-%COMP%] {\n  height: 100%;\n  display: flex;\n  flex-direction: column;\n  width: 260px;\n  background: #fff;\n  border-right: 1px solid #e5e7eb;\n  padding: 1rem;\n  overflow-y: auto;\n}\n.palette-header[_ngcontent-%COMP%] {\n  font-size: 1.1rem;\n  font-weight: 700;\n  color: #111827;\n  margin-bottom: 0.4rem;\n}\n.palette-hint[_ngcontent-%COMP%] {\n  font-size: 0.7rem;\n  color: #9ca3af;\n  margin-bottom: 1rem;\n  line-height: 1.35;\n}\n.palette-card[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n  padding: 0.75rem;\n  border: 1px solid #e5e7eb;\n  border-radius: 10px;\n  margin-bottom: 0.65rem;\n  cursor: grab;\n  background: #fff;\n  transition: border-color 0.15s, background 0.15s;\n}\n.palette-card[_ngcontent-%COMP%]:hover {\n  border-color: #2563eb;\n  background: #f8fafc;\n}\n.palette-card.disabled[_ngcontent-%COMP%] {\n  cursor: default;\n  opacity: 0.55;\n}\n.palette-card.disabled[_ngcontent-%COMP%]:hover {\n  border-color: #e5e7eb;\n  background: #fff;\n}\n.card-icon[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 34px;\n  height: 34px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  border-radius: 8px;\n  background: #eff6ff;\n  color: #2563eb;\n  font-size: 0.95rem;\n}\n.card-text[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 2px;\n}\n.card-title[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n  font-weight: 700;\n  color: #111827;\n}\n.card-subtitle[_ngcontent-%COMP%] {\n  font-size: 0.7rem;\n  color: #6b7280;\n}\n.palette-footer[_ngcontent-%COMP%] {\n  margin-top: auto;\n  padding-top: 1rem;\n}\n.templates-btn[_ngcontent-%COMP%] {\n  width: 100%;\n}\n/*# sourceMappingURL=node-palette.component.css.map */"] });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(NodePaletteComponent, [{
    type: Component,
    args: [{ selector: "app-node-palette", standalone: true, imports: [CommonModule, ButtonModule], template: '<div class="palette-root">\n  <div class="palette-header">\n    <span>Nodes Library</span>\n  </div>\n  <div class="palette-hint">Drag a node onto the canvas, then draw a connection from its port.</div>\n\n  <div class="palette-card disabled">\n    <i class="pi {{ triggerCard.icon }} card-icon"></i>\n    <div class="card-text">\n      <div class="card-title">{{ triggerCard.title }}</div>\n      <div class="card-subtitle">{{ triggerCard.subtitle }}</div>\n    </div>\n  </div>\n\n  <div\n    class="palette-card"\n    *ngFor="let card of cards"\n    draggable="true"\n    (dragstart)="onDragStart($event, card.kind)"\n  >\n    <i class="pi {{ card.icon }} card-icon"></i>\n    <div class="card-text">\n      <div class="card-title">{{ card.title }}</div>\n      <div class="card-subtitle">{{ card.subtitle }}</div>\n    </div>\n  </div>\n\n  <div class="palette-footer">\n    <button pButton type="button" label="Templates" class="p-button-outlined p-button-sm templates-btn" disabled></button>\n  </div>\n</div>\n', styles: ["/* src/app/features/workflows/node-palette.component.scss */\n:host {\n  display: block;\n  height: 100%;\n  flex-shrink: 0;\n}\n.palette-root {\n  height: 100%;\n  display: flex;\n  flex-direction: column;\n  width: 260px;\n  background: #fff;\n  border-right: 1px solid #e5e7eb;\n  padding: 1rem;\n  overflow-y: auto;\n}\n.palette-header {\n  font-size: 1.1rem;\n  font-weight: 700;\n  color: #111827;\n  margin-bottom: 0.4rem;\n}\n.palette-hint {\n  font-size: 0.7rem;\n  color: #9ca3af;\n  margin-bottom: 1rem;\n  line-height: 1.35;\n}\n.palette-card {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n  padding: 0.75rem;\n  border: 1px solid #e5e7eb;\n  border-radius: 10px;\n  margin-bottom: 0.65rem;\n  cursor: grab;\n  background: #fff;\n  transition: border-color 0.15s, background 0.15s;\n}\n.palette-card:hover {\n  border-color: #2563eb;\n  background: #f8fafc;\n}\n.palette-card.disabled {\n  cursor: default;\n  opacity: 0.55;\n}\n.palette-card.disabled:hover {\n  border-color: #e5e7eb;\n  background: #fff;\n}\n.card-icon {\n  flex-shrink: 0;\n  width: 34px;\n  height: 34px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  border-radius: 8px;\n  background: #eff6ff;\n  color: #2563eb;\n  font-size: 0.95rem;\n}\n.card-text {\n  display: flex;\n  flex-direction: column;\n  gap: 2px;\n}\n.card-title {\n  font-size: 0.85rem;\n  font-weight: 700;\n  color: #111827;\n}\n.card-subtitle {\n  font-size: 0.7rem;\n  color: #6b7280;\n}\n.palette-footer {\n  margin-top: auto;\n  padding-top: 1rem;\n}\n.templates-btn {\n  width: 100%;\n}\n/*# sourceMappingURL=node-palette.component.css.map */\n"] }]
  }], null, null);
})();
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && \u0275setClassDebugInfo(NodePaletteComponent, { className: "NodePaletteComponent", filePath: "app/features/workflows/node-palette.component.ts", lineNumber: 21 });
})();

// src/app/features/workflows/workflow-builder.component.ts
var _c0 = ["canvasWrapperEl"];
var _c1 = () => ({ width: "700px" });
var _c2 = () => ({ width: "600px" });
var _c3 = (a0, a1, a2, a3) => ({ "pi-file": a0, "pi-directions": a1, "pi-bolt": a2, "pi-flag": a3 });
var _c4 = () => ["true", "false"];
function WorkflowBuilderComponent_div_2_span_9_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.savedText);
  }
}
function WorkflowBuilderComponent_div_2_span_10_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1, " \xB7 ");
    \u0275\u0275elementEnd();
  }
}
function WorkflowBuilderComponent_div_2_span_11_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1("Version ", ctx_r2.draftVersion.versionNumber, " [Draft]");
  }
}
function WorkflowBuilderComponent_div_2_span_12_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1(" \xB7 Published: v", ctx_r2.workflow == null ? null : ctx_r2.workflow.publishedVersionNumber, "");
  }
}
function WorkflowBuilderComponent_div_2__svg_path_26_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275namespaceSVG();
    \u0275\u0275element(0, "path", 41);
  }
  if (rf & 2) {
    const edge_r4 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275attribute("d", ctx_r2.edgePath(edge_r4));
  }
}
function WorkflowBuilderComponent_div_2__svg_path_27_Template(rf, ctx) {
  if (rf & 1) {
    const _r5 = \u0275\u0275getCurrentView();
    \u0275\u0275namespaceSVG();
    \u0275\u0275elementStart(0, "path", 42);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2__svg_path_27_Template_path_click_0_listener($event) {
      const edge_r6 = \u0275\u0275restoreView(_r5).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.selectEdge(edge_r6, $event));
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const edge_r6 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275classProp("edge-hit-selected", ctx_r2.selectedEdgeId === edge_r6.id);
    \u0275\u0275attribute("d", ctx_r2.edgePath(edge_r6));
  }
}
function WorkflowBuilderComponent_div_2__svg_path_28_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275namespaceSVG();
    \u0275\u0275element(0, "path", 43);
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275attribute("d", ctx_r2.connectingLinePath());
  }
}
function WorkflowBuilderComponent_div_2_ng_container_29_div_1_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 46);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const edge_r7 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275property("ngStyle", ctx_r2.edgeLabelStyle(edge_r7));
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(edge_r7.label);
  }
}
function WorkflowBuilderComponent_div_2_ng_container_29_button_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r8 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "button", 47);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_ng_container_29_button_2_Template_button_click_0_listener($event) {
      \u0275\u0275restoreView(_r8);
      const edge_r7 = \u0275\u0275nextContext().$implicit;
      const ctx_r2 = \u0275\u0275nextContext(2);
      ctx_r2.deleteEdge(edge_r7.id);
      return \u0275\u0275resetView($event.stopPropagation());
    });
    \u0275\u0275text(1, "\u2715");
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const edge_r7 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275property("ngStyle", ctx_r2.edgeLabelStyle(edge_r7));
  }
}
function WorkflowBuilderComponent_div_2_ng_container_29_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementContainerStart(0);
    \u0275\u0275template(1, WorkflowBuilderComponent_div_2_ng_container_29_div_1_Template, 2, 2, "div", 44)(2, WorkflowBuilderComponent_div_2_ng_container_29_button_2_Template, 2, 1, "button", 45);
    \u0275\u0275elementContainerEnd();
  }
  if (rf & 2) {
    const edge_r7 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", edge_r7.label);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.selectedEdgeId === edge_r7.id);
  }
}
function WorkflowBuilderComponent_div_2_div_30_span_5_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span", 54);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(node_r10.type === "trigger" ? "Trigger" : node_r10.type === "condition" ? "Condition" : "Action");
  }
}
function WorkflowBuilderComponent_div_2_div_30_div_6_span_2_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = \u0275\u0275nextContext(2).$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.triggerBodyText(node_r10));
  }
}
function WorkflowBuilderComponent_div_2_div_30_div_6_span_3_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = \u0275\u0275nextContext(2).$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.conditionSummary(node_r10));
  }
}
function WorkflowBuilderComponent_div_2_div_30_div_6_span_4_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = \u0275\u0275nextContext(2).$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.actionBodyText(node_r10));
  }
}
function WorkflowBuilderComponent_div_2_div_30_div_6_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 55);
    \u0275\u0275elementContainerStart(1, 56);
    \u0275\u0275template(2, WorkflowBuilderComponent_div_2_div_30_div_6_span_2_Template, 2, 1, "span", 57)(3, WorkflowBuilderComponent_div_2_div_30_div_6_span_3_Template, 2, 1, "span", 57)(4, WorkflowBuilderComponent_div_2_div_30_div_6_span_4_Template, 2, 1, "span", 57);
    \u0275\u0275elementContainerEnd();
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitch", node_r10.type);
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "trigger");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "condition");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "action");
  }
}
function WorkflowBuilderComponent_div_2_div_30_Template(rf, ctx) {
  if (rf & 1) {
    const _r9 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 48);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_30_Template_div_click_0_listener($event) {
      const node_r10 = \u0275\u0275restoreView(_r9).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(2);
      ctx_r2.selectNode(node_r10);
      return \u0275\u0275resetView($event.stopPropagation());
    });
    \u0275\u0275elementStart(1, "div", 49);
    \u0275\u0275listener("mousedown", function WorkflowBuilderComponent_div_2_div_30_Template_div_mousedown_1_listener($event) {
      const node_r10 = \u0275\u0275restoreView(_r9).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.onNodeMouseDown($event, node_r10));
    });
    \u0275\u0275element(2, "i", 50);
    \u0275\u0275elementStart(3, "span", 51);
    \u0275\u0275text(4);
    \u0275\u0275elementEnd();
    \u0275\u0275template(5, WorkflowBuilderComponent_div_2_div_30_span_5_Template, 2, 1, "span", 52);
    \u0275\u0275elementEnd();
    \u0275\u0275template(6, WorkflowBuilderComponent_div_2_div_30_div_6_Template, 5, 4, "div", 53);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const node_r10 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275styleProp("left", node_r10.x, "px")("top", node_r10.y, "px")("width", ctx_r2.nodeWidth(node_r10), "px")("border-color", ctx_r2.nodeBorderColor(node_r10));
    \u0275\u0275classProp("node-end", node_r10.type === "end")("selected", (ctx_r2.selectedNode == null ? null : ctx_r2.selectedNode.id) === node_r10.id)("executing", ctx_r2.executingNodeId === node_r10.id);
    \u0275\u0275advance();
    \u0275\u0275styleProp("background", ctx_r2.nodeHeaderBg(node_r10));
    \u0275\u0275advance();
    \u0275\u0275property("ngClass", \u0275\u0275pureFunction4(20, _c3, node_r10.type === "trigger", node_r10.type === "condition", node_r10.type === "action", node_r10.type === "end"));
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(ctx_r2.nodeTypeLabel(node_r10));
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", node_r10.type !== "end");
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", node_r10.type !== "end");
  }
}
function WorkflowBuilderComponent_div_2_ng_container_31_div_1_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275element(0, "div", 60);
  }
  if (rf & 2) {
    const node_r11 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275styleProp("left", ctx_r2.inputPortPos(node_r11).x - 6, "px")("top", ctx_r2.inputPortPos(node_r11).y - 6, "px");
  }
}
function WorkflowBuilderComponent_div_2_ng_container_31_div_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r12 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 61);
    \u0275\u0275listener("mousedown", function WorkflowBuilderComponent_div_2_ng_container_31_div_2_Template_div_mousedown_0_listener($event) {
      const port_r13 = \u0275\u0275restoreView(_r12).$implicit;
      const node_r11 = \u0275\u0275nextContext().$implicit;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.onPortMouseDown($event, node_r11, port_r13.id));
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const port_r13 = ctx.$implicit;
    const node_r11 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275styleProp("left", ctx_r2.outputPortPos(node_r11, port_r13.id).x - 6, "px")("top", ctx_r2.outputPortPos(node_r11, port_r13.id).y - 6, "px");
    \u0275\u0275property("title", port_r13.label || "Drag to connect");
  }
}
function WorkflowBuilderComponent_div_2_ng_container_31_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementContainerStart(0);
    \u0275\u0275template(1, WorkflowBuilderComponent_div_2_ng_container_31_div_1_Template, 1, 4, "div", 58)(2, WorkflowBuilderComponent_div_2_ng_container_31_div_2_Template, 1, 5, "div", 59);
    \u0275\u0275elementContainerEnd();
  }
  if (rf & 2) {
    const node_r11 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.hasInputPort(node_r11));
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.outputPorts(node_r11));
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_18_div_5_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 7)(1, "label");
    \u0275\u0275text(2, "Webhook URL");
    \u0275\u0275elementEnd();
    \u0275\u0275element(3, "input", 75);
    \u0275\u0275elementStart(4, "span", 76);
    \u0275\u0275text(5, "POST JSON here to trigger the published version.");
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(4);
    \u0275\u0275advance(3);
    \u0275\u0275property("value", ctx_r2.webhookUrl());
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_18_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div")(1, "div", 7)(2, "label");
    \u0275\u0275text(3, "Trigger");
    \u0275\u0275elementEnd();
    \u0275\u0275element(4, "input", 73);
    \u0275\u0275elementEnd();
    \u0275\u0275template(5, WorkflowBuilderComponent_div_2_div_40_div_18_div_5_Template, 6, 1, "div", 74);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const sn_r15 = \u0275\u0275nextContext().ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance(4);
    \u0275\u0275property("value", ctx_r2.asTriggerData(sn_r15).label);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", (ctx_r2.workflow == null ? null : ctx_r2.workflow.triggerType) === "Webhook");
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_4_Template(rf, ctx) {
  if (rf & 1) {
    const _r21 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "p-dropdown", 94);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_4_Template_p_dropdown_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r21);
      const rule_r20 = \u0275\u0275nextContext().$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.value, $event) || (rule_r20.value = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_4_Template_p_dropdown_ngModelChange_0_listener() {
      \u0275\u0275restoreView(_r21);
      const ctx_r2 = \u0275\u0275nextContext(6);
      return \u0275\u0275resetView(ctx_r2.scheduleAutosave());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const rule_r20 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(5);
    \u0275\u0275property("options", \u0275\u0275pureFunction0(3, _c4));
    \u0275\u0275twoWayProperty("ngModel", rule_r20.value);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r22 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "p-dropdown", 94);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_5_Template_p_dropdown_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r22);
      const rule_r20 = \u0275\u0275nextContext().$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.value, $event) || (rule_r20.value = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_5_Template_p_dropdown_ngModelChange_0_listener() {
      \u0275\u0275restoreView(_r22);
      const ctx_r2 = \u0275\u0275nextContext(6);
      return \u0275\u0275resetView(ctx_r2.scheduleAutosave());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const rule_r20 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(5);
    \u0275\u0275property("options", ctx_r2.triggerFieldOptions(rule_r20));
    \u0275\u0275twoWayProperty("ngModel", rule_r20.value);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_6_Template(rf, ctx) {
  if (rf & 1) {
    const _r23 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "input", 95);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_6_Template_input_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r23);
      const rule_r20 = \u0275\u0275nextContext().$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.value, $event) || (rule_r20.value = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_6_Template_input_ngModelChange_0_listener() {
      \u0275\u0275restoreView(_r23);
      const ctx_r2 = \u0275\u0275nextContext(6);
      return \u0275\u0275resetView(ctx_r2.scheduleAutosave());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const rule_r20 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(5);
    \u0275\u0275twoWayProperty("ngModel", rule_r20.value);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r24 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "input", 96);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_7_Template_input_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r24);
      const rule_r20 = \u0275\u0275nextContext().$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.value, $event) || (rule_r20.value = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_7_Template_input_ngModelChange_0_listener() {
      \u0275\u0275restoreView(_r24);
      const ctx_r2 = \u0275\u0275nextContext(6);
      return \u0275\u0275resetView(ctx_r2.scheduleAutosave());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const rule_r20 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(5);
    \u0275\u0275twoWayProperty("ngModel", rule_r20.value);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r19 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 88)(1, "p-dropdown", 89);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template_p_dropdown_ngModelChange_1_listener($event) {
      const rule_r20 = \u0275\u0275restoreView(_r19).$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.field, $event) || (rule_r20.field = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("onChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template_p_dropdown_onChange_1_listener() {
      const rule_r20 = \u0275\u0275restoreView(_r19).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(5);
      return \u0275\u0275resetView(ctx_r2.onConditionFieldChange(rule_r20));
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(2, "p-dropdown", 90);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template_p_dropdown_ngModelChange_2_listener($event) {
      const rule_r20 = \u0275\u0275restoreView(_r19).$implicit;
      \u0275\u0275twoWayBindingSet(rule_r20.operator, $event) || (rule_r20.operator = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template_p_dropdown_ngModelChange_2_listener() {
      \u0275\u0275restoreView(_r19);
      const ctx_r2 = \u0275\u0275nextContext(5);
      return \u0275\u0275resetView(ctx_r2.scheduleAutosave());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementContainerStart(3, 56);
    \u0275\u0275template(4, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_4_Template, 1, 4, "p-dropdown", 91)(5, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_p_dropdown_5_Template, 1, 3, "p-dropdown", 91)(6, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_6_Template, 1, 2, "input", 92)(7, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_input_7_Template, 1, 2, "input", 93);
    \u0275\u0275elementContainerEnd();
    \u0275\u0275elementStart(8, "button", 85);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template_button_click_8_listener() {
      const ri_r25 = \u0275\u0275restoreView(_r19).index;
      const branch_r18 = \u0275\u0275nextContext().$implicit;
      const ctx_r2 = \u0275\u0275nextContext(4);
      return \u0275\u0275resetView(ctx_r2.removeCondition(branch_r18, ri_r25));
    });
    \u0275\u0275text(9, "\u2715");
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const rule_r20 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext(5);
    \u0275\u0275advance();
    \u0275\u0275property("options", ctx_r2.triggerFields);
    \u0275\u0275twoWayProperty("ngModel", rule_r20.field);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
    \u0275\u0275advance();
    \u0275\u0275property("options", ctx_r2.operatorOptionsFor(rule_r20));
    \u0275\u0275twoWayProperty("ngModel", rule_r20.operator);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitch", ctx_r2.valueKind(rule_r20));
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "boolean");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "options");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "number");
    \u0275\u0275advance(2);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template(rf, ctx) {
  if (rf & 1) {
    const _r17 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 82)(1, "div", 83)(2, "input", 84);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template_input_ngModelChange_2_listener($event) {
      const branch_r18 = \u0275\u0275restoreView(_r17).$implicit;
      \u0275\u0275twoWayBindingSet(branch_r18.name, $event) || (branch_r18.name = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template_input_ngModelChange_2_listener($event) {
      const branch_r18 = \u0275\u0275restoreView(_r17).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(4);
      return \u0275\u0275resetView(ctx_r2.renameBranch(branch_r18, $event));
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(3, "button", 85);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template_button_click_3_listener() {
      const branch_r18 = \u0275\u0275restoreView(_r17).$implicit;
      const sn_r15 = \u0275\u0275nextContext(2).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.removeBranch(sn_r15, branch_r18.id));
    });
    \u0275\u0275text(4, "\u2715");
    \u0275\u0275elementEnd()();
    \u0275\u0275template(5, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_div_5_Template, 10, 11, "div", 86);
    \u0275\u0275elementStart(6, "button", 87);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template_button_click_6_listener() {
      const branch_r18 = \u0275\u0275restoreView(_r17).$implicit;
      const ctx_r2 = \u0275\u0275nextContext(4);
      return \u0275\u0275resetView(ctx_r2.addCondition(branch_r18));
    });
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const branch_r18 = ctx.$implicit;
    const sn_r15 = \u0275\u0275nextContext(2).ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance(2);
    \u0275\u0275twoWayProperty("ngModel", branch_r18.name);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
    \u0275\u0275advance();
    \u0275\u0275property("disabled", ctx_r2.previewMode || ctx_r2.asConditionData(sn_r15).branches.length <= 1);
    \u0275\u0275advance(2);
    \u0275\u0275property("ngForOf", branch_r18.conditions);
    \u0275\u0275advance();
    \u0275\u0275property("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_7_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r26 = \u0275\u0275getCurrentView();
    \u0275\u0275elementContainerStart(0);
    \u0275\u0275elementStart(1, "span", 98);
    \u0275\u0275text(2);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(3, "span", 99);
    \u0275\u0275text(4);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "button", 100);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_div_19_div_7_ng_container_1_Template_button_click_5_listener() {
      const be_r27 = \u0275\u0275restoreView(_r26).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(5);
      return \u0275\u0275resetView(ctx_r2.jumpToNode(be_r27.target));
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementContainerEnd();
  }
  if (rf & 2) {
    const be_r27 = ctx.ngIf;
    const branch_r28 = \u0275\u0275nextContext().$implicit;
    const ctx_r2 = \u0275\u0275nextContext(4);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(branch_r28.name);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(be_r27.target && ctx_r2.nodeById(be_r27.target) ? ctx_r2.nodeTypeLabel(ctx_r2.nodeById(be_r27.target)) : "");
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_div_7_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 97);
    \u0275\u0275template(1, WorkflowBuilderComponent_div_2_div_40_div_19_div_7_ng_container_1_Template, 6, 2, "ng-container", 17);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const branch_r28 = ctx.$implicit;
    const sn_r15 = \u0275\u0275nextContext(2).ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.edgeForBranch(sn_r15.id, branch_r28.id));
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_19_Template(rf, ctx) {
  if (rf & 1) {
    const _r16 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div")(1, "div", 77);
    \u0275\u0275text(2, "Branches");
    \u0275\u0275elementEnd();
    \u0275\u0275template(3, WorkflowBuilderComponent_div_2_div_40_div_19_div_3_Template, 7, 5, "div", 78);
    \u0275\u0275elementStart(4, "button", 79);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_div_19_Template_button_click_4_listener() {
      \u0275\u0275restoreView(_r16);
      const sn_r15 = \u0275\u0275nextContext().ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.addBranch(sn_r15));
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "div", 80);
    \u0275\u0275text(6, "Next Step");
    \u0275\u0275elementEnd();
    \u0275\u0275template(7, WorkflowBuilderComponent_div_2_div_40_div_19_div_7_Template, 2, 1, "div", 81);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const sn_r15 = \u0275\u0275nextContext().ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance(3);
    \u0275\u0275property("ngForOf", ctx_r2.asConditionData(sn_r15).branches);
    \u0275\u0275advance();
    \u0275\u0275property("disabled", ctx_r2.previewMode);
    \u0275\u0275advance(3);
    \u0275\u0275property("ngForOf", ctx_r2.asConditionData(sn_r15).branches);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_20_div_5_p_dropdown_3_Template(rf, ctx) {
  if (rf & 1) {
    const _r30 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "p-dropdown", 94);
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_20_div_5_p_dropdown_3_Template_p_dropdown_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r30);
      const f_r31 = \u0275\u0275nextContext().$implicit;
      const sn_r15 = \u0275\u0275nextContext(2).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.updateConfigValue(sn_r15, f_r31.key, $event));
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const f_r31 = \u0275\u0275nextContext().$implicit;
    const sn_r15 = \u0275\u0275nextContext(2).ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275property("options", f_r31.options)("ngModel", ctx_r2.asActionData(sn_r15).config[f_r31.key])("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_20_div_5_input_4_Template(rf, ctx) {
  if (rf & 1) {
    const _r32 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "input", 105);
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_20_div_5_input_4_Template_input_ngModelChange_0_listener($event) {
      \u0275\u0275restoreView(_r32);
      const f_r31 = \u0275\u0275nextContext().$implicit;
      const sn_r15 = \u0275\u0275nextContext(2).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.updateConfigValue(sn_r15, f_r31.key, $event));
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const f_r31 = \u0275\u0275nextContext().$implicit;
    const sn_r15 = \u0275\u0275nextContext(2).ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275property("type", f_r31.type === "number" ? "number" : "text")("ngModel", ctx_r2.asActionData(sn_r15).config[f_r31.key])("disabled", ctx_r2.previewMode);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_20_div_5_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 7)(1, "label");
    \u0275\u0275text(2);
    \u0275\u0275elementEnd();
    \u0275\u0275template(3, WorkflowBuilderComponent_div_2_div_40_div_20_div_5_p_dropdown_3_Template, 1, 3, "p-dropdown", 103)(4, WorkflowBuilderComponent_div_2_div_40_div_20_div_5_input_4_Template, 1, 3, "input", 104);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const f_r31 = ctx.$implicit;
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(f_r31.label);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", f_r31.options && f_r31.options.length);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", !f_r31.options || !f_r31.options.length);
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_20_Template(rf, ctx) {
  if (rf & 1) {
    const _r29 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div")(1, "div", 7)(2, "label");
    \u0275\u0275text(3, "Action Type");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(4, "p-dropdown", 101);
    \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_div_20_Template_p_dropdown_ngModelChange_4_listener($event) {
      \u0275\u0275restoreView(_r29);
      const sn_r15 = \u0275\u0275nextContext().ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      \u0275\u0275twoWayBindingSet(ctx_r2.asActionData(sn_r15).actionType, $event) || (ctx_r2.asActionData(sn_r15).actionType = $event);
      return \u0275\u0275resetView($event);
    });
    \u0275\u0275listener("onChange", function WorkflowBuilderComponent_div_2_div_40_div_20_Template_p_dropdown_onChange_4_listener() {
      \u0275\u0275restoreView(_r29);
      const sn_r15 = \u0275\u0275nextContext().ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.onActionTypeChange(sn_r15));
    });
    \u0275\u0275elementEnd()();
    \u0275\u0275template(5, WorkflowBuilderComponent_div_2_div_40_div_20_div_5_Template, 5, 3, "div", 102);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const sn_r15 = \u0275\u0275nextContext().ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance(4);
    \u0275\u0275property("options", ctx_r2.actionTypes);
    \u0275\u0275twoWayProperty("ngModel", ctx_r2.asActionData(sn_r15).actionType);
    \u0275\u0275property("disabled", ctx_r2.previewMode);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.configFieldsFor(sn_r15));
  }
}
function WorkflowBuilderComponent_div_2_div_40_div_21_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div")(1, "p", 76);
    \u0275\u0275text(2, "This path ends here. No further configuration is needed.");
    \u0275\u0275elementEnd()();
  }
}
function WorkflowBuilderComponent_div_2_div_40_Template(rf, ctx) {
  if (rf & 1) {
    const _r14 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 62)(1, "div", 63)(2, "span");
    \u0275\u0275text(3, "Properties");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(4, "div", 64)(5, "button", 65);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_Template_button_click_5_listener() {
      const sn_r15 = \u0275\u0275restoreView(_r14).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.deleteNode(sn_r15));
    });
    \u0275\u0275element(6, "i", 66);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(7, "button", 67);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_div_40_Template_button_click_7_listener() {
      \u0275\u0275restoreView(_r14);
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.closePanel());
    });
    \u0275\u0275element(8, "i", 68);
    \u0275\u0275elementEnd()()();
    \u0275\u0275elementStart(9, "div", 69)(10, "span", 70);
    \u0275\u0275text(11);
    \u0275\u0275elementEnd();
    \u0275\u0275element(12, "button", 71);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(13, "div", 7)(14, "label");
    \u0275\u0275text(15, "Description");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(16, "input", 72);
    \u0275\u0275listener("ngModelChange", function WorkflowBuilderComponent_div_2_div_40_Template_input_ngModelChange_16_listener($event) {
      const sn_r15 = \u0275\u0275restoreView(_r14).ngIf;
      const ctx_r2 = \u0275\u0275nextContext(2);
      return \u0275\u0275resetView(ctx_r2.onDescriptionChange(sn_r15, $event));
    });
    \u0275\u0275elementEnd()();
    \u0275\u0275elementContainerStart(17, 56);
    \u0275\u0275template(18, WorkflowBuilderComponent_div_2_div_40_div_18_Template, 6, 2, "div", 57)(19, WorkflowBuilderComponent_div_2_div_40_div_19_Template, 8, 3, "div", 57)(20, WorkflowBuilderComponent_div_2_div_40_div_20_Template, 6, 4, "div", 57)(21, WorkflowBuilderComponent_div_2_div_40_div_21_Template, 3, 0, "div", 57);
    \u0275\u0275elementContainerEnd();
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const sn_r15 = ctx.ngIf;
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance(5);
    \u0275\u0275property("disabled", ctx_r2.previewMode || sn_r15.type === "trigger");
    \u0275\u0275advance(5);
    \u0275\u0275styleProp("background", ctx_r2.nodeHeaderBg(sn_r15))("border-color", ctx_r2.nodeBorderColor(sn_r15));
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.nodeTypeLabel(sn_r15));
    \u0275\u0275advance();
    \u0275\u0275property("disabled", true);
    \u0275\u0275advance(4);
    \u0275\u0275property("ngModel", ctx_r2.descriptionOf(sn_r15))("disabled", ctx_r2.previewMode);
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitch", sn_r15.type);
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "trigger");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "condition");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "action");
    \u0275\u0275advance();
    \u0275\u0275property("ngSwitchCase", "end");
  }
}
function WorkflowBuilderComponent_div_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r2 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "div", 11)(1, "div", 12)(2, "div", 13)(3, "div", 14)(4, "span", 15);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_span_click_4_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.backToList());
    });
    \u0275\u0275text(5, "Workflows");
    \u0275\u0275elementEnd()();
    \u0275\u0275elementStart(6, "h1");
    \u0275\u0275text(7);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(8, "div", 16);
    \u0275\u0275template(9, WorkflowBuilderComponent_div_2_span_9_Template, 2, 1, "span", 17)(10, WorkflowBuilderComponent_div_2_span_10_Template, 2, 0, "span", 17)(11, WorkflowBuilderComponent_div_2_span_11_Template, 2, 1, "span", 17)(12, WorkflowBuilderComponent_div_2_span_12_Template, 2, 1, "span", 17);
    \u0275\u0275elementEnd()();
    \u0275\u0275elementStart(13, "div", 18)(14, "button", 19);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_14_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.undo());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(15, "button", 20);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_15_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.redo());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(16, "button", 21);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_16_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.openRunDialog());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(17, "button", 22);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_17_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.openExecutions());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(18, "button", 23);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_18_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.togglePreview());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(19, "button", 24);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_19_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.confirmPublish());
    });
    \u0275\u0275elementEnd()()();
    \u0275\u0275elementStart(20, "div", 25);
    \u0275\u0275element(21, "app-node-palette");
    \u0275\u0275elementStart(22, "div", 26, 1);
    \u0275\u0275listener("wheel", function WorkflowBuilderComponent_div_2_Template_div_wheel_22_listener($event) {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onCanvasWheel($event));
    })("mousedown", function WorkflowBuilderComponent_div_2_Template_div_mousedown_22_listener($event) {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onCanvasMouseDown($event));
    });
    \u0275\u0275elementStart(24, "div", 27);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_div_click_24_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onCanvasClick());
    })("dragover", function WorkflowBuilderComponent_div_2_Template_div_dragover_24_listener($event) {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onCanvasDragOver($event));
    })("drop", function WorkflowBuilderComponent_div_2_Template_div_drop_24_listener($event) {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.onCanvasDrop($event));
    });
    \u0275\u0275namespaceSVG();
    \u0275\u0275elementStart(25, "svg", 28);
    \u0275\u0275template(26, WorkflowBuilderComponent_div_2__svg_path_26_Template, 1, 1, "path", 29)(27, WorkflowBuilderComponent_div_2__svg_path_27_Template, 1, 3, "path", 30)(28, WorkflowBuilderComponent_div_2__svg_path_28_Template, 1, 1, "path", 31);
    \u0275\u0275elementEnd();
    \u0275\u0275template(29, WorkflowBuilderComponent_div_2_ng_container_29_Template, 3, 2, "ng-container", 32)(30, WorkflowBuilderComponent_div_2_div_30_Template, 7, 25, "div", 33)(31, WorkflowBuilderComponent_div_2_ng_container_31_Template, 3, 2, "ng-container", 32);
    \u0275\u0275elementEnd();
    \u0275\u0275namespaceHTML();
    \u0275\u0275elementStart(32, "div", 34)(33, "button", 35);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_33_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.zoomIn());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(34, "button", 36);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_34_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.zoomOut());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(35, "span", 37);
    \u0275\u0275text(36);
    \u0275\u0275pipe(37, "number");
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(38, "button", 38);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_38_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.resetZoom());
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(39, "button", 39);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_div_2_Template_button_click_39_listener() {
      \u0275\u0275restoreView(_r2);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.fitToView());
    });
    \u0275\u0275elementEnd()()();
    \u0275\u0275template(40, WorkflowBuilderComponent_div_2_div_40_Template, 22, 14, "div", 40);
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance(7);
    \u0275\u0275textInterpolate(ctx_r2.workflow == null ? null : ctx_r2.workflow.name);
    \u0275\u0275advance(2);
    \u0275\u0275property("ngIf", ctx_r2.savedText);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.savedText);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.draftVersion);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.workflow == null ? null : ctx_r2.workflow.publishedVersionNumber);
    \u0275\u0275advance(2);
    \u0275\u0275property("disabled", !ctx_r2.canUndo());
    \u0275\u0275advance();
    \u0275\u0275property("disabled", !ctx_r2.canRedo());
    \u0275\u0275advance(3);
    \u0275\u0275property("label", ctx_r2.previewMode ? "Exit Preview" : "Preview");
    \u0275\u0275advance(6);
    \u0275\u0275styleProp("width", ctx_r2.canvasWidth(), "px")("height", ctx_r2.canvasHeight(), "px")("transform", ctx_r2.canvasTransform());
    \u0275\u0275advance();
    \u0275\u0275attribute("width", ctx_r2.canvasWidth())("height", ctx_r2.canvasHeight());
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.graph.edges);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.graph.edges);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.connectingFrom);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.graph.edges);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.graph.nodes)("ngForTrackBy", ctx_r2.trackById);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.graph.nodes);
    \u0275\u0275advance(5);
    \u0275\u0275textInterpolate1("", \u0275\u0275pipeBind2(37, 25, ctx_r2.viewport.scale * 100, "1.0-0"), "%");
    \u0275\u0275advance(4);
    \u0275\u0275property("ngIf", ctx_r2.showPropertiesPanel && ctx_r2.selectedNode);
  }
}
function WorkflowBuilderComponent_ng_template_3_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 106);
    \u0275\u0275text(1, "Loading workflow\u2026");
    \u0275\u0275elementEnd();
  }
}
function WorkflowBuilderComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 76);
    \u0275\u0275text(1, "Loading\u2026");
    \u0275\u0275elementEnd();
  }
}
function WorkflowBuilderComponent_div_7_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 76);
    \u0275\u0275text(1, "No executions yet.");
    \u0275\u0275elementEnd();
  }
}
function WorkflowBuilderComponent_div_8_div_8_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 111);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ex_r33 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ex_r33.errorMessage);
  }
}
function WorkflowBuilderComponent_div_8_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 107)(1, "div", 108);
    \u0275\u0275element(2, "p-tag", 109);
    \u0275\u0275elementStart(3, "span");
    \u0275\u0275text(4);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(5, "span", 76);
    \u0275\u0275text(6);
    \u0275\u0275pipe(7, "date");
    \u0275\u0275elementEnd()();
    \u0275\u0275template(8, WorkflowBuilderComponent_div_8_div_8_Template, 2, 1, "div", 110);
    \u0275\u0275elementStart(9, "pre");
    \u0275\u0275text(10);
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const ex_r33 = ctx.$implicit;
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance(2);
    \u0275\u0275property("value", ex_r33.status)("severity", ctx_r2.executionSeverity(ex_r33.status));
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(ex_r33.triggerEntityType);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(\u0275\u0275pipeBind2(7, 6, ex_r33.createdDate, "medium"));
    \u0275\u0275advance(2);
    \u0275\u0275property("ngIf", ex_r33.errorMessage);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(ctx_r2.prettyPath(ex_r33.pathJson));
  }
}
function WorkflowBuilderComponent_div_14_div_1_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 76);
    \u0275\u0275text(1, "Running live\u2026");
    \u0275\u0275elementEnd();
  }
}
function WorkflowBuilderComponent_div_14_div_2_span_5_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const step_r34 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1(" \u2192 ", step_r34.matchedBranchName, "");
  }
}
function WorkflowBuilderComponent_div_14_div_2_span_6_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "span");
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const step_r34 = \u0275\u0275nextContext().$implicit;
    \u0275\u0275advance();
    \u0275\u0275textInterpolate1(" \u2014 ", step_r34.result, "");
  }
}
function WorkflowBuilderComponent_div_14_div_2_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 114)(1, "span", 115);
    \u0275\u0275text(2);
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(3, "span", 116);
    \u0275\u0275text(4);
    \u0275\u0275elementEnd();
    \u0275\u0275template(5, WorkflowBuilderComponent_div_14_div_2_span_5_Template, 2, 1, "span", 17)(6, WorkflowBuilderComponent_div_14_div_2_span_6_Template, 2, 1, "span", 17);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const step_r34 = ctx.$implicit;
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(step_r34.type);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(step_r34.node);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", step_r34.matchedBranchName);
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", step_r34.result);
  }
}
function WorkflowBuilderComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 112);
    \u0275\u0275template(1, WorkflowBuilderComponent_div_14_div_1_Template, 2, 0, "div", 4)(2, WorkflowBuilderComponent_div_14_div_2_Template, 7, 4, "div", 113);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.running);
    \u0275\u0275advance();
    \u0275\u0275property("ngForOf", ctx_r2.liveSteps);
  }
}
function WorkflowBuilderComponent_div_15_div_2_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 111);
    \u0275\u0275text(1);
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext(2);
    \u0275\u0275advance();
    \u0275\u0275textInterpolate(ctx_r2.lastRunResult.errorMessage);
  }
}
function WorkflowBuilderComponent_div_15_Template(rf, ctx) {
  if (rf & 1) {
    \u0275\u0275elementStart(0, "div", 112);
    \u0275\u0275element(1, "p-tag", 109);
    \u0275\u0275template(2, WorkflowBuilderComponent_div_15_div_2_Template, 2, 1, "div", 110);
    \u0275\u0275elementStart(3, "pre");
    \u0275\u0275text(4);
    \u0275\u0275elementEnd()();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance();
    \u0275\u0275property("value", ctx_r2.lastRunResult.status)("severity", ctx_r2.executionSeverity(ctx_r2.lastRunResult.status));
    \u0275\u0275advance();
    \u0275\u0275property("ngIf", ctx_r2.lastRunResult.errorMessage);
    \u0275\u0275advance(2);
    \u0275\u0275textInterpolate(ctx_r2.prettyPath(ctx_r2.lastRunResult.pathJson));
  }
}
function WorkflowBuilderComponent_ng_template_16_Template(rf, ctx) {
  if (rf & 1) {
    const _r35 = \u0275\u0275getCurrentView();
    \u0275\u0275elementStart(0, "button", 117);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_ng_template_16_Template_button_click_0_listener() {
      \u0275\u0275restoreView(_r35);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.showRunDialog = false);
    });
    \u0275\u0275elementEnd();
    \u0275\u0275elementStart(1, "button", 118);
    \u0275\u0275listener("click", function WorkflowBuilderComponent_ng_template_16_Template_button_click_1_listener() {
      \u0275\u0275restoreView(_r35);
      const ctx_r2 = \u0275\u0275nextContext();
      return \u0275\u0275resetView(ctx_r2.runNow());
    });
    \u0275\u0275elementEnd();
  }
  if (rf & 2) {
    const ctx_r2 = \u0275\u0275nextContext();
    \u0275\u0275advance();
    \u0275\u0275property("loading", ctx_r2.running);
  }
}
var NODE_WIDTH = 220;
var WorkflowBuilderComponent = class _WorkflowBuilderComponent {
  route;
  router;
  api;
  notify;
  confirmation;
  canvasWrapperRef;
  workflowId;
  workflow = null;
  draftVersion = null;
  graph = { nodes: [], edges: [] };
  loading = false;
  previewMode = false;
  triggerTypes = [];
  actionTypes = [];
  triggerFields = [];
  selectedNode = null;
  showPropertiesPanel = false;
  selectedEdgeId = null;
  lastSavedAt = null;
  savedText = "";
  showExecutionsDialog = false;
  executions = [];
  executionsLoading = false;
  showRunDialog = false;
  running = false;
  runContextJson = "{}";
  lastRunResult = null;
  draggingNode = null;
  dragOffsetX = 0;
  dragOffsetY = 0;
  connectingFrom = null;
  connectingCursor = null;
  // ---------------- Zoom / pan ----------------
  viewport = { scale: 1, x: 0, y: 0 };
  isPanning = false;
  panStartClientX = 0;
  panStartClientY = 0;
  panStartOffsetX = 0;
  panStartOffsetY = 0;
  // ---------------- Undo / redo ----------------
  historyStack = [];
  historyIndex = -1;
  maxHistory = 50;
  // ---------------- Copy / paste ----------------
  clipboardNode = null;
  // ---------------- Live Test Run streaming ----------------
  liveSteps = [];
  executingNodeId = null;
  autosave$ = new Subject();
  subs = [];
  tickHandle = null;
  operatorsByType = {
    number: ["Equals", "NotEquals", "GreaterThan", "GreaterThanOrEqual", "LessThan", "LessThanOrEqual"],
    boolean: ["Equals", "NotEquals"],
    string: ["Equals", "NotEquals", "Contains"]
  };
  constructor(route, router, api, notify, confirmation) {
    this.route = route;
    this.router = router;
    this.api = api;
    this.notify = notify;
    this.confirmation = confirmation;
  }
  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get("id");
    this.workflowId = Number(idParam);
    this.loadAll();
    this.subs.push(this.autosave$.pipe(debounceTime(1e3)).subscribe(() => this.persistDraft()));
    this.tickHandle = setInterval(() => this.computeSavedText(), 15e3);
  }
  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
    if (this.tickHandle)
      clearInterval(this.tickHandle);
  }
  // ---------------- Load ----------------
  loadAll() {
    this.loading = true;
    forkJoin({
      workflow: this.api.getWorkflow(this.workflowId),
      draft: this.api.getDraft(this.workflowId),
      triggers: this.api.getTriggerTypes(),
      actions: this.api.getActionTypes()
    }).subscribe({
      next: ({ workflow, draft, triggers, actions }) => {
        this.workflow = workflow;
        this.draftVersion = draft;
        this.triggerTypes = triggers;
        this.actionTypes = actions;
        this.triggerFields = triggers.find((t) => t.key === workflow.triggerType)?.fields ?? [];
        try {
          const parsed = JSON.parse(draft.graphJson || "{}");
          this.graph = { nodes: parsed.nodes ?? [], edges: parsed.edges ?? [] };
        } catch {
          this.graph = { nodes: [], edges: [] };
        }
        this.historyStack = [JSON.stringify(this.graph)];
        this.historyIndex = 0;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.notify.error(err.error?.message ?? "Failed to load workflow.");
      }
    });
  }
  // ---------------- Autosave ----------------
  scheduleAutosave() {
    if (this.previewMode)
      return;
    this.autosave$.next();
  }
  persistDraft() {
    this.api.saveDraft(this.workflowId, { graphJson: JSON.stringify(this.graph) }).subscribe({
      next: (version) => {
        this.draftVersion = version;
        this.lastSavedAt = /* @__PURE__ */ new Date();
        this.computeSavedText();
      },
      error: (err) => this.notify.error(err.error?.message ?? "Failed to autosave workflow.")
    });
  }
  computeSavedText() {
    if (!this.lastSavedAt) {
      this.savedText = "";
      return;
    }
    const diffMs = Date.now() - this.lastSavedAt.getTime();
    const mins = Math.floor(diffMs / 6e4);
    if (mins < 1)
      this.savedText = "Saved just now";
    else if (mins === 1)
      this.savedText = "Saved 1 min ago";
    else
      this.savedText = `Saved ${mins} min ago`;
  }
  // ---------------- Undo / redo ----------------
  // Snapshots are taken before each discrete structural mutation (add/delete/move/connect a node
  // or branch, copy/paste) — not on every field edit in the properties panel.
  pushHistory() {
    const snapshot = JSON.stringify(this.graph);
    if (this.historyStack[this.historyIndex] === snapshot)
      return;
    this.historyStack = this.historyStack.slice(0, this.historyIndex + 1);
    this.historyStack.push(snapshot);
    if (this.historyStack.length > this.maxHistory)
      this.historyStack.shift();
    this.historyIndex = this.historyStack.length - 1;
  }
  canUndo() {
    return this.historyIndex > 0;
  }
  canRedo() {
    return this.historyIndex < this.historyStack.length - 1;
  }
  undo() {
    if (!this.canUndo())
      return;
    this.historyIndex--;
    this.graph = JSON.parse(this.historyStack[this.historyIndex]);
    this.closePanel();
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }
  redo() {
    if (!this.canRedo())
      return;
    this.historyIndex++;
    this.graph = JSON.parse(this.historyStack[this.historyIndex]);
    this.closePanel();
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }
  // ---------------- Copy / paste ----------------
  copyNode() {
    if (!this.selectedNode || this.selectedNode.type === "trigger")
      return;
    this.clipboardNode = JSON.parse(JSON.stringify(this.selectedNode));
    this.notify.info("Node copied.");
  }
  pasteNode() {
    if (!this.clipboardNode)
      return;
    this.pushHistory();
    const clone = JSON.parse(JSON.stringify(this.clipboardNode));
    clone.id = this.newId("node");
    clone.x += 40;
    clone.y += 40;
    if (clone.type === "condition") {
      clone.data.branches = clone.data.branches.map((b) => __spreadProps(__spreadValues({}, b), {
        id: this.newId("branch")
      }));
    }
    this.graph.nodes.push(clone);
    this.selectNode(clone);
    this.scheduleAutosave();
  }
  // ---------------- Keyboard shortcuts ----------------
  onDocKeyDown(event) {
    if (this.previewMode)
      return;
    const target = event.target;
    const isTyping = !!target && (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable);
    if (isTyping)
      return;
    const ctrl = event.ctrlKey || event.metaKey;
    const key = event.key.toLowerCase();
    if (ctrl && key === "z") {
      event.preventDefault();
      if (event.shiftKey)
        this.redo();
      else
        this.undo();
      return;
    }
    if (ctrl && key === "y") {
      event.preventDefault();
      this.redo();
      return;
    }
    if (ctrl && key === "c") {
      if (this.selectedNode) {
        event.preventDefault();
        this.copyNode();
      }
      return;
    }
    if (ctrl && key === "v") {
      if (this.clipboardNode) {
        event.preventDefault();
        this.pasteNode();
      }
      return;
    }
    if (event.key === "Delete" || event.key === "Backspace") {
      if (this.selectedNode) {
        event.preventDefault();
        this.deleteNode(this.selectedNode);
      } else if (this.selectedEdgeId) {
        event.preventDefault();
        this.deleteEdge(this.selectedEdgeId);
      }
    }
  }
  // ---------------- Graph helpers ----------------
  nodeById(id) {
    return this.graph.nodes.find((n) => n.id === id);
  }
  outgoingEdges(nodeId) {
    return this.graph.edges.filter((e) => e.source === nodeId);
  }
  incomingEdge(nodeId) {
    return this.graph.edges.find((e) => e.target === nodeId);
  }
  nodeHeight(node) {
    return node.type === "end" ? 56 : 96;
  }
  nodeWidth(node) {
    if (node.type === "end")
      return 120;
    if (node.type === "condition") {
      const branchCount = this.asConditionData(node).branches?.length ?? 1;
      return Math.max(NODE_WIDTH, branchCount * 130);
    }
    return NODE_WIDTH;
  }
  nodeBorderColor(node) {
    switch (node.type) {
      case "trigger":
        return "#fca5a5";
      case "condition":
        return "#c4b5fd";
      case "end":
        return "#9ca3af";
      case "action":
        return "#93c5fd";
      default:
        return "#e5e7eb";
    }
  }
  nodeHeaderBg(node) {
    switch (node.type) {
      case "trigger":
        return "#fef2f2";
      case "condition":
        return "#f5f3ff";
      case "end":
        return "#f3f4f6";
      case "action":
        return "#eff6ff";
      default:
        return "#f9fafb";
    }
  }
  nodeTypeLabel(node) {
    switch (node.type) {
      case "trigger":
        return "Trigger";
      case "condition":
        return "Decision";
      case "end":
        return "End";
      case "action":
        return node.data.label || "Action";
      default:
        return "";
    }
  }
  triggerBodyText(node) {
    return node.data.label || "Not configured";
  }
  conditionSummary(node) {
    const branches = this.asConditionData(node).branches ?? [];
    if (branches.length === 0)
      return "Not configured";
    return branches.map((b) => `\u2192 ${b.name}`).join("  /  ");
  }
  actionBodyText(node) {
    const data = node.data;
    if (!data.actionType)
      return "Not configured";
    switch (data.actionType) {
      case "HttpRequest":
        return `HTTP ${data.config?.["method"] ?? "GET"} ${data.config?.["url"] ?? ""}`;
      case "SetVariable":
        return `Set ${data.config?.["key"] ?? ""}`;
      case "LogMessage":
        return `Log: ${data.config?.["message"] ?? ""}`;
      case "Delay":
        return `Wait ${data.config?.["seconds"] ?? "0"}s`;
      case "Notify":
        return `Notify: ${data.config?.["title"] ?? ""}`;
      default:
        return data.label || data.actionType;
    }
  }
  trackById(_index, node) {
    return node.id;
  }
  triggerFieldOptions(rule) {
    return this.triggerFields.find((f) => f.key === rule.field)?.options ?? [];
  }
  newId(prefix) {
    return `${prefix}-${Date.now()}-${Math.floor(Math.random() * 1e5)}`;
  }
  // ---------------- Ports ----------------
  outputPorts(node) {
    if (node.type === "end")
      return [];
    if (node.type === "condition") {
      return (this.asConditionData(node).branches ?? []).map((b) => ({ id: b.id, label: b.name }));
    }
    return [{ id: "out", label: "" }];
  }
  hasInputPort(node) {
    return node.type !== "trigger";
  }
  outputPortPos(node, portId) {
    const ports = this.outputPorts(node);
    const idx = Math.max(0, ports.findIndex((p) => p.id === portId));
    const count = Math.max(1, ports.length);
    const width = this.nodeWidth(node);
    return { x: node.x + (idx + 1) * width / (count + 1), y: node.y + this.nodeHeight(node) };
  }
  inputPortPos(node) {
    return { x: node.x + this.nodeWidth(node) / 2, y: node.y };
  }
  portIdOfEdge(edge) {
    return edge.branch ?? "out";
  }
  nodeAtPoint(x, y, excludeId) {
    return this.graph.nodes.find((n) => {
      if (n.id === excludeId)
        return false;
      const w = this.nodeWidth(n);
      const h = this.nodeHeight(n);
      return x >= n.x - 10 && x <= n.x + w + 10 && y >= n.y - 10 && y <= n.y + h + 10;
    });
  }
  // ---------------- SVG edge rendering ----------------
  edgePath(edge) {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t)
      return "";
    const { x: sx, y: sy } = this.outputPortPos(s, this.portIdOfEdge(edge));
    const { x: tx, y: ty } = this.inputPortPos(t);
    const my = sy + (ty - sy) / 2;
    return `M ${sx} ${sy} L ${sx} ${my} L ${tx} ${my} L ${tx} ${ty}`;
  }
  edgeLabelStyle(edge) {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t)
      return { display: "none" };
    const { x: sx, y: sy } = this.outputPortPos(s, this.portIdOfEdge(edge));
    const { x: tx, y: ty } = this.inputPortPos(t);
    const my = sy + (ty - sy) / 2;
    const midX = (sx + tx) / 2;
    return { left: `${midX}px`, top: `${my}px` };
  }
  connectingLinePath() {
    if (!this.connectingFrom || !this.connectingCursor)
      return "";
    const s = this.nodeById(this.connectingFrom.nodeId);
    if (!s)
      return "";
    const { x: sx, y: sy } = this.outputPortPos(s, this.connectingFrom.portId);
    return `M ${sx} ${sy} L ${this.connectingCursor.x} ${this.connectingCursor.y}`;
  }
  canvasWidth() {
    const maxX = Math.max(1200, ...this.graph.nodes.map((n) => n.x + this.nodeWidth(n) + 100));
    return maxX;
  }
  canvasHeight() {
    const maxY = Math.max(900, ...this.graph.nodes.map((n) => n.y + this.nodeHeight(n) + 150));
    return maxY;
  }
  // ---------------- Selection / properties panel ----------------
  selectNode(node) {
    this.selectedEdgeId = null;
    this.selectedNode = node;
    this.showPropertiesPanel = true;
  }
  closePanel() {
    this.selectedNode = null;
    this.showPropertiesPanel = false;
  }
  asTriggerData(node) {
    return node.data;
  }
  asConditionData(node) {
    return node.data;
  }
  asActionData(node) {
    return node.data;
  }
  descriptionOf(node) {
    return node.data.description ?? "";
  }
  onDescriptionChange(node, value) {
    node.data.description = value;
    this.scheduleAutosave();
  }
  // ---------------- Condition editing ----------------
  operatorOptionsFor(rule) {
    const field = this.triggerFields.find((f) => f.key === rule.field);
    const type = field?.type ?? "string";
    return this.operatorsByType[type] ?? this.operatorsByType["string"];
  }
  valueKind(rule) {
    const field = this.triggerFields.find((f) => f.key === rule.field);
    if (!field)
      return "text";
    if (field.type === "boolean")
      return "boolean";
    if (field.options && field.options.length)
      return "options";
    if (field.type === "number")
      return "number";
    return "text";
  }
  onConditionFieldChange(rule) {
    const ops = this.operatorOptionsFor(rule);
    rule.operator = ops[0];
    rule.value = "";
    this.scheduleAutosave();
  }
  addCondition(branch) {
    branch.conditions.push({ field: this.triggerFields[0]?.key ?? "", operator: "Equals", value: "" });
    this.scheduleAutosave();
  }
  removeCondition(branch, idx) {
    branch.conditions.splice(idx, 1);
    this.scheduleAutosave();
  }
  edgeForBranch(nodeId, branchId) {
    return this.graph.edges.find((e) => e.source === nodeId && e.branch === branchId);
  }
  renameBranch(_branch, _name) {
    this.scheduleAutosave();
  }
  addBranch(node) {
    this.pushHistory();
    const data = this.asConditionData(node);
    const branch = { id: this.newId("branch"), name: `Branch #${data.branches.length + 1}`, conditions: [] };
    data.branches.push(branch);
    this.scheduleAutosave();
  }
  removeBranch(node, branchId) {
    this.pushHistory();
    const data = this.asConditionData(node);
    data.branches = data.branches.filter((b) => b.id !== branchId);
    this.graph.edges = this.graph.edges.filter((e) => !(e.source === node.id && e.branch === branchId));
    this.scheduleAutosave();
  }
  jumpToNode(nodeId) {
    const n = this.nodeById(nodeId);
    if (n)
      this.selectNode(n);
  }
  // ---------------- Action editing ----------------
  configFieldsFor(node) {
    const data = this.asActionData(node);
    const at = this.actionTypes.find((a) => a.key === data.actionType);
    return at?.configFields ?? [];
  }
  onActionTypeChange(node) {
    const data = this.asActionData(node);
    const at = this.actionTypes.find((a) => a.key === data.actionType);
    data.label = at?.label ?? data.actionType;
    data.config = {};
    this.scheduleAutosave();
  }
  updateConfigValue(node, key, value) {
    const data = this.asActionData(node);
    data.config = __spreadProps(__spreadValues({}, data.config), { [key]: value });
    this.scheduleAutosave();
  }
  // ---------------- Drag-and-drop node creation ----------------
  onCanvasDragOver(event) {
    event.preventDefault();
  }
  onCanvasDrop(event) {
    event.preventDefault();
    if (this.previewMode || !this.canvasWrapperRef)
      return;
    const kind = event.dataTransfer?.getData("text/kind");
    if (!kind)
      return;
    const p = this.toLogicalPoint(event.clientX, event.clientY);
    this.addNodeAt(kind, Math.max(0, p.x - NODE_WIDTH / 2), Math.max(0, p.y - 20));
  }
  addNodeAt(kind, x, y) {
    const id = this.newId("node");
    let node;
    switch (kind) {
      case "delay":
        node = { id, type: "action", x, y, data: { actionType: "Delay", label: "Delay", config: { seconds: "5" } } };
        break;
      case "notification":
        node = { id, type: "action", x, y, data: { actionType: "Notify", label: "Notification", config: {} } };
        break;
      case "decision":
        node = {
          id,
          type: "condition",
          x,
          y,
          data: {
            label: "Decision",
            branches: [
              { id: this.newId("branch"), name: "Branch #1", conditions: [] },
              { id: this.newId("branch"), name: "Branch #2", conditions: [] }
            ]
          }
        };
        break;
      case "end":
        node = { id, type: "end", x, y, data: {} };
        break;
      default:
        node = { id, type: "action", x, y, data: { actionType: "", label: "New Step", config: {} } };
    }
    this.pushHistory();
    this.graph.nodes.push(node);
    this.selectNode(node);
    this.scheduleAutosave();
  }
  deleteNode(node) {
    if (this.previewMode)
      return;
    if (node.type === "trigger") {
      this.notify.warn("The trigger node can't be deleted \u2014 every workflow needs exactly one.");
      return;
    }
    this.pushHistory();
    this.graph.nodes = this.graph.nodes.filter((n) => n.id !== node.id);
    this.graph.edges = this.graph.edges.filter((e) => e.source !== node.id && e.target !== node.id);
    if (this.selectedNode?.id === node.id)
      this.closePanel();
    this.scheduleAutosave();
  }
  // ---------------- Wiring (manual connections) ----------------
  onPortMouseDown(event, node, portId) {
    if (this.previewMode)
      return;
    event.preventDefault();
    event.stopPropagation();
    this.connectingFrom = { nodeId: node.id, portId };
    this.connectingCursor = this.toLogicalPoint(event.clientX, event.clientY);
  }
  connectPorts(sourceId, portId, targetId) {
    const source = this.nodeById(sourceId);
    if (!source)
      return;
    this.pushHistory();
    this.graph.edges = this.graph.edges.filter((e) => !(e.source === sourceId && this.portIdOfEdge(e) === portId));
    const branchLabel = source.type === "condition" ? this.asConditionData(source).branches.find((b) => b.id === portId)?.name : void 0;
    this.graph.edges.push({
      id: this.newId("edge"),
      source: sourceId,
      target: targetId,
      branch: portId === "out" ? void 0 : portId,
      label: branchLabel
    });
    this.scheduleAutosave();
  }
  selectEdge(edge, event) {
    event.stopPropagation();
    this.selectedEdgeId = edge.id;
  }
  deleteEdge(edgeId) {
    this.pushHistory();
    this.graph.edges = this.graph.edges.filter((e) => e.id !== edgeId);
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }
  // ---------------- Dragging existing nodes ----------------
  onNodeMouseDown(event, node) {
    if (this.previewMode)
      return;
    event.preventDefault();
    event.stopPropagation();
    this.pushHistory();
    this.draggingNode = node;
    const p = this.toLogicalPoint(event.clientX, event.clientY);
    this.dragOffsetX = p.x - node.x;
    this.dragOffsetY = p.y - node.y;
  }
  onDocMouseMove(event) {
    if (!this.canvasWrapperRef)
      return;
    if (this.isPanning) {
      this.viewport.x = this.panStartOffsetX + (event.clientX - this.panStartClientX);
      this.viewport.y = this.panStartOffsetY + (event.clientY - this.panStartClientY);
      return;
    }
    if (this.connectingFrom) {
      this.connectingCursor = this.toLogicalPoint(event.clientX, event.clientY);
      return;
    }
    if (this.draggingNode) {
      const p = this.toLogicalPoint(event.clientX, event.clientY);
      this.draggingNode.x = Math.max(0, p.x - this.dragOffsetX);
      this.draggingNode.y = Math.max(0, p.y - this.dragOffsetY);
    }
  }
  onDocMouseUp() {
    if (this.isPanning) {
      this.isPanning = false;
      return;
    }
    if (this.connectingFrom && this.connectingCursor) {
      const target = this.nodeAtPoint(this.connectingCursor.x, this.connectingCursor.y, this.connectingFrom.nodeId);
      if (target && this.hasInputPort(target)) {
        this.connectPorts(this.connectingFrom.nodeId, this.connectingFrom.portId, target.id);
      }
      this.connectingFrom = null;
      this.connectingCursor = null;
      return;
    }
    if (this.draggingNode) {
      this.draggingNode = null;
      this.scheduleAutosave();
    }
  }
  onCanvasClick() {
    this.selectedEdgeId = null;
  }
  // ---------------- Zoom / pan ----------------
  toLogicalPoint(clientX, clientY) {
    const rect = this.canvasWrapperRef.nativeElement.getBoundingClientRect();
    return {
      x: (clientX - rect.left - this.viewport.x) / this.viewport.scale,
      y: (clientY - rect.top - this.viewport.y) / this.viewport.scale
    };
  }
  canvasTransform() {
    return `translate(${this.viewport.x}px, ${this.viewport.y}px) scale(${this.viewport.scale})`;
  }
  onCanvasMouseDown(event) {
    if (this.connectingFrom || this.draggingNode)
      return;
    this.isPanning = true;
    this.panStartClientX = event.clientX;
    this.panStartClientY = event.clientY;
    this.panStartOffsetX = this.viewport.x;
    this.panStartOffsetY = this.viewport.y;
  }
  onCanvasWheel(event) {
    event.preventDefault();
    const rect = this.canvasWrapperRef.nativeElement.getBoundingClientRect();
    const cursor = { x: event.clientX - rect.left, y: event.clientY - rect.top };
    const factor = event.deltaY < 0 ? 1.1 : 1 / 1.1;
    this.setScale(this.viewport.scale * factor, cursor);
  }
  setScale(newScale, cursor) {
    const clamped = Math.min(2, Math.max(0.25, newScale));
    if (cursor) {
      this.viewport.x = cursor.x - (cursor.x - this.viewport.x) / this.viewport.scale * clamped;
      this.viewport.y = cursor.y - (cursor.y - this.viewport.y) / this.viewport.scale * clamped;
    }
    this.viewport.scale = clamped;
  }
  zoomIn() {
    this.setScale(this.viewport.scale * 1.2);
  }
  zoomOut() {
    this.setScale(this.viewport.scale / 1.2);
  }
  resetZoom() {
    this.viewport = { scale: 1, x: 0, y: 0 };
  }
  fitToView() {
    if (!this.canvasWrapperRef || this.graph.nodes.length === 0) {
      this.resetZoom();
      return;
    }
    const rect = this.canvasWrapperRef.nativeElement.getBoundingClientRect();
    const minX = Math.min(...this.graph.nodes.map((n) => n.x));
    const minY = Math.min(...this.graph.nodes.map((n) => n.y));
    const maxX = Math.max(...this.graph.nodes.map((n) => n.x + this.nodeWidth(n)));
    const maxY = Math.max(...this.graph.nodes.map((n) => n.y + this.nodeHeight(n)));
    const contentW = Math.max(1, maxX - minX + 80);
    const contentH = Math.max(1, maxY - minY + 80);
    const scale = Math.min(2, Math.max(0.25, Math.min(rect.width / contentW, rect.height / contentH)));
    this.viewport = {
      scale,
      x: rect.width / 2 - (minX + maxX) / 2 * scale,
      y: rect.height / 2 - (minY + maxY) / 2 * scale
    };
  }
  // ---------------- Top bar actions ----------------
  togglePreview() {
    this.previewMode = !this.previewMode;
    if (this.previewMode) {
      this.closePanel();
      this.selectedEdgeId = null;
    }
  }
  confirmPublish() {
    this.confirmation.confirm({
      message: "Publish this workflow? It will start running immediately for webhook triggers.",
      header: "Publish Workflow",
      icon: "pi pi-exclamation-triangle",
      accept: () => this.doPublish()
    });
  }
  doPublish() {
    this.api.publish(this.workflowId).subscribe({
      next: (wf) => {
        this.workflow = wf;
        this.notify.success("Workflow published successfully.");
        this.api.getDraft(this.workflowId).subscribe({
          next: (draft) => this.draftVersion = draft,
          error: () => {
          }
        });
      },
      error: (err) => this.notify.error(err.error?.message ?? "Failed to publish workflow.")
    });
  }
  backToList() {
    this.router.navigate(["/workflows"]);
  }
  // ---------------- Executions ----------------
  openExecutions() {
    this.showExecutionsDialog = true;
    this.executionsLoading = true;
    this.api.getExecutions(this.workflowId).subscribe({
      next: (data) => {
        this.executions = data;
        this.executionsLoading = false;
      },
      error: (err) => {
        this.executionsLoading = false;
        this.notify.error(err.error?.message ?? "Failed to load executions.");
      }
    });
  }
  prettyPath(json) {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }
  executionSeverity(status) {
    switch (status) {
      case "Completed":
      case "Success":
        return "success";
      case "Failed":
      case "Error":
        return "danger";
      default:
        return "info";
    }
  }
  // ---------------- Test run ----------------
  openRunDialog() {
    this.lastRunResult = null;
    this.showRunDialog = true;
  }
  runNow() {
    let context;
    try {
      context = JSON.parse(this.runContextJson || "{}");
    } catch {
      this.notify.error("Context must be valid JSON.");
      return;
    }
    this.running = true;
    this.liveSteps = [];
    this.executingNodeId = null;
    this.lastRunResult = null;
    const token = localStorage.getItem(environment.tokenKey);
    fetch(this.api.runStreamUrl(this.workflowId), {
      method: "POST",
      headers: __spreadValues({
        "Content-Type": "application/json"
      }, token ? { Authorization: `Bearer ${token}` } : {}),
      body: JSON.stringify({ context })
    }).then((response) => {
      if (!response.ok || !response.body)
        throw new Error(`Request failed (${response.status}).`);
      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";
      const pump = () => reader.read().then(({ done, value }) => {
        if (done)
          return;
        buffer += decoder.decode(value, { stream: true });
        const parts = buffer.split("\n\n");
        buffer = parts.pop() ?? "";
        for (const part of parts) {
          const line = part.trim();
          if (!line.startsWith("data:"))
            continue;
          const payload = JSON.parse(line.slice(5).trim());
          if (payload.done) {
            this.lastRunResult = payload.execution;
            this.executingNodeId = null;
          } else {
            this.liveSteps.push(payload);
            this.executingNodeId = payload.node ?? null;
          }
        }
        return pump();
      });
      return pump();
    }).then(() => {
      this.running = false;
      this.executingNodeId = null;
      if (this.lastRunResult?.status === "Completed")
        this.notify.success("Test run completed.");
      else if (this.lastRunResult)
        this.notify.error(this.lastRunResult.errorMessage ?? "Test run failed.");
    }).catch((err) => {
      this.running = false;
      this.executingNodeId = null;
      this.notify.error(err?.message ?? "Failed to run workflow.");
    });
  }
  webhookUrl() {
    return this.workflow ? this.api.webhookUrl(this.workflow.webhookToken) : "";
  }
  static \u0275fac = function WorkflowBuilderComponent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _WorkflowBuilderComponent)(\u0275\u0275directiveInject(ActivatedRoute), \u0275\u0275directiveInject(Router), \u0275\u0275directiveInject(WorkflowApiService), \u0275\u0275directiveInject(NotificationService), \u0275\u0275directiveInject(ConfirmationService));
  };
  static \u0275cmp = /* @__PURE__ */ \u0275\u0275defineComponent({ type: _WorkflowBuilderComponent, selectors: [["app-workflow-builder"]], viewQuery: function WorkflowBuilderComponent_Query(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275viewQuery(_c0, 5);
    }
    if (rf & 2) {
      let _t;
      \u0275\u0275queryRefresh(_t = \u0275\u0275loadQuery()) && (ctx.canvasWrapperRef = _t.first);
    }
  }, hostBindings: function WorkflowBuilderComponent_HostBindings(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275listener("keydown", function WorkflowBuilderComponent_keydown_HostBindingHandler($event) {
        return ctx.onDocKeyDown($event);
      }, false, \u0275\u0275resolveDocument)("mousemove", function WorkflowBuilderComponent_mousemove_HostBindingHandler($event) {
        return ctx.onDocMouseMove($event);
      }, false, \u0275\u0275resolveDocument)("mouseup", function WorkflowBuilderComponent_mouseup_HostBindingHandler() {
        return ctx.onDocMouseUp();
      }, false, \u0275\u0275resolveDocument);
    }
  }, features: [\u0275\u0275ProvidersFeature([ConfirmationService])], decls: 17, vars: 20, consts: [["loadingTpl", ""], ["canvasWrapperEl", ""], ["class", "builder-root", 4, "ngIf", "ngIfElse"], ["header", "Recent Executions", 3, "visibleChange", "visible", "modal"], ["class", "muted", 4, "ngIf"], ["class", "execution-row", 4, "ngFor", "ngForOf"], ["header", "Test Run", 3, "visibleChange", "visible", "modal"], [1, "form-field"], ["pInputTextarea", "", "rows", "6", "placeholder", "{}", 3, "ngModelChange", "ngModel"], ["class", "run-result", 4, "ngIf"], ["pTemplate", "footer"], [1, "builder-root"], [1, "topbar"], [1, "topbar-left"], [1, "breadcrumb"], [1, "crumb-link", 3, "click"], [1, "subtitle"], [4, "ngIf"], [1, "topbar-right"], ["pButton", "", "type", "button", "icon", "pi pi-undo", "title", "Undo (Ctrl+Z)", 1, "p-button-text", "p-button-sm", "icon-btn", 3, "click", "disabled"], ["pButton", "", "type", "button", "icon", "pi pi-refresh", "title", "Redo (Ctrl+Y)", 1, "p-button-text", "p-button-sm", "icon-btn", 3, "click", "disabled"], ["pButton", "", "type", "button", "label", "Test Run", "icon", "pi pi-play", 1, "p-button-outlined", "p-button-sm", 3, "click"], ["pButton", "", "type", "button", "label", "View Runs", "icon", "pi pi-history", 1, "p-button-outlined", "p-button-sm", 3, "click"], ["pButton", "", "type", "button", "icon", "pi pi-eye", 1, "p-button-outlined", 3, "click", "label"], ["pButton", "", "type", "button", "label", "Publish", 1, "publish-btn", 3, "click"], [1, "body-row"], [1, "canvas-wrapper", 3, "wheel", "mousedown"], [1, "canvas-inner", 3, "click", "dragover", "drop"], [1, "edges-svg"], ["stroke", "#9ca3af", "stroke-width", "2", "fill", "none", 4, "ngFor", "ngForOf"], ["class", "edge-hit", "stroke-width", "14", "fill", "none", 3, "edge-hit-selected", "click", 4, "ngFor", "ngForOf"], ["stroke", "#2563eb", "stroke-width", "2", "stroke-dasharray", "5,4", "fill", "none", 4, "ngIf"], [4, "ngFor", "ngForOf"], ["class", "node-card", 3, "node-end", "selected", "executing", "left", "top", "width", "border-color", "click", 4, "ngFor", "ngForOf", "ngForTrackBy"], [1, "zoom-controls"], ["pButton", "", "type", "button", "icon", "pi pi-plus", "title", "Zoom in", 1, "p-button-text", "p-button-sm", 3, "click"], ["pButton", "", "type", "button", "icon", "pi pi-minus", "title", "Zoom out", 1, "p-button-text", "p-button-sm", 3, "click"], [1, "zoom-pct"], ["pButton", "", "type", "button", "icon", "pi pi-refresh", "title", "Reset zoom", 1, "p-button-text", "p-button-sm", 3, "click"], ["pButton", "", "type", "button", "icon", "pi pi-arrows-alt", "title", "Fit to view", 1, "p-button-text", "p-button-sm", 3, "click"], ["class", "properties-panel", 4, "ngIf"], ["stroke", "#9ca3af", "stroke-width", "2", "fill", "none"], ["stroke-width", "14", "fill", "none", 1, "edge-hit", 3, "click"], ["stroke", "#2563eb", "stroke-width", "2", "stroke-dasharray", "5,4", "fill", "none"], ["class", "edge-label", 3, "ngStyle", 4, "ngIf"], ["type", "button", "class", "edge-delete-btn", 3, "ngStyle", "click", 4, "ngIf"], [1, "edge-label", 3, "ngStyle"], ["type", "button", 1, "edge-delete-btn", 3, "click", "ngStyle"], [1, "node-card", 3, "click"], [1, "node-header", 3, "mousedown"], [1, "pi", "node-icon", 3, "ngClass"], [1, "node-title"], ["class", "node-badge", 4, "ngIf"], ["class", "node-body", 4, "ngIf"], [1, "node-badge"], [1, "node-body"], [3, "ngSwitch"], [4, "ngSwitchCase"], ["class", "port port-input", 3, "left", "top", 4, "ngIf"], ["class", "port port-output", 3, "left", "top", "title", "mousedown", 4, "ngFor", "ngForOf"], [1, "port", "port-input"], [1, "port", "port-output", 3, "mousedown", "title"], [1, "properties-panel"], [1, "panel-header"], [1, "panel-header-actions"], ["type", "button", "title", "Delete node (Del)", 1, "close-btn", 3, "click", "disabled"], [1, "pi", "pi-trash"], ["type", "button", 1, "close-btn", 3, "click"], [1, "pi", "pi-times"], [1, "panel-type-row"], [1, "type-badge"], ["pButton", "", "type", "button", "label", "Change", 1, "p-button-text", "p-button-sm", 3, "disabled"], ["pInputText", "", "type", "text", "placeholder", "Add description here", 3, "ngModelChange", "ngModel", "disabled"], ["pInputText", "", "type", "text", "disabled", "", 3, "value"], ["class", "form-field", 4, "ngIf"], ["pInputText", "", "type", "text", "readonly", "", 3, "value"], [1, "muted"], [1, "section-title"], ["class", "group-card", 4, "ngFor", "ngForOf"], ["pButton", "", "type", "button", "label", "Add Branch", "icon", "pi pi-plus", 1, "p-button-outlined", "p-button-sm", 3, "click", "disabled"], [1, "section-title", 2, "margin-top", "1.25rem"], ["class", "branch-card", 4, "ngFor", "ngForOf"], [1, "group-card"], [1, "group-header", "branch-name-row"], ["pInputText", "", "type", "text", 1, "branch-name-input", 3, "ngModelChange", "ngModel", "disabled"], ["type", "button", 1, "row-remove", 3, "click", "disabled"], ["class", "condition-row", 4, "ngFor", "ngForOf"], ["pButton", "", "type", "button", "label", "Add Condition", 1, "p-button-text", "p-button-sm", 3, "click", "disabled"], [1, "condition-row"], ["optionLabel", "label", "optionValue", "key", "placeholder", "Field", 3, "ngModelChange", "onChange", "options", "ngModel", "disabled"], ["placeholder", "Operator", 3, "ngModelChange", "options", "ngModel", "disabled"], [3, "options", "ngModel", "disabled", "ngModelChange", 4, "ngSwitchCase"], ["pInputText", "", "type", "number", 3, "ngModel", "disabled", "ngModelChange", 4, "ngSwitchCase"], ["pInputText", "", "type", "text", 3, "ngModel", "disabled", "ngModelChange", 4, "ngSwitchDefault"], [3, "ngModelChange", "options", "ngModel", "disabled"], ["pInputText", "", "type", "number", 3, "ngModelChange", "ngModel", "disabled"], ["pInputText", "", "type", "text", 3, "ngModelChange", "ngModel", "disabled"], [1, "branch-card"], [1, "branch-pill", "pill-other"], [1, "branch-target"], ["pButton", "", "type", "button", "icon", "pi pi-arrow-right", 1, "p-button-text", "p-button-sm", 3, "click"], ["optionLabel", "label", "optionValue", "key", "placeholder", "Select action", 3, "ngModelChange", "onChange", "options", "ngModel", "disabled"], ["class", "form-field", 4, "ngFor", "ngForOf"], [3, "options", "ngModel", "disabled", "ngModelChange", 4, "ngIf"], ["pInputText", "", 3, "type", "ngModel", "disabled", "ngModelChange", 4, "ngIf"], ["pInputText", "", 3, "ngModelChange", "type", "ngModel", "disabled"], [1, "loading-state"], [1, "execution-row"], [1, "execution-head"], [3, "value", "severity"], ["class", "error-text", 4, "ngIf"], [1, "error-text"], [1, "run-result"], ["class", "live-step", 4, "ngFor", "ngForOf"], [1, "live-step"], [1, "step-type"], [1, "step-node"], ["pButton", "", "type", "button", "label", "Close", 1, "p-button-text", 3, "click"], ["pButton", "", "type", "button", "label", "Run", "icon", "pi pi-play", 3, "click", "loading"]], template: function WorkflowBuilderComponent_Template(rf, ctx) {
    if (rf & 1) {
      const _r1 = \u0275\u0275getCurrentView();
      \u0275\u0275element(0, "p-toast")(1, "p-confirmDialog");
      \u0275\u0275template(2, WorkflowBuilderComponent_div_2_Template, 41, 28, "div", 2)(3, WorkflowBuilderComponent_ng_template_3_Template, 2, 0, "ng-template", null, 0, \u0275\u0275templateRefExtractor);
      \u0275\u0275elementStart(5, "p-dialog", 3);
      \u0275\u0275twoWayListener("visibleChange", function WorkflowBuilderComponent_Template_p_dialog_visibleChange_5_listener($event) {
        \u0275\u0275restoreView(_r1);
        \u0275\u0275twoWayBindingSet(ctx.showExecutionsDialog, $event) || (ctx.showExecutionsDialog = $event);
        return \u0275\u0275resetView($event);
      });
      \u0275\u0275template(6, WorkflowBuilderComponent_div_6_Template, 2, 0, "div", 4)(7, WorkflowBuilderComponent_div_7_Template, 2, 0, "div", 4)(8, WorkflowBuilderComponent_div_8_Template, 11, 9, "div", 5);
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(9, "p-dialog", 6);
      \u0275\u0275twoWayListener("visibleChange", function WorkflowBuilderComponent_Template_p_dialog_visibleChange_9_listener($event) {
        \u0275\u0275restoreView(_r1);
        \u0275\u0275twoWayBindingSet(ctx.showRunDialog, $event) || (ctx.showRunDialog = $event);
        return \u0275\u0275resetView($event);
      });
      \u0275\u0275elementStart(10, "div", 7)(11, "label");
      \u0275\u0275text(12);
      \u0275\u0275elementEnd();
      \u0275\u0275elementStart(13, "textarea", 8);
      \u0275\u0275twoWayListener("ngModelChange", function WorkflowBuilderComponent_Template_textarea_ngModelChange_13_listener($event) {
        \u0275\u0275restoreView(_r1);
        \u0275\u0275twoWayBindingSet(ctx.runContextJson, $event) || (ctx.runContextJson = $event);
        return \u0275\u0275resetView($event);
      });
      \u0275\u0275elementEnd()();
      \u0275\u0275template(14, WorkflowBuilderComponent_div_14_Template, 3, 2, "div", 9)(15, WorkflowBuilderComponent_div_15_Template, 5, 4, "div", 9)(16, WorkflowBuilderComponent_ng_template_16_Template, 2, 1, "ng-template", 10);
      \u0275\u0275elementEnd();
    }
    if (rf & 2) {
      const loadingTpl_r36 = \u0275\u0275reference(4);
      \u0275\u0275advance(2);
      \u0275\u0275property("ngIf", !ctx.loading)("ngIfElse", loadingTpl_r36);
      \u0275\u0275advance(3);
      \u0275\u0275styleMap(\u0275\u0275pureFunction0(18, _c1));
      \u0275\u0275twoWayProperty("visible", ctx.showExecutionsDialog);
      \u0275\u0275property("modal", true);
      \u0275\u0275advance();
      \u0275\u0275property("ngIf", ctx.executionsLoading);
      \u0275\u0275advance();
      \u0275\u0275property("ngIf", !ctx.executionsLoading && ctx.executions.length === 0);
      \u0275\u0275advance();
      \u0275\u0275property("ngForOf", ctx.executions);
      \u0275\u0275advance();
      \u0275\u0275styleMap(\u0275\u0275pureFunction0(19, _c2));
      \u0275\u0275twoWayProperty("visible", ctx.showRunDialog);
      \u0275\u0275property("modal", true);
      \u0275\u0275advance(3);
      \u0275\u0275textInterpolate2("Context JSON (available to condition rules and ", "{", "Field", "}", " placeholders)");
      \u0275\u0275advance();
      \u0275\u0275twoWayProperty("ngModel", ctx.runContextJson);
      \u0275\u0275advance();
      \u0275\u0275property("ngIf", ctx.running || ctx.liveSteps.length);
      \u0275\u0275advance();
      \u0275\u0275property("ngIf", !ctx.running && ctx.lastRunResult);
    }
  }, dependencies: [
    CommonModule,
    NgClass,
    NgForOf,
    NgIf,
    NgStyle,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    DecimalPipe,
    DatePipe,
    FormsModule,
    DefaultValueAccessor,
    NumberValueAccessor,
    NgControlStatus,
    NgModel,
    ButtonModule,
    ButtonDirective,
    PrimeTemplate,
    DialogModule,
    Dialog,
    DropdownModule,
    Dropdown,
    InputTextModule,
    InputText,
    InputTextarea,
    TagModule,
    Tag,
    TableModule,
    ToastModule,
    Toast,
    ConfirmDialogModule,
    ConfirmDialog,
    NodePaletteComponent
  ], styles: ["\n\n[_nghost-%COMP%] {\n  display: block;\n  height: 100%;\n}\n.loading-state[_ngcontent-%COMP%] {\n  padding: 3rem;\n  text-align: center;\n  color: #6b7280;\n}\n.builder-root[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: calc(100vh - 56px);\n}\n.topbar[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  background: #fff;\n  border-bottom: 1px solid #e5e7eb;\n  padding: 0.75rem 1.25rem;\n  flex-shrink: 0;\n}\n.topbar-left[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n}\n.topbar-left[_ngcontent-%COMP%]   .breadcrumb[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  color: #9ca3af;\n}\n.topbar-left[_ngcontent-%COMP%]   .breadcrumb[_ngcontent-%COMP%]   .crumb-link[_ngcontent-%COMP%] {\n  cursor: pointer;\n}\n.topbar-left[_ngcontent-%COMP%]   .breadcrumb[_ngcontent-%COMP%]   .crumb-link[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n.topbar-left[_ngcontent-%COMP%]   h1[_ngcontent-%COMP%] {\n  font-size: 1.25rem;\n  font-weight: 700;\n  margin: 0.15rem 0;\n}\n.topbar-left[_ngcontent-%COMP%]   .subtitle[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  color: #6b7280;\n}\n.topbar-right[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n}\n.topbar-right[_ngcontent-%COMP%]   .icon-btn[_ngcontent-%COMP%] {\n  color: #9ca3af;\n  opacity: 0.6;\n}\n.topbar-right[_ngcontent-%COMP%]   .publish-btn[_ngcontent-%COMP%] {\n  background: #f97316;\n  border-color: #f97316;\n  color: #fff;\n}\n.topbar-right[_ngcontent-%COMP%]   .publish-btn[_ngcontent-%COMP%]:hover {\n  background: #ea580c;\n  border-color: #ea580c;\n}\n.body-row[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  min-height: 0;\n}\n.canvas-wrapper[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow: hidden;\n  position: relative;\n  background-color: #f9fafb;\n  cursor: grab;\n}\n.canvas-wrapper[_ngcontent-%COMP%]:active {\n  cursor: grabbing;\n}\n.canvas-inner[_ngcontent-%COMP%] {\n  position: relative;\n  min-width: 1200px;\n  min-height: 900px;\n  transform-origin: 0 0;\n  background-image:\n    radial-gradient(\n      circle,\n      #d1d5db 1px,\n      transparent 1px);\n  background-size: 20px 20px;\n}\n.zoom-controls[_ngcontent-%COMP%] {\n  position: absolute;\n  right: 1rem;\n  bottom: 1rem;\n  z-index: 10;\n  display: flex;\n  align-items: center;\n  gap: 0.15rem;\n  background: #fff;\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.25rem 0.4rem;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);\n}\n.zoom-controls[_ngcontent-%COMP%]   .zoom-pct[_ngcontent-%COMP%] {\n  font-size: 0.7rem;\n  color: #6b7280;\n  width: 2.75rem;\n  text-align: center;\n}\n.edges-svg[_ngcontent-%COMP%] {\n  position: absolute;\n  top: 0;\n  left: 0;\n  z-index: 0;\n  pointer-events: none;\n}\n.edge-label[_ngcontent-%COMP%] {\n  position: absolute;\n  transform: translate(-50%, -50%);\n  z-index: 1;\n  font-size: 0.7rem;\n  font-weight: 600;\n  padding: 2px 8px;\n  border-radius: 10px;\n  white-space: nowrap;\n}\n.edge-label-true[_ngcontent-%COMP%] {\n  background: #dcfce7;\n  color: #166534;\n}\n.edge-label-false[_ngcontent-%COMP%] {\n  background: #ede9fe;\n  color: #6d28d9;\n}\n.node-card[_ngcontent-%COMP%] {\n  position: absolute;\n  z-index: 2;\n  background: #fff;\n  border: 2px solid #e5e7eb;\n  border-radius: 8px;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);\n  overflow: hidden;\n}\n.node-card.selected[_ngcontent-%COMP%] {\n  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.25);\n}\n.node-card.executing[_ngcontent-%COMP%] {\n  animation: _ngcontent-%COMP%_wb-pulse 1s ease-in-out infinite;\n}\n.node-card.node-end[_ngcontent-%COMP%] {\n  border-radius: 24px;\n  text-align: center;\n}\n.node-card.node-end[_ngcontent-%COMP%]   .node-header[_ngcontent-%COMP%] {\n  justify-content: center;\n  border-radius: 24px;\n}\n.node-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.4rem;\n  padding: 0.5rem 0.65rem;\n  cursor: grab;\n  font-size: 0.8rem;\n  font-weight: 600;\n  color: #1f2937;\n}\n.node-header[_ngcontent-%COMP%]:active {\n  cursor: grabbing;\n}\n.node-icon[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n}\n@keyframes _ngcontent-%COMP%_wb-pulse {\n  0%, 100% {\n    box-shadow: 0 0 0 3px rgba(249, 115, 22, 0.6);\n  }\n  50% {\n    box-shadow: 0 0 0 6px rgba(249, 115, 22, 0.25);\n  }\n}\n.node-title[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.node-badge[_ngcontent-%COMP%] {\n  font-size: 0.6rem;\n  font-weight: 600;\n  background: rgba(0, 0, 0, 0.06);\n  color: #4b5563;\n  padding: 2px 6px;\n  border-radius: 8px;\n}\n.node-body[_ngcontent-%COMP%] {\n  padding: 0.5rem 0.65rem;\n  font-size: 0.75rem;\n  color: #4b5563;\n  border-top: 1px solid #f1f5f9;\n}\n.port[_ngcontent-%COMP%] {\n  position: absolute;\n  z-index: 5;\n  width: 12px;\n  height: 12px;\n  border-radius: 50%;\n  background: #fff;\n  border: 2px solid #6b7280;\n}\n.port-output[_ngcontent-%COMP%] {\n  cursor: crosshair;\n}\n.port-output[_ngcontent-%COMP%]:hover {\n  border-color: #2563eb;\n  background: #dbeafe;\n  transform: scale(1.2);\n}\n.port-input[_ngcontent-%COMP%] {\n  pointer-events: none;\n  border-color: #9ca3af;\n}\n.edge-hit[_ngcontent-%COMP%] {\n  cursor: pointer;\n  stroke: transparent;\n  pointer-events: stroke;\n}\n.edge-hit.edge-hit-selected[_ngcontent-%COMP%] {\n  stroke: rgba(37, 99, 235, 0.15);\n}\n.edge-delete-btn[_ngcontent-%COMP%] {\n  position: absolute;\n  z-index: 6;\n  transform: translate(calc(-50% + 22px), -50%);\n  width: 20px;\n  height: 20px;\n  border-radius: 50%;\n  border: none;\n  background: #dc2626;\n  color: #fff;\n  font-size: 0.65rem;\n  line-height: 1;\n  cursor: pointer;\n}\n.edge-delete-btn[_ngcontent-%COMP%]:hover {\n  background: #b91c1c;\n}\n.properties-panel[_ngcontent-%COMP%] {\n  width: 360px;\n  flex-shrink: 0;\n  background: #fff;\n  border-left: 1px solid #e5e7eb;\n  overflow-y: auto;\n  padding: 1rem 1.25rem;\n}\n.panel-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  font-weight: 700;\n  font-size: 0.95rem;\n  margin-bottom: 1rem;\n}\n.panel-header[_ngcontent-%COMP%]   .panel-header-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 0.6rem;\n}\n.panel-header[_ngcontent-%COMP%]   .close-btn[_ngcontent-%COMP%] {\n  border: none;\n  background: none;\n  cursor: pointer;\n  color: #6b7280;\n}\n.panel-header[_ngcontent-%COMP%]   .close-btn[_ngcontent-%COMP%]:hover {\n  color: #111827;\n}\n.panel-type-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 1rem;\n}\n.panel-type-row[_ngcontent-%COMP%]   .type-badge[_ngcontent-%COMP%] {\n  border: 2px solid #e5e7eb;\n  border-radius: 6px;\n  padding: 0.25rem 0.6rem;\n  font-size: 0.75rem;\n  font-weight: 600;\n}\n.form-field[_ngcontent-%COMP%] {\n  margin-bottom: 1rem;\n  display: flex;\n  flex-direction: column;\n  gap: 0.375rem;\n}\n.form-field[_ngcontent-%COMP%]   label[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  font-weight: 600;\n  color: #374151;\n}\n.form-field[_ngcontent-%COMP%]   input[_ngcontent-%COMP%], \n.form-field[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%] {\n  width: 100%;\n}\n.section-title[_ngcontent-%COMP%] {\n  font-size: 0.8rem;\n  font-weight: 700;\n  color: #111827;\n  margin-bottom: 0.5rem;\n}\n.group-card[_ngcontent-%COMP%] {\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.75rem;\n  margin-bottom: 0.75rem;\n}\n.group-header[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  font-weight: 700;\n  color: #6d28d9;\n  margin-bottom: 0.5rem;\n}\n.branch-name-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n}\n.branch-name-row[_ngcontent-%COMP%]   .branch-name-input[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.condition-row[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 1fr 1fr auto;\n  gap: 0.35rem;\n  align-items: center;\n  margin-bottom: 0.5rem;\n}\n.row-remove[_ngcontent-%COMP%] {\n  border: none;\n  background: none;\n  color: #9ca3af;\n  cursor: pointer;\n}\n.row-remove[_ngcontent-%COMP%]:hover {\n  color: #dc2626;\n}\n.row-remove[_ngcontent-%COMP%]:disabled {\n  opacity: 0.35;\n  cursor: not-allowed;\n}\n.branch-card[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n  padding: 0.5rem 0;\n  border-bottom: 1px solid #f1f5f9;\n  font-size: 0.8rem;\n}\n.branch-pill[_ngcontent-%COMP%] {\n  font-size: 0.7rem;\n  font-weight: 700;\n  padding: 2px 8px;\n  border-radius: 10px;\n}\n.pill-true[_ngcontent-%COMP%] {\n  background: #dcfce7;\n  color: #166534;\n}\n.pill-false[_ngcontent-%COMP%] {\n  background: #ede9fe;\n  color: #6d28d9;\n}\n.pill-other[_ngcontent-%COMP%] {\n  background: #e0f2fe;\n  color: #0369a1;\n}\n.branch-target[_ngcontent-%COMP%] {\n  flex: 1;\n  color: #374151;\n}\n.muted[_ngcontent-%COMP%] {\n  color: #9ca3af;\n  font-size: 0.8rem;\n}\n.execution-row[_ngcontent-%COMP%] {\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.75rem;\n  margin-bottom: 0.75rem;\n}\n.execution-head[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.6rem;\n  margin-bottom: 0.5rem;\n}\n.error-text[_ngcontent-%COMP%] {\n  color: #dc2626;\n  font-size: 0.8rem;\n  margin-bottom: 0.5rem;\n}\n.execution-row[_ngcontent-%COMP%]   pre[_ngcontent-%COMP%], \n.run-result[_ngcontent-%COMP%]   pre[_ngcontent-%COMP%] {\n  background: #f9fafb;\n  border-radius: 6px;\n  padding: 0.5rem;\n  font-size: 0.7rem;\n  max-height: 200px;\n  overflow: auto;\n  margin: 0;\n}\n.run-result[_ngcontent-%COMP%] {\n  margin-top: 0.5rem;\n}\n.live-step[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n  font-size: 0.75rem;\n  padding: 0.35rem 0;\n  border-bottom: 1px solid #f1f5f9;\n}\n.live-step[_ngcontent-%COMP%]   .step-type[_ngcontent-%COMP%] {\n  text-transform: uppercase;\n  font-size: 0.6rem;\n  font-weight: 700;\n  color: #9ca3af;\n}\n.live-step[_ngcontent-%COMP%]   .step-node[_ngcontent-%COMP%] {\n  font-weight: 600;\n  color: #374151;\n}\n/*# sourceMappingURL=workflow-builder.component.css.map */"] });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(WorkflowBuilderComponent, [{
    type: Component,
    args: [{ selector: "app-workflow-builder", standalone: true, imports: [
      CommonModule,
      FormsModule,
      ButtonModule,
      DialogModule,
      DropdownModule,
      InputTextModule,
      InputTextarea,
      TagModule,
      TableModule,
      ToastModule,
      ConfirmDialogModule,
      NodePaletteComponent
    ], providers: [ConfirmationService], template: `<p-toast></p-toast>
<p-confirmDialog></p-confirmDialog>

<div class="builder-root" *ngIf="!loading; else loadingTpl">
  <!-- Top bar -->
  <div class="topbar">
    <div class="topbar-left">
      <div class="breadcrumb">
        <span class="crumb-link" (click)="backToList()">Workflows</span>
      </div>
      <h1>{{ workflow?.name }}</h1>
      <div class="subtitle">
        <span *ngIf="savedText">{{ savedText }}</span>
        <span *ngIf="savedText"> &middot; </span>
        <span *ngIf="draftVersion">Version {{ draftVersion.versionNumber }} [Draft]</span>
        <span *ngIf="workflow?.publishedVersionNumber"> &middot; Published: v{{ workflow?.publishedVersionNumber }}</span>
      </div>
    </div>
    <div class="topbar-right">
      <button pButton type="button" icon="pi pi-undo" class="p-button-text p-button-sm icon-btn" [disabled]="!canUndo()" title="Undo (Ctrl+Z)" (click)="undo()"></button>
      <button pButton type="button" icon="pi pi-refresh" class="p-button-text p-button-sm icon-btn" [disabled]="!canRedo()" title="Redo (Ctrl+Y)" (click)="redo()"></button>
      <button pButton type="button" label="Test Run" icon="pi pi-play" class="p-button-outlined p-button-sm" (click)="openRunDialog()"></button>
      <button pButton type="button" label="View Runs" icon="pi pi-history" class="p-button-outlined p-button-sm" (click)="openExecutions()"></button>
      <button
        pButton
        type="button"
        [label]="previewMode ? 'Exit Preview' : 'Preview'"
        icon="pi pi-eye"
        class="p-button-outlined"
        (click)="togglePreview()"
      ></button>
      <button pButton type="button" label="Publish" class="publish-btn" (click)="confirmPublish()"></button>
    </div>
  </div>

  <!-- Body: palette + canvas + properties panel -->
  <div class="body-row">
    <app-node-palette></app-node-palette>
    <div class="canvas-wrapper" #canvasWrapperEl (wheel)="onCanvasWheel($event)" (mousedown)="onCanvasMouseDown($event)">
      <div
        class="canvas-inner"
        [style.width.px]="canvasWidth()"
        [style.height.px]="canvasHeight()"
        [style.transform]="canvasTransform()"
        (click)="onCanvasClick()"
        (dragover)="onCanvasDragOver($event)"
        (drop)="onCanvasDrop($event)"
      >
        <svg class="edges-svg" [attr.width]="canvasWidth()" [attr.height]="canvasHeight()">
          <path *ngFor="let edge of graph.edges" [attr.d]="edgePath(edge)" stroke="#9ca3af" stroke-width="2" fill="none" />
          <path
            *ngFor="let edge of graph.edges"
            [attr.d]="edgePath(edge)"
            class="edge-hit"
            [class.edge-hit-selected]="selectedEdgeId === edge.id"
            stroke-width="14"
            fill="none"
            (click)="selectEdge(edge, $event)"
          />
          <path *ngIf="connectingFrom" [attr.d]="connectingLinePath()" stroke="#2563eb" stroke-width="2" stroke-dasharray="5,4" fill="none" />
        </svg>

        <ng-container *ngFor="let edge of graph.edges">
          <div *ngIf="edge.label" class="edge-label" [ngStyle]="edgeLabelStyle(edge)">{{ edge.label }}</div>
          <button
            *ngIf="selectedEdgeId === edge.id"
            type="button"
            class="edge-delete-btn"
            [ngStyle]="edgeLabelStyle(edge)"
            (click)="deleteEdge(edge.id); $event.stopPropagation()"
          >\u2715</button>
        </ng-container>

        <div
          *ngFor="let node of graph.nodes; trackBy: trackById"
          class="node-card"
          [class.node-end]="node.type === 'end'"
          [class.selected]="selectedNode?.id === node.id"
          [class.executing]="executingNodeId === node.id"
          [style.left.px]="node.x"
          [style.top.px]="node.y"
          [style.width.px]="nodeWidth(node)"
          [style.border-color]="nodeBorderColor(node)"
          (click)="selectNode(node); $event.stopPropagation()"
        >
          <div class="node-header" [style.background]="nodeHeaderBg(node)" (mousedown)="onNodeMouseDown($event, node)">
            <i
              class="pi node-icon"
              [ngClass]="{
                'pi-file': node.type === 'trigger',
                'pi-directions': node.type === 'condition',
                'pi-bolt': node.type === 'action',
                'pi-flag': node.type === 'end'
              }"
            ></i>
            <span class="node-title">{{ nodeTypeLabel(node) }}</span>
            <span class="node-badge" *ngIf="node.type !== 'end'">{{
              node.type === 'trigger' ? 'Trigger' : node.type === 'condition' ? 'Condition' : 'Action'
            }}</span>
          </div>
          <div class="node-body" *ngIf="node.type !== 'end'">
            <ng-container [ngSwitch]="node.type">
              <span *ngSwitchCase="'trigger'">{{ triggerBodyText(node) }}</span>
              <span *ngSwitchCase="'condition'">{{ conditionSummary(node) }}</span>
              <span *ngSwitchCase="'action'">{{ actionBodyText(node) }}</span>
            </ng-container>
          </div>
        </div>

        <!-- Connection ports (drag from an output port to another node's input to wire them) -->
        <ng-container *ngFor="let node of graph.nodes">
          <div
            *ngIf="hasInputPort(node)"
            class="port port-input"
            [style.left.px]="inputPortPos(node).x - 6"
            [style.top.px]="inputPortPos(node).y - 6"
          ></div>
          <div
            *ngFor="let port of outputPorts(node)"
            class="port port-output"
            [style.left.px]="outputPortPos(node, port.id).x - 6"
            [style.top.px]="outputPortPos(node, port.id).y - 6"
            (mousedown)="onPortMouseDown($event, node, port.id)"
            [title]="port.label || 'Drag to connect'"
          ></div>
        </ng-container>
      </div>

      <div class="zoom-controls">
        <button pButton type="button" icon="pi pi-plus" class="p-button-text p-button-sm" (click)="zoomIn()" title="Zoom in"></button>
        <button pButton type="button" icon="pi pi-minus" class="p-button-text p-button-sm" (click)="zoomOut()" title="Zoom out"></button>
        <span class="zoom-pct">{{ viewport.scale * 100 | number: '1.0-0' }}%</span>
        <button pButton type="button" icon="pi pi-refresh" class="p-button-text p-button-sm" (click)="resetZoom()" title="Reset zoom"></button>
        <button pButton type="button" icon="pi pi-arrows-alt" class="p-button-text p-button-sm" (click)="fitToView()" title="Fit to view"></button>
      </div>
    </div>

    <div class="properties-panel" *ngIf="showPropertiesPanel && selectedNode as sn">
      <div class="panel-header">
        <span>Properties</span>
        <div class="panel-header-actions">
          <button
            type="button"
            class="close-btn"
            (click)="deleteNode(sn)"
            [disabled]="previewMode || sn.type === 'trigger'"
            title="Delete node (Del)"
          ><i class="pi pi-trash"></i></button>
          <button type="button" class="close-btn" (click)="closePanel()"><i class="pi pi-times"></i></button>
        </div>
      </div>

      <div class="panel-type-row">
        <span class="type-badge" [style.background]="nodeHeaderBg(sn)" [style.border-color]="nodeBorderColor(sn)">{{
          nodeTypeLabel(sn)
        }}</span>
        <button pButton type="button" label="Change" class="p-button-text p-button-sm" [disabled]="true"></button>
      </div>

      <div class="form-field">
        <label>Description</label>
        <input
          pInputText
          type="text"
          [ngModel]="descriptionOf(sn)"
          (ngModelChange)="onDescriptionChange(sn, $event)"
          placeholder="Add description here"
          [disabled]="previewMode"
        />
      </div>

      <ng-container [ngSwitch]="sn.type">
        <!-- Trigger -->
        <div *ngSwitchCase="'trigger'">
          <div class="form-field">
            <label>Trigger</label>
            <input pInputText type="text" [value]="asTriggerData(sn).label" disabled />
          </div>
          <div class="form-field" *ngIf="workflow?.triggerType === 'Webhook'">
            <label>Webhook URL</label>
            <input pInputText type="text" [value]="webhookUrl()" readonly />
            <span class="muted">POST JSON here to trigger the published version.</span>
          </div>
        </div>

        <!-- Decision (condition) -->
        <div *ngSwitchCase="'condition'">
          <div class="section-title">Branches</div>
          <div class="group-card" *ngFor="let branch of asConditionData(sn).branches; let bi = index">
            <div class="group-header branch-name-row">
              <input
                pInputText
                type="text"
                class="branch-name-input"
                [(ngModel)]="branch.name"
                (ngModelChange)="renameBranch(branch, $event)"
                [disabled]="previewMode"
              />
              <button
                type="button"
                class="row-remove"
                (click)="removeBranch(sn, branch.id)"
                [disabled]="previewMode || asConditionData(sn).branches.length <= 1"
              >\u2715</button>
            </div>
            <div class="condition-row" *ngFor="let rule of branch.conditions; let ri = index">
              <p-dropdown
                [options]="triggerFields"
                optionLabel="label"
                optionValue="key"
                [(ngModel)]="rule.field"
                (onChange)="onConditionFieldChange(rule)"
                placeholder="Field"
                [disabled]="previewMode"
              ></p-dropdown>
              <p-dropdown
                [options]="operatorOptionsFor(rule)"
                [(ngModel)]="rule.operator"
                (ngModelChange)="scheduleAutosave()"
                placeholder="Operator"
                [disabled]="previewMode"
              ></p-dropdown>
              <ng-container [ngSwitch]="valueKind(rule)">
                <p-dropdown
                  *ngSwitchCase="'boolean'"
                  [options]="['true', 'false']"
                  [(ngModel)]="rule.value"
                  (ngModelChange)="scheduleAutosave()"
                  [disabled]="previewMode"
                ></p-dropdown>
                <p-dropdown
                  *ngSwitchCase="'options'"
                  [options]="triggerFieldOptions(rule)"
                  [(ngModel)]="rule.value"
                  (ngModelChange)="scheduleAutosave()"
                  [disabled]="previewMode"
                ></p-dropdown>
                <input
                  *ngSwitchCase="'number'"
                  pInputText
                  type="number"
                  [(ngModel)]="rule.value"
                  (ngModelChange)="scheduleAutosave()"
                  [disabled]="previewMode"
                />
                <input
                  *ngSwitchDefault
                  pInputText
                  type="text"
                  [(ngModel)]="rule.value"
                  (ngModelChange)="scheduleAutosave()"
                  [disabled]="previewMode"
                />
              </ng-container>
              <button type="button" class="row-remove" (click)="removeCondition(branch, ri)" [disabled]="previewMode">\u2715</button>
            </div>
            <button
              pButton
              type="button"
              label="Add Condition"
              class="p-button-text p-button-sm"
              (click)="addCondition(branch)"
              [disabled]="previewMode"
            ></button>
          </div>
          <button
            pButton
            type="button"
            label="Add Branch"
            icon="pi pi-plus"
            class="p-button-outlined p-button-sm"
            (click)="addBranch(sn)"
            [disabled]="previewMode"
          ></button>

          <div class="section-title" style="margin-top: 1.25rem">Next Step</div>
          <div class="branch-card" *ngFor="let branch of asConditionData(sn).branches; let bi = index">
            <ng-container *ngIf="edgeForBranch(sn.id, branch.id) as be">
              <span class="branch-pill pill-other">{{ branch.name }}</span>
              <span class="branch-target">{{ be.target && nodeById(be.target) ? nodeTypeLabel(nodeById(be.target)!) : '' }}</span>
              <button pButton type="button" icon="pi pi-arrow-right" class="p-button-text p-button-sm" (click)="jumpToNode(be.target)"></button>
            </ng-container>
          </div>
        </div>

        <!-- Action -->
        <div *ngSwitchCase="'action'">
          <div class="form-field">
            <label>Action Type</label>
            <p-dropdown
              [options]="actionTypes"
              optionLabel="label"
              optionValue="key"
              [(ngModel)]="asActionData(sn).actionType"
              (onChange)="onActionTypeChange(sn)"
              placeholder="Select action"
              [disabled]="previewMode"
            ></p-dropdown>
          </div>
          <div class="form-field" *ngFor="let f of configFieldsFor(sn)">
            <label>{{ f.label }}</label>
            <p-dropdown
              *ngIf="f.options && f.options.length"
              [options]="f.options"
              [ngModel]="asActionData(sn).config[f.key]"
              (ngModelChange)="updateConfigValue(sn, f.key, $event)"
              [disabled]="previewMode"
            ></p-dropdown>
            <input
              *ngIf="!f.options || !f.options.length"
              pInputText
              [type]="f.type === 'number' ? 'number' : 'text'"
              [ngModel]="asActionData(sn).config[f.key]"
              (ngModelChange)="updateConfigValue(sn, f.key, $event)"
              [disabled]="previewMode"
            />
          </div>
        </div>

        <!-- End -->
        <div *ngSwitchCase="'end'">
          <p class="muted">This path ends here. No further configuration is needed.</p>
        </div>
      </ng-container>
    </div>
  </div>
</div>

<ng-template #loadingTpl>
  <div class="loading-state">Loading workflow\u2026</div>
</ng-template>

<p-dialog header="Recent Executions" [(visible)]="showExecutionsDialog" [modal]="true" [style]="{ width: '700px' }">
  <div *ngIf="executionsLoading" class="muted">Loading\u2026</div>
  <div *ngIf="!executionsLoading && executions.length === 0" class="muted">No executions yet.</div>
  <div class="execution-row" *ngFor="let ex of executions">
    <div class="execution-head">
      <p-tag [value]="ex.status" [severity]="executionSeverity(ex.status)"></p-tag>
      <span>{{ ex.triggerEntityType }}</span>
      <span class="muted">{{ ex.createdDate | date: 'medium' }}</span>
    </div>
    <div *ngIf="ex.errorMessage" class="error-text">{{ ex.errorMessage }}</div>
    <pre>{{ prettyPath(ex.pathJson) }}</pre>
  </div>
</p-dialog>

<p-dialog header="Test Run" [(visible)]="showRunDialog" [modal]="true" [style]="{ width: '600px' }">
  <div class="form-field">
    <label>Context JSON (available to condition rules and {{'{'}}Field{{'}'}} placeholders)</label>
    <textarea pInputTextarea rows="6" [(ngModel)]="runContextJson" placeholder="{}"></textarea>
  </div>
  <div class="run-result" *ngIf="running || liveSteps.length">
    <div class="muted" *ngIf="running">Running live\u2026</div>
    <div class="live-step" *ngFor="let step of liveSteps">
      <span class="step-type">{{ step.type }}</span>
      <span class="step-node">{{ step.node }}</span>
      <span *ngIf="step.matchedBranchName"> \u2192 {{ step.matchedBranchName }}</span>
      <span *ngIf="step.result"> \u2014 {{ step.result }}</span>
    </div>
  </div>
  <div class="run-result" *ngIf="!running && lastRunResult">
    <p-tag [value]="lastRunResult.status" [severity]="executionSeverity(lastRunResult.status)"></p-tag>
    <div *ngIf="lastRunResult.errorMessage" class="error-text">{{ lastRunResult.errorMessage }}</div>
    <pre>{{ prettyPath(lastRunResult.pathJson) }}</pre>
  </div>
  <ng-template pTemplate="footer">
    <button pButton type="button" label="Close" class="p-button-text" (click)="showRunDialog = false"></button>
    <button pButton type="button" label="Run" icon="pi pi-play" [loading]="running" (click)="runNow()"></button>
  </ng-template>
</p-dialog>
`, styles: ["/* src/app/features/workflows/workflow-builder.component.scss */\n:host {\n  display: block;\n  height: 100%;\n}\n.loading-state {\n  padding: 3rem;\n  text-align: center;\n  color: #6b7280;\n}\n.builder-root {\n  display: flex;\n  flex-direction: column;\n  height: calc(100vh - 56px);\n}\n.topbar {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  background: #fff;\n  border-bottom: 1px solid #e5e7eb;\n  padding: 0.75rem 1.25rem;\n  flex-shrink: 0;\n}\n.topbar-left {\n  display: flex;\n  align-items: center;\n  gap: 0.75rem;\n}\n.topbar-left .breadcrumb {\n  font-size: 0.75rem;\n  color: #9ca3af;\n}\n.topbar-left .breadcrumb .crumb-link {\n  cursor: pointer;\n}\n.topbar-left .breadcrumb .crumb-link:hover {\n  text-decoration: underline;\n}\n.topbar-left h1 {\n  font-size: 1.25rem;\n  font-weight: 700;\n  margin: 0.15rem 0;\n}\n.topbar-left .subtitle {\n  font-size: 0.75rem;\n  color: #6b7280;\n}\n.topbar-right {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n}\n.topbar-right .icon-btn {\n  color: #9ca3af;\n  opacity: 0.6;\n}\n.topbar-right .publish-btn {\n  background: #f97316;\n  border-color: #f97316;\n  color: #fff;\n}\n.topbar-right .publish-btn:hover {\n  background: #ea580c;\n  border-color: #ea580c;\n}\n.body-row {\n  flex: 1;\n  display: flex;\n  min-height: 0;\n}\n.canvas-wrapper {\n  flex: 1;\n  overflow: hidden;\n  position: relative;\n  background-color: #f9fafb;\n  cursor: grab;\n}\n.canvas-wrapper:active {\n  cursor: grabbing;\n}\n.canvas-inner {\n  position: relative;\n  min-width: 1200px;\n  min-height: 900px;\n  transform-origin: 0 0;\n  background-image:\n    radial-gradient(\n      circle,\n      #d1d5db 1px,\n      transparent 1px);\n  background-size: 20px 20px;\n}\n.zoom-controls {\n  position: absolute;\n  right: 1rem;\n  bottom: 1rem;\n  z-index: 10;\n  display: flex;\n  align-items: center;\n  gap: 0.15rem;\n  background: #fff;\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.25rem 0.4rem;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);\n}\n.zoom-controls .zoom-pct {\n  font-size: 0.7rem;\n  color: #6b7280;\n  width: 2.75rem;\n  text-align: center;\n}\n.edges-svg {\n  position: absolute;\n  top: 0;\n  left: 0;\n  z-index: 0;\n  pointer-events: none;\n}\n.edge-label {\n  position: absolute;\n  transform: translate(-50%, -50%);\n  z-index: 1;\n  font-size: 0.7rem;\n  font-weight: 600;\n  padding: 2px 8px;\n  border-radius: 10px;\n  white-space: nowrap;\n}\n.edge-label-true {\n  background: #dcfce7;\n  color: #166534;\n}\n.edge-label-false {\n  background: #ede9fe;\n  color: #6d28d9;\n}\n.node-card {\n  position: absolute;\n  z-index: 2;\n  background: #fff;\n  border: 2px solid #e5e7eb;\n  border-radius: 8px;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);\n  overflow: hidden;\n}\n.node-card.selected {\n  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.25);\n}\n.node-card.executing {\n  animation: wb-pulse 1s ease-in-out infinite;\n}\n.node-card.node-end {\n  border-radius: 24px;\n  text-align: center;\n}\n.node-card.node-end .node-header {\n  justify-content: center;\n  border-radius: 24px;\n}\n.node-header {\n  display: flex;\n  align-items: center;\n  gap: 0.4rem;\n  padding: 0.5rem 0.65rem;\n  cursor: grab;\n  font-size: 0.8rem;\n  font-weight: 600;\n  color: #1f2937;\n}\n.node-header:active {\n  cursor: grabbing;\n}\n.node-icon {\n  font-size: 0.75rem;\n}\n@keyframes wb-pulse {\n  0%, 100% {\n    box-shadow: 0 0 0 3px rgba(249, 115, 22, 0.6);\n  }\n  50% {\n    box-shadow: 0 0 0 6px rgba(249, 115, 22, 0.25);\n  }\n}\n.node-title {\n  flex: 1;\n}\n.node-badge {\n  font-size: 0.6rem;\n  font-weight: 600;\n  background: rgba(0, 0, 0, 0.06);\n  color: #4b5563;\n  padding: 2px 6px;\n  border-radius: 8px;\n}\n.node-body {\n  padding: 0.5rem 0.65rem;\n  font-size: 0.75rem;\n  color: #4b5563;\n  border-top: 1px solid #f1f5f9;\n}\n.port {\n  position: absolute;\n  z-index: 5;\n  width: 12px;\n  height: 12px;\n  border-radius: 50%;\n  background: #fff;\n  border: 2px solid #6b7280;\n}\n.port-output {\n  cursor: crosshair;\n}\n.port-output:hover {\n  border-color: #2563eb;\n  background: #dbeafe;\n  transform: scale(1.2);\n}\n.port-input {\n  pointer-events: none;\n  border-color: #9ca3af;\n}\n.edge-hit {\n  cursor: pointer;\n  stroke: transparent;\n  pointer-events: stroke;\n}\n.edge-hit.edge-hit-selected {\n  stroke: rgba(37, 99, 235, 0.15);\n}\n.edge-delete-btn {\n  position: absolute;\n  z-index: 6;\n  transform: translate(calc(-50% + 22px), -50%);\n  width: 20px;\n  height: 20px;\n  border-radius: 50%;\n  border: none;\n  background: #dc2626;\n  color: #fff;\n  font-size: 0.65rem;\n  line-height: 1;\n  cursor: pointer;\n}\n.edge-delete-btn:hover {\n  background: #b91c1c;\n}\n.properties-panel {\n  width: 360px;\n  flex-shrink: 0;\n  background: #fff;\n  border-left: 1px solid #e5e7eb;\n  overflow-y: auto;\n  padding: 1rem 1.25rem;\n}\n.panel-header {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  font-weight: 700;\n  font-size: 0.95rem;\n  margin-bottom: 1rem;\n}\n.panel-header .panel-header-actions {\n  display: flex;\n  gap: 0.6rem;\n}\n.panel-header .close-btn {\n  border: none;\n  background: none;\n  cursor: pointer;\n  color: #6b7280;\n}\n.panel-header .close-btn:hover {\n  color: #111827;\n}\n.panel-type-row {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 1rem;\n}\n.panel-type-row .type-badge {\n  border: 2px solid #e5e7eb;\n  border-radius: 6px;\n  padding: 0.25rem 0.6rem;\n  font-size: 0.75rem;\n  font-weight: 600;\n}\n.form-field {\n  margin-bottom: 1rem;\n  display: flex;\n  flex-direction: column;\n  gap: 0.375rem;\n}\n.form-field label {\n  font-size: 0.75rem;\n  font-weight: 600;\n  color: #374151;\n}\n.form-field input,\n.form-field textarea {\n  width: 100%;\n}\n.section-title {\n  font-size: 0.8rem;\n  font-weight: 700;\n  color: #111827;\n  margin-bottom: 0.5rem;\n}\n.group-card {\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.75rem;\n  margin-bottom: 0.75rem;\n}\n.group-header {\n  font-size: 0.75rem;\n  font-weight: 700;\n  color: #6d28d9;\n  margin-bottom: 0.5rem;\n}\n.branch-name-row {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n}\n.branch-name-row .branch-name-input {\n  flex: 1;\n}\n.condition-row {\n  display: grid;\n  grid-template-columns: 1fr 1fr 1fr auto;\n  gap: 0.35rem;\n  align-items: center;\n  margin-bottom: 0.5rem;\n}\n.row-remove {\n  border: none;\n  background: none;\n  color: #9ca3af;\n  cursor: pointer;\n}\n.row-remove:hover {\n  color: #dc2626;\n}\n.row-remove:disabled {\n  opacity: 0.35;\n  cursor: not-allowed;\n}\n.branch-card {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n  padding: 0.5rem 0;\n  border-bottom: 1px solid #f1f5f9;\n  font-size: 0.8rem;\n}\n.branch-pill {\n  font-size: 0.7rem;\n  font-weight: 700;\n  padding: 2px 8px;\n  border-radius: 10px;\n}\n.pill-true {\n  background: #dcfce7;\n  color: #166534;\n}\n.pill-false {\n  background: #ede9fe;\n  color: #6d28d9;\n}\n.pill-other {\n  background: #e0f2fe;\n  color: #0369a1;\n}\n.branch-target {\n  flex: 1;\n  color: #374151;\n}\n.muted {\n  color: #9ca3af;\n  font-size: 0.8rem;\n}\n.execution-row {\n  border: 1px solid #e5e7eb;\n  border-radius: 8px;\n  padding: 0.75rem;\n  margin-bottom: 0.75rem;\n}\n.execution-head {\n  display: flex;\n  align-items: center;\n  gap: 0.6rem;\n  margin-bottom: 0.5rem;\n}\n.error-text {\n  color: #dc2626;\n  font-size: 0.8rem;\n  margin-bottom: 0.5rem;\n}\n.execution-row pre,\n.run-result pre {\n  background: #f9fafb;\n  border-radius: 6px;\n  padding: 0.5rem;\n  font-size: 0.7rem;\n  max-height: 200px;\n  overflow: auto;\n  margin: 0;\n}\n.run-result {\n  margin-top: 0.5rem;\n}\n.live-step {\n  display: flex;\n  align-items: center;\n  gap: 0.5rem;\n  font-size: 0.75rem;\n  padding: 0.35rem 0;\n  border-bottom: 1px solid #f1f5f9;\n}\n.live-step .step-type {\n  text-transform: uppercase;\n  font-size: 0.6rem;\n  font-weight: 700;\n  color: #9ca3af;\n}\n.live-step .step-node {\n  font-weight: 600;\n  color: #374151;\n}\n/*# sourceMappingURL=workflow-builder.component.css.map */\n"] }]
  }], () => [{ type: ActivatedRoute }, { type: Router }, { type: WorkflowApiService }, { type: NotificationService }, { type: ConfirmationService }], { canvasWrapperRef: [{
    type: ViewChild,
    args: ["canvasWrapperEl"]
  }], onDocKeyDown: [{
    type: HostListener,
    args: ["document:keydown", ["$event"]]
  }], onDocMouseMove: [{
    type: HostListener,
    args: ["document:mousemove", ["$event"]]
  }], onDocMouseUp: [{
    type: HostListener,
    args: ["document:mouseup"]
  }] });
})();
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && \u0275setClassDebugInfo(WorkflowBuilderComponent, { className: "WorkflowBuilderComponent", filePath: "app/features/workflows/workflow-builder.component.ts", lineNumber: 67 });
})();
export {
  WorkflowBuilderComponent
};
//# sourceMappingURL=chunk-GTLLTPTR.js.map
