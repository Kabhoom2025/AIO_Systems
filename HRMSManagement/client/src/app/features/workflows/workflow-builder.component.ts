import { Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription, forkJoin } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  WorkflowApiService,
  WorkflowDefinitionDto,
  WorkflowVersionDto,
  WorkflowGraph,
  WorkflowNode,
  WorkflowEdge,
  TriggerData,
  ConditionData,
  ConditionGroup,
  ConditionRule,
  ActionData,
  TriggerTypeDto,
  ActionTypeDto,
  WorkflowFieldDto,
  WorkflowExecutionDto
} from '../../core/workflow-api.service';

const NODE_WIDTH = 220;

@Component({
  selector: 'app-workflow-builder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    DialogModule,
    DropdownModule,
    InputTextModule,
    TagModule,
    TableModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './workflow-builder.component.html',
  styleUrl: './workflow-builder.component.scss'
})
export class WorkflowBuilderComponent implements OnInit, OnDestroy {
  @ViewChild('canvasEl') canvasElRef!: ElementRef<HTMLDivElement>;

  workflowId!: number;
  workflow: WorkflowDefinitionDto | null = null;
  draftVersion: WorkflowVersionDto | null = null;
  graph: WorkflowGraph = { nodes: [], edges: [] };

  loading = false;
  previewMode = false;

  triggerTypes: TriggerTypeDto[] = [];
  actionTypes: ActionTypeDto[] = [];
  triggerFields: WorkflowFieldDto[] = [];

  selectedNode: WorkflowNode | null = null;
  showPropertiesPanel = false;

  addMenuNodeId: string | null = null;

  lastSavedAt: Date | null = null;
  savedText = '';

  showExecutionsDialog = false;
  executions: WorkflowExecutionDto[] = [];
  executionsLoading = false;

  draggingNode: WorkflowNode | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;

  private autosave$ = new Subject<void>();
  private subs: Subscription[] = [];
  private tickHandle: ReturnType<typeof setInterval> | null = null;

  operatorsByType: Record<string, string[]> = {
    number: ['Equals', 'NotEquals', 'GreaterThan', 'GreaterThanOrEqual', 'LessThan', 'LessThanOrEqual'],
    boolean: ['Equals', 'NotEquals'],
    string: ['Equals', 'NotEquals', 'Contains']
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: WorkflowApiService,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.workflowId = Number(idParam);
    this.loadAll();

    this.subs.push(
      this.autosave$.pipe(debounceTime(1000)).subscribe(() => this.persistDraft())
    );

    this.tickHandle = setInterval(() => this.computeSavedText(), 15000);
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    if (this.tickHandle) clearInterval(this.tickHandle);
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
        this.triggerFields = triggers.find(t => t.key === workflow.triggerType)?.fields ?? [];
        try {
          const parsed = JSON.parse(draft.graphJson || '{}');
          this.graph = { nodes: parsed.nodes ?? [], edges: parsed.edges ?? [] };
        } catch {
          this.graph = { nodes: [], edges: [] };
        }
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load workflow.');
      }
    });
  }

  // ---------------- Autosave ----------------
  scheduleAutosave() {
    if (this.previewMode) return;
    this.autosave$.next();
  }

  private persistDraft() {
    this.api.saveDraft(this.workflowId, { graphJson: JSON.stringify(this.graph) }).subscribe({
      next: version => {
        this.draftVersion = version;
        this.lastSavedAt = new Date();
        this.computeSavedText();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to autosave workflow.')
    });
  }

  private computeSavedText() {
    if (!this.lastSavedAt) {
      this.savedText = '';
      return;
    }
    const diffMs = Date.now() - this.lastSavedAt.getTime();
    const mins = Math.floor(diffMs / 60000);
    if (mins < 1) this.savedText = 'Saved just now';
    else if (mins === 1) this.savedText = 'Saved 1 min ago';
    else this.savedText = `Saved ${mins} min ago`;
  }

  // ---------------- Graph helpers ----------------
  nodeById(id: string): WorkflowNode | undefined {
    return this.graph.nodes.find(n => n.id === id);
  }

  outgoingEdges(nodeId: string): WorkflowEdge[] {
    return this.graph.edges.filter(e => e.source === nodeId);
  }

  incomingEdge(nodeId: string): WorkflowEdge | undefined {
    return this.graph.edges.find(e => e.target === nodeId);
  }

  isLeaf(node: WorkflowNode): boolean {
    return node.type !== 'end' && this.outgoingEdges(node.id).length === 0;
  }

  nodeHeight(node: WorkflowNode): number {
    return node.type === 'end' ? 56 : 96;
  }

  nodeWidth(node: WorkflowNode): number {
    return node.type === 'end' ? 120 : NODE_WIDTH;
  }

  nodeBorderColor(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger':
        return '#fca5a5';
      case 'condition':
        return '#c4b5fd';
      case 'end':
        return '#9ca3af';
      case 'action': {
        const inEdge = this.incomingEdge(node.id);
        if (inEdge?.branch === 'true') return '#86efac';
        if (inEdge?.branch === 'false') return '#d8b4fe';
        return '#93c5fd';
      }
      default:
        return '#e5e7eb';
    }
  }

  nodeHeaderBg(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger':
        return '#fef2f2';
      case 'condition':
        return '#f5f3ff';
      case 'end':
        return '#f3f4f6';
      case 'action': {
        const inEdge = this.incomingEdge(node.id);
        if (inEdge?.branch === 'true') return '#f0fdf4';
        if (inEdge?.branch === 'false') return '#faf5ff';
        return '#eff6ff';
      }
      default:
        return '#f9fafb';
    }
  }

  nodeTypeLabel(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger':
        return 'Trigger';
      case 'condition':
        return 'If / else';
      case 'end':
        return 'End';
      case 'action':
        return (node.data as ActionData).label || 'Action';
      default:
        return '';
    }
  }

  triggerBodyText(node: WorkflowNode): string {
    return (node.data as TriggerData).label || 'Not configured';
  }

  conditionSummary(node: WorkflowNode): string {
    const edges = this.outgoingEdges(node.id);
    if (edges.length === 0) return 'Not configured';
    return edges.map(e => `→ ${e.label ?? e.branch}`).join('  /  ');
  }

  actionBodyText(node: WorkflowNode): string {
    const data = node.data as ActionData;
    if (!data.actionType) return 'Not configured';
    switch (data.actionType) {
      case 'Notify':
        return `Notify: ${data.config?.['recipient'] ?? ''}`;
      case 'AutoApprove':
        return 'Auto Approve';
      case 'AutoReject':
        return 'Auto Reject';
      default:
        return data.label || data.actionType;
    }
  }

  trackById(_index: number, node: WorkflowNode): string {
    return node.id;
  }

  triggerFieldOptions(rule: ConditionRule): string[] {
    return this.triggerFields.find(f => f.key === rule.field)?.options ?? [];
  }

  private newId(prefix: string): string {
    return `${prefix}-${Date.now()}-${Math.floor(Math.random() * 100000)}`;
  }

  // ---------------- SVG edge rendering ----------------
  edgePath(edge: WorkflowEdge): string {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t) return '';
    const sx = s.x + this.nodeWidth(s) / 2;
    const sy = s.y + this.nodeHeight(s);
    const tx = t.x + this.nodeWidth(t) / 2;
    const ty = t.y;
    const my = sy + (ty - sy) / 2;
    return `M ${sx} ${sy} L ${sx} ${my} L ${tx} ${my} L ${tx} ${ty}`;
  }

  edgeLabelStyle(edge: WorkflowEdge): { [k: string]: string } {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t) return { display: 'none' };
    const sx = s.x + this.nodeWidth(s) / 2;
    const sy = s.y + this.nodeHeight(s);
    const tx = t.x + this.nodeWidth(t) / 2;
    const ty = t.y;
    const my = sy + (ty - sy) / 2;
    const midX = (sx + tx) / 2;
    return { left: `${midX}px`, top: `${my}px` };
  }

  edgeLabelClass(edge: WorkflowEdge): string {
    return edge.branch === 'false' ? 'edge-label edge-label-false' : 'edge-label edge-label-true';
  }

  canvasWidth(): number {
    const maxX = Math.max(1200, ...this.graph.nodes.map(n => n.x + this.nodeWidth(n) + 100));
    return maxX;
  }

  canvasHeight(): number {
    const maxY = Math.max(900, ...this.graph.nodes.map(n => n.y + this.nodeHeight(n) + 150));
    return maxY;
  }

  // ---------------- Selection / properties panel ----------------
  selectNode(node: WorkflowNode) {
    this.addMenuNodeId = null;
    this.selectedNode = node;
    this.showPropertiesPanel = true;
  }

  closePanel() {
    this.selectedNode = null;
    this.showPropertiesPanel = false;
  }

  asTriggerData(node: WorkflowNode): TriggerData {
    return node.data as TriggerData;
  }

  asConditionData(node: WorkflowNode): ConditionData {
    return node.data as ConditionData;
  }

  asActionData(node: WorkflowNode): ActionData {
    return node.data as ActionData;
  }

  descriptionOf(node: WorkflowNode): string {
    return (node.data as any).description ?? '';
  }

  onDescriptionChange(node: WorkflowNode, value: string) {
    (node.data as any).description = value;
    this.scheduleAutosave();
  }

  // ---------------- Condition editing ----------------
  operatorOptionsFor(rule: ConditionRule): string[] {
    const field = this.triggerFields.find(f => f.key === rule.field);
    const type = field?.type ?? 'string';
    return this.operatorsByType[type] ?? this.operatorsByType['string'];
  }

  valueKind(rule: ConditionRule): 'options' | 'boolean' | 'number' | 'text' {
    const field = this.triggerFields.find(f => f.key === rule.field);
    if (!field) return 'text';
    if (field.type === 'boolean') return 'boolean';
    if (field.options && field.options.length) return 'options';
    if (field.type === 'number') return 'number';
    return 'text';
  }

  onConditionFieldChange(rule: ConditionRule) {
    const ops = this.operatorOptionsFor(rule);
    rule.operator = ops[0];
    rule.value = '';
    this.scheduleAutosave();
  }

  addCondition(group: ConditionGroup) {
    group.conditions.push({ field: this.triggerFields[0]?.key ?? '', operator: 'Equals', value: '' });
    this.scheduleAutosave();
  }

  removeCondition(group: ConditionGroup, idx: number) {
    group.conditions.splice(idx, 1);
    this.scheduleAutosave();
  }

  branchIdForGroupIndex(i: number): string {
    if (i === 0) return 'true';
    if (i === 1) return 'false';
    return `group-${i + 1}`;
  }

  branchLabelForGroupIndex(i: number): string {
    if (i === 0) return 'Is True';
    if (i === 1) return 'If False';
    return `Group ${i + 1}`;
  }

  edgeForBranch(nodeId: string, branch: string): WorkflowEdge | undefined {
    return this.graph.edges.find(e => e.source === nodeId && e.branch === branch);
  }

  addGroup(node: WorkflowNode) {
    const data = this.asConditionData(node);
    data.groups.push({ conditions: [] });
    const newIndex = data.groups.length - 1;
    const branchId = this.branchIdForGroupIndex(newIndex);
    const newAction: WorkflowNode = {
      id: this.newId('node'),
      type: 'action',
      x: node.x + 150 * newIndex,
      y: node.y + 150,
      data: { actionType: '', label: 'New Step', config: {} }
    };
    this.graph.nodes.push(newAction);
    this.graph.edges.push({
      id: this.newId('edge'),
      source: node.id,
      target: newAction.id,
      branch: branchId,
      label: this.branchLabelForGroupIndex(newIndex)
    });
    this.scheduleAutosave();
  }

  jumpToNode(nodeId: string) {
    const n = this.nodeById(nodeId);
    if (n) this.selectNode(n);
  }

  // ---------------- Action editing ----------------
  configFieldsFor(node: WorkflowNode): WorkflowFieldDto[] {
    const data = this.asActionData(node);
    const at = this.actionTypes.find(a => a.key === data.actionType);
    return at?.configFields ?? [];
  }

  onActionTypeChange(node: WorkflowNode) {
    const data = this.asActionData(node);
    const at = this.actionTypes.find(a => a.key === data.actionType);
    data.label = at?.label ?? data.actionType;
    data.config = {};
    this.scheduleAutosave();
  }

  updateConfigValue(node: WorkflowNode, key: string, value: string) {
    const data = this.asActionData(node);
    data.config = { ...data.config, [key]: value };
    this.scheduleAutosave();
  }

  // ---------------- Add-node flow ----------------
  toggleAddMenu(nodeId: string, event: MouseEvent) {
    event.stopPropagation();
    this.addMenuNodeId = this.addMenuNodeId === nodeId ? null : nodeId;
  }

  addStep(leafId: string) {
    const leaf = this.nodeById(leafId);
    if (!leaf) return;
    const id = this.newId('node');
    const newNode: WorkflowNode = {
      id,
      type: 'action',
      x: leaf.x,
      y: leaf.y + 150,
      data: { actionType: '', label: 'New Step', config: {} }
    };
    this.graph.nodes.push(newNode);
    this.graph.edges.push({ id: this.newId('edge'), source: leafId, target: id });
    this.addMenuNodeId = null;
    this.selectNode(newNode);
    this.scheduleAutosave();
  }

  addIfElse(leafId: string) {
    const leaf = this.nodeById(leafId);
    if (!leaf) return;
    const condId = this.newId('node');
    const trueId = this.newId('node');
    const falseId = this.newId('node');

    const condNode: WorkflowNode = {
      id: condId,
      type: 'condition',
      x: leaf.x,
      y: leaf.y + 150,
      data: { label: 'If / else', groups: [{ conditions: [] }] }
    };
    const trueNode: WorkflowNode = {
      id: trueId,
      type: 'action',
      x: leaf.x - 140,
      y: leaf.y + 300,
      data: { actionType: '', label: 'New Step', config: {} }
    };
    const falseNode: WorkflowNode = {
      id: falseId,
      type: 'action',
      x: leaf.x + 140,
      y: leaf.y + 300,
      data: { actionType: '', label: 'New Step', config: {} }
    };

    this.graph.nodes.push(condNode, trueNode, falseNode);
    this.graph.edges.push(
      { id: this.newId('edge'), source: leafId, target: condId },
      { id: this.newId('edge'), source: condId, target: trueId, branch: 'true', label: 'Is True' },
      { id: this.newId('edge'), source: condId, target: falseId, branch: 'false', label: 'If False' }
    );
    this.addMenuNodeId = null;
    this.selectNode(condNode);
    this.scheduleAutosave();
  }

  addEnd(leafId: string) {
    const leaf = this.nodeById(leafId);
    if (!leaf) return;
    const id = this.newId('node');
    const newNode: WorkflowNode = { id, type: 'end', x: leaf.x + 50, y: leaf.y + 150, data: {} };
    this.graph.nodes.push(newNode);
    this.graph.edges.push({ id: this.newId('edge'), source: leafId, target: id });
    this.addMenuNodeId = null;
    this.scheduleAutosave();
  }

  // ---------------- Dragging ----------------
  onNodeMouseDown(event: MouseEvent, node: WorkflowNode) {
    if (this.previewMode) return;
    event.preventDefault();
    event.stopPropagation();
    this.draggingNode = node;
    const rect = this.canvasElRef.nativeElement.getBoundingClientRect();
    this.dragOffsetX = event.clientX - rect.left - node.x;
    this.dragOffsetY = event.clientY - rect.top - node.y;
  }

  @HostListener('document:mousemove', ['$event'])
  onDocMouseMove(event: MouseEvent) {
    if (!this.draggingNode || !this.canvasElRef) return;
    const rect = this.canvasElRef.nativeElement.getBoundingClientRect();
    const x = Math.max(0, event.clientX - rect.left - this.dragOffsetX);
    const y = Math.max(0, event.clientY - rect.top - this.dragOffsetY);
    this.draggingNode.x = x;
    this.draggingNode.y = y;
  }

  @HostListener('document:mouseup')
  onDocMouseUp() {
    if (this.draggingNode) {
      this.draggingNode = null;
      this.scheduleAutosave();
    }
  }

  onCanvasClick() {
    this.addMenuNodeId = null;
  }

  // ---------------- Top bar actions ----------------
  togglePreview() {
    this.previewMode = !this.previewMode;
    if (this.previewMode) {
      this.closePanel();
      this.addMenuNodeId = null;
    }
  }

  confirmPublish() {
    this.confirmation.confirm({
      message: 'Publish this workflow? It will start running immediately.',
      header: 'Publish Workflow',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.doPublish()
    });
  }

  private doPublish() {
    this.api.publish(this.workflowId).subscribe({
      next: wf => {
        this.workflow = wf;
        this.notify.success('Workflow published successfully.');
        this.api.getDraft(this.workflowId).subscribe({
          next: draft => (this.draftVersion = draft),
          error: () => {}
        });
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to publish workflow.')
    });
  }

  backToList() {
    this.router.navigate(['/workflows']);
  }

  // ---------------- Executions ----------------
  openExecutions() {
    this.showExecutionsDialog = true;
    this.executionsLoading = true;
    this.api.getExecutions(this.workflowId).subscribe({
      next: data => {
        this.executions = data;
        this.executionsLoading = false;
      },
      error: err => {
        this.executionsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load executions.');
      }
    });
  }

  prettyPath(json: string): string {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }

  executionSeverity(status: string): 'success' | 'danger' | 'warn' | 'info' {
    switch (status) {
      case 'Completed':
      case 'Success':
        return 'success';
      case 'Failed':
      case 'Error':
        return 'danger';
      default:
        return 'info';
    }
  }
}
