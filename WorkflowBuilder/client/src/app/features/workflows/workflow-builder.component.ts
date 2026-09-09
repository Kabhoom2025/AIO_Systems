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
import { InputTextarea } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { NotificationService } from '../../core/notification.service';
import { NodePaletteComponent, PaletteNodeKind } from './node-palette.component';
import { environment } from '../../../environments/environment';
import {
  WorkflowApiService,
  WorkflowDefinitionDto,
  WorkflowVersionDto,
  WorkflowGraph,
  WorkflowNode,
  WorkflowEdge,
  TriggerData,
  ConditionData,
  DecisionBranch,
  ConditionRule,
  ActionData,
  TriggerTypeDto,
  ActionTypeDto,
  WorkflowFieldDto,
  WorkflowExecutionDto,
  WorkflowStepEvent
} from '../../core/workflow-api.service';

const NODE_WIDTH = 220;

interface Point { x: number; y: number; }
interface Port { id: string; label: string; }

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
    InputTextarea,
    TagModule,
    TableModule,
    ToastModule,
    ConfirmDialogModule,
    NodePaletteComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './workflow-builder.component.html',
  styleUrl: './workflow-builder.component.scss'
})
export class WorkflowBuilderComponent implements OnInit, OnDestroy {
  @ViewChild('canvasWrapperEl') canvasWrapperRef!: ElementRef<HTMLDivElement>;

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

  selectedEdgeId: string | null = null;

  lastSavedAt: Date | null = null;
  savedText = '';

  showExecutionsDialog = false;
  executions: WorkflowExecutionDto[] = [];
  executionsLoading = false;

  showRunDialog = false;
  running = false;
  runContextJson = '{}';
  lastRunResult: WorkflowExecutionDto | null = null;

  draggingNode: WorkflowNode | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;

  connectingFrom: { nodeId: string; portId: string } | null = null;
  connectingCursor: Point | null = null;

  // ---------------- Zoom / pan ----------------
  viewport = { scale: 1, x: 0, y: 0 };
  private isPanning = false;
  private panStartClientX = 0;
  private panStartClientY = 0;
  private panStartOffsetX = 0;
  private panStartOffsetY = 0;

  // ---------------- Undo / redo ----------------
  private historyStack: string[] = [];
  private historyIndex = -1;
  private readonly maxHistory = 50;

  // ---------------- Copy / paste ----------------
  clipboardNode: WorkflowNode | null = null;

  // ---------------- Live Test Run streaming ----------------
  liveSteps: WorkflowStepEvent[] = [];
  executingNodeId: string | null = null;

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
        this.historyStack = [JSON.stringify(this.graph)];
        this.historyIndex = 0;
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

  // ---------------- Undo / redo ----------------
  // Snapshots are taken before each discrete structural mutation (add/delete/move/connect a node
  // or branch, copy/paste) — not on every field edit in the properties panel.
  private pushHistory() {
    const snapshot = JSON.stringify(this.graph);
    if (this.historyStack[this.historyIndex] === snapshot) return;
    this.historyStack = this.historyStack.slice(0, this.historyIndex + 1);
    this.historyStack.push(snapshot);
    if (this.historyStack.length > this.maxHistory) this.historyStack.shift();
    this.historyIndex = this.historyStack.length - 1;
  }

  canUndo(): boolean {
    return this.historyIndex > 0;
  }

  canRedo(): boolean {
    return this.historyIndex < this.historyStack.length - 1;
  }

  undo() {
    if (!this.canUndo()) return;
    this.historyIndex--;
    this.graph = JSON.parse(this.historyStack[this.historyIndex]);
    this.closePanel();
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }

  redo() {
    if (!this.canRedo()) return;
    this.historyIndex++;
    this.graph = JSON.parse(this.historyStack[this.historyIndex]);
    this.closePanel();
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }

  // ---------------- Copy / paste ----------------
  copyNode() {
    if (!this.selectedNode || this.selectedNode.type === 'trigger') return;
    this.clipboardNode = JSON.parse(JSON.stringify(this.selectedNode));
    this.notify.info('Node copied.');
  }

  pasteNode() {
    if (!this.clipboardNode) return;
    this.pushHistory();
    const clone: WorkflowNode = JSON.parse(JSON.stringify(this.clipboardNode));
    clone.id = this.newId('node');
    clone.x += 40;
    clone.y += 40;
    if (clone.type === 'condition') {
      (clone.data as ConditionData).branches = (clone.data as ConditionData).branches.map(b => ({
        ...b,
        id: this.newId('branch')
      }));
    }
    this.graph.nodes.push(clone);
    this.selectNode(clone);
    this.scheduleAutosave();
  }

  // ---------------- Keyboard shortcuts ----------------
  @HostListener('document:keydown', ['$event'])
  onDocKeyDown(event: KeyboardEvent) {
    if (this.previewMode) return;
    const target = event.target as HTMLElement | null;
    const isTyping = !!target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);
    if (isTyping) return;

    const ctrl = event.ctrlKey || event.metaKey;
    const key = event.key.toLowerCase();

    if (ctrl && key === 'z') {
      event.preventDefault();
      if (event.shiftKey) this.redo(); else this.undo();
      return;
    }
    if (ctrl && key === 'y') {
      event.preventDefault();
      this.redo();
      return;
    }
    if (ctrl && key === 'c') {
      if (this.selectedNode) {
        event.preventDefault();
        this.copyNode();
      }
      return;
    }
    if (ctrl && key === 'v') {
      if (this.clipboardNode) {
        event.preventDefault();
        this.pasteNode();
      }
      return;
    }
    if (event.key === 'Delete' || event.key === 'Backspace') {
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
  nodeById(id: string): WorkflowNode | undefined {
    return this.graph.nodes.find(n => n.id === id);
  }

  outgoingEdges(nodeId: string): WorkflowEdge[] {
    return this.graph.edges.filter(e => e.source === nodeId);
  }

  incomingEdge(nodeId: string): WorkflowEdge | undefined {
    return this.graph.edges.find(e => e.target === nodeId);
  }

  nodeHeight(node: WorkflowNode): number {
    return node.type === 'end' ? 56 : 96;
  }

  nodeWidth(node: WorkflowNode): number {
    if (node.type === 'end') return 120;
    if (node.type === 'condition') {
      const branchCount = this.asConditionData(node).branches?.length ?? 1;
      return Math.max(NODE_WIDTH, branchCount * 130);
    }
    return NODE_WIDTH;
  }

  nodeBorderColor(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger': return '#fca5a5';
      case 'condition': return '#c4b5fd';
      case 'end': return '#9ca3af';
      case 'action': return '#93c5fd';
      default: return '#e5e7eb';
    }
  }

  nodeHeaderBg(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger': return '#fef2f2';
      case 'condition': return '#f5f3ff';
      case 'end': return '#f3f4f6';
      case 'action': return '#eff6ff';
      default: return '#f9fafb';
    }
  }

  nodeTypeLabel(node: WorkflowNode): string {
    switch (node.type) {
      case 'trigger': return 'Trigger';
      case 'condition': return 'Decision';
      case 'end': return 'End';
      case 'action': return (node.data as ActionData).label || 'Action';
      default: return '';
    }
  }

  triggerBodyText(node: WorkflowNode): string {
    return (node.data as TriggerData).label || 'Not configured';
  }

  conditionSummary(node: WorkflowNode): string {
    const branches = this.asConditionData(node).branches ?? [];
    if (branches.length === 0) return 'Not configured';
    return branches.map(b => `→ ${b.name}`).join('  /  ');
  }

  actionBodyText(node: WorkflowNode): string {
    const data = node.data as ActionData;
    if (!data.actionType) return 'Not configured';
    switch (data.actionType) {
      case 'HttpRequest': return `HTTP ${data.config?.['method'] ?? 'GET'} ${data.config?.['url'] ?? ''}`;
      case 'SetVariable': return `Set ${data.config?.['key'] ?? ''}`;
      case 'LogMessage': return `Log: ${data.config?.['message'] ?? ''}`;
      case 'Delay': return `Wait ${data.config?.['seconds'] ?? '0'}s`;
      case 'Notify': return `Notify: ${data.config?.['title'] ?? ''}`;
      default: return data.label || data.actionType;
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

  // ---------------- Ports ----------------
  outputPorts(node: WorkflowNode): Port[] {
    if (node.type === 'end') return [];
    if (node.type === 'condition') {
      return (this.asConditionData(node).branches ?? []).map(b => ({ id: b.id, label: b.name }));
    }
    return [{ id: 'out', label: '' }];
  }

  hasInputPort(node: WorkflowNode): boolean {
    return node.type !== 'trigger';
  }

  outputPortPos(node: WorkflowNode, portId: string): Point {
    const ports = this.outputPorts(node);
    const idx = Math.max(0, ports.findIndex(p => p.id === portId));
    const count = Math.max(1, ports.length);
    const width = this.nodeWidth(node);
    return { x: node.x + ((idx + 1) * width) / (count + 1), y: node.y + this.nodeHeight(node) };
  }

  inputPortPos(node: WorkflowNode): Point {
    return { x: node.x + this.nodeWidth(node) / 2, y: node.y };
  }

  private portIdOfEdge(edge: WorkflowEdge): string {
    return edge.branch ?? 'out';
  }

  private nodeAtPoint(x: number, y: number, excludeId?: string): WorkflowNode | undefined {
    return this.graph.nodes.find(n => {
      if (n.id === excludeId) return false;
      const w = this.nodeWidth(n);
      const h = this.nodeHeight(n);
      return x >= n.x - 10 && x <= n.x + w + 10 && y >= n.y - 10 && y <= n.y + h + 10;
    });
  }

  // ---------------- SVG edge rendering ----------------
  edgePath(edge: WorkflowEdge): string {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t) return '';
    const { x: sx, y: sy } = this.outputPortPos(s, this.portIdOfEdge(edge));
    const { x: tx, y: ty } = this.inputPortPos(t);
    const my = sy + (ty - sy) / 2;
    return `M ${sx} ${sy} L ${sx} ${my} L ${tx} ${my} L ${tx} ${ty}`;
  }

  edgeLabelStyle(edge: WorkflowEdge): { [k: string]: string } {
    const s = this.nodeById(edge.source);
    const t = this.nodeById(edge.target);
    if (!s || !t) return { display: 'none' };
    const { x: sx, y: sy } = this.outputPortPos(s, this.portIdOfEdge(edge));
    const { x: tx, y: ty } = this.inputPortPos(t);
    const my = sy + (ty - sy) / 2;
    const midX = (sx + tx) / 2;
    return { left: `${midX}px`, top: `${my}px` };
  }

  connectingLinePath(): string {
    if (!this.connectingFrom || !this.connectingCursor) return '';
    const s = this.nodeById(this.connectingFrom.nodeId);
    if (!s) return '';
    const { x: sx, y: sy } = this.outputPortPos(s, this.connectingFrom.portId);
    return `M ${sx} ${sy} L ${this.connectingCursor.x} ${this.connectingCursor.y}`;
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
    this.selectedEdgeId = null;
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

  addCondition(branch: DecisionBranch) {
    branch.conditions.push({ field: this.triggerFields[0]?.key ?? '', operator: 'Equals', value: '' });
    this.scheduleAutosave();
  }

  removeCondition(branch: DecisionBranch, idx: number) {
    branch.conditions.splice(idx, 1);
    this.scheduleAutosave();
  }

  edgeForBranch(nodeId: string, branchId: string): WorkflowEdge | undefined {
    return this.graph.edges.find(e => e.source === nodeId && e.branch === branchId);
  }

  renameBranch(_branch: DecisionBranch, _name: string) {
    this.scheduleAutosave();
  }

  addBranch(node: WorkflowNode) {
    this.pushHistory();
    const data = this.asConditionData(node);
    const branch: DecisionBranch = { id: this.newId('branch'), name: `Branch #${data.branches.length + 1}`, conditions: [] };
    data.branches.push(branch);
    this.scheduleAutosave();
  }

  removeBranch(node: WorkflowNode, branchId: string) {
    this.pushHistory();
    const data = this.asConditionData(node);
    data.branches = data.branches.filter(b => b.id !== branchId);
    this.graph.edges = this.graph.edges.filter(e => !(e.source === node.id && e.branch === branchId));
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

  // ---------------- Drag-and-drop node creation ----------------
  onCanvasDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onCanvasDrop(event: DragEvent) {
    event.preventDefault();
    if (this.previewMode || !this.canvasWrapperRef) return;
    const kind = event.dataTransfer?.getData('text/kind') as PaletteNodeKind;
    if (!kind) return;
    const p = this.toLogicalPoint(event.clientX, event.clientY);
    this.addNodeAt(kind, Math.max(0, p.x - NODE_WIDTH / 2), Math.max(0, p.y - 20));
  }

  addNodeAt(kind: PaletteNodeKind, x: number, y: number) {
    const id = this.newId('node');
    let node: WorkflowNode;
    switch (kind) {
      case 'delay':
        node = { id, type: 'action', x, y, data: { actionType: 'Delay', label: 'Delay', config: { seconds: '5' } } };
        break;
      case 'notification':
        node = { id, type: 'action', x, y, data: { actionType: 'Notify', label: 'Notification', config: {} } };
        break;
      case 'decision':
        node = {
          id, type: 'condition', x, y,
          data: {
            label: 'Decision',
            branches: [
              { id: this.newId('branch'), name: 'Branch #1', conditions: [] },
              { id: this.newId('branch'), name: 'Branch #2', conditions: [] }
            ]
          }
        };
        break;
      case 'end':
        node = { id, type: 'end', x, y, data: {} };
        break;
      default:
        node = { id, type: 'action', x, y, data: { actionType: '', label: 'New Step', config: {} } };
    }
    this.pushHistory();
    this.graph.nodes.push(node);
    this.selectNode(node);
    this.scheduleAutosave();
  }

  deleteNode(node: WorkflowNode) {
    if (this.previewMode) return;
    if (node.type === 'trigger') {
      this.notify.warn("The trigger node can't be deleted — every workflow needs exactly one.");
      return;
    }
    this.pushHistory();
    this.graph.nodes = this.graph.nodes.filter(n => n.id !== node.id);
    this.graph.edges = this.graph.edges.filter(e => e.source !== node.id && e.target !== node.id);
    if (this.selectedNode?.id === node.id) this.closePanel();
    this.scheduleAutosave();
  }

  // ---------------- Wiring (manual connections) ----------------
  onPortMouseDown(event: MouseEvent, node: WorkflowNode, portId: string) {
    if (this.previewMode) return;
    event.preventDefault();
    event.stopPropagation();
    this.connectingFrom = { nodeId: node.id, portId };
    this.connectingCursor = this.toLogicalPoint(event.clientX, event.clientY);
  }

  private connectPorts(sourceId: string, portId: string, targetId: string) {
    const source = this.nodeById(sourceId);
    if (!source) return;
    this.pushHistory();
    this.graph.edges = this.graph.edges.filter(e => !(e.source === sourceId && this.portIdOfEdge(e) === portId));
    const branchLabel = source.type === 'condition'
      ? this.asConditionData(source).branches.find(b => b.id === portId)?.name
      : undefined;
    this.graph.edges.push({
      id: this.newId('edge'),
      source: sourceId,
      target: targetId,
      branch: portId === 'out' ? undefined : portId,
      label: branchLabel
    });
    this.scheduleAutosave();
  }

  selectEdge(edge: WorkflowEdge, event: MouseEvent) {
    event.stopPropagation();
    this.selectedEdgeId = edge.id;
  }

  deleteEdge(edgeId: string) {
    this.pushHistory();
    this.graph.edges = this.graph.edges.filter(e => e.id !== edgeId);
    this.selectedEdgeId = null;
    this.scheduleAutosave();
  }

  // ---------------- Dragging existing nodes ----------------
  onNodeMouseDown(event: MouseEvent, node: WorkflowNode) {
    if (this.previewMode) return;
    event.preventDefault();
    event.stopPropagation();
    this.pushHistory(); // snapshot the pre-drag position so it can be undone
    this.draggingNode = node;
    const p = this.toLogicalPoint(event.clientX, event.clientY);
    this.dragOffsetX = p.x - node.x;
    this.dragOffsetY = p.y - node.y;
  }

  @HostListener('document:mousemove', ['$event'])
  onDocMouseMove(event: MouseEvent) {
    if (!this.canvasWrapperRef) return;

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

  @HostListener('document:mouseup')
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
  private toLogicalPoint(clientX: number, clientY: number): Point {
    const rect = this.canvasWrapperRef.nativeElement.getBoundingClientRect();
    return {
      x: (clientX - rect.left - this.viewport.x) / this.viewport.scale,
      y: (clientY - rect.top - this.viewport.y) / this.viewport.scale
    };
  }

  canvasTransform(): string {
    return `translate(${this.viewport.x}px, ${this.viewport.y}px) scale(${this.viewport.scale})`;
  }

  onCanvasMouseDown(event: MouseEvent) {
    if (this.connectingFrom || this.draggingNode) return;
    this.isPanning = true;
    this.panStartClientX = event.clientX;
    this.panStartClientY = event.clientY;
    this.panStartOffsetX = this.viewport.x;
    this.panStartOffsetY = this.viewport.y;
  }

  onCanvasWheel(event: WheelEvent) {
    event.preventDefault();
    const rect = this.canvasWrapperRef.nativeElement.getBoundingClientRect();
    const cursor = { x: event.clientX - rect.left, y: event.clientY - rect.top };
    const factor = event.deltaY < 0 ? 1.1 : 1 / 1.1;
    this.setScale(this.viewport.scale * factor, cursor);
  }

  private setScale(newScale: number, cursor?: Point) {
    const clamped = Math.min(2, Math.max(0.25, newScale));
    if (cursor) {
      this.viewport.x = cursor.x - ((cursor.x - this.viewport.x) / this.viewport.scale) * clamped;
      this.viewport.y = cursor.y - ((cursor.y - this.viewport.y) / this.viewport.scale) * clamped;
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
    const minX = Math.min(...this.graph.nodes.map(n => n.x));
    const minY = Math.min(...this.graph.nodes.map(n => n.y));
    const maxX = Math.max(...this.graph.nodes.map(n => n.x + this.nodeWidth(n)));
    const maxY = Math.max(...this.graph.nodes.map(n => n.y + this.nodeHeight(n)));
    const contentW = Math.max(1, maxX - minX + 80);
    const contentH = Math.max(1, maxY - minY + 80);
    const scale = Math.min(2, Math.max(0.25, Math.min(rect.width / contentW, rect.height / contentH)));
    this.viewport = {
      scale,
      x: rect.width / 2 - ((minX + maxX) / 2) * scale,
      y: rect.height / 2 - ((minY + maxY) / 2) * scale
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
      message: 'Publish this workflow? It will start running immediately for webhook triggers.',
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

  // ---------------- Test run ----------------
  openRunDialog() {
    this.lastRunResult = null;
    this.showRunDialog = true;
  }

  runNow() {
    let context: Record<string, unknown>;
    try {
      context = JSON.parse(this.runContextJson || '{}');
    } catch {
      this.notify.error('Context must be valid JSON.');
      return;
    }

    this.running = true;
    this.liveSteps = [];
    this.executingNodeId = null;
    this.lastRunResult = null;

    const token = localStorage.getItem(environment.tokenKey);

    fetch(this.api.runStreamUrl(this.workflowId), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      },
      body: JSON.stringify({ context })
    })
      .then(response => {
        if (!response.ok || !response.body) throw new Error(`Request failed (${response.status}).`);
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = '';

        const pump = (): Promise<void> =>
          reader.read().then(({ done, value }) => {
            if (done) return;
            buffer += decoder.decode(value, { stream: true });
            const parts = buffer.split('\n\n');
            buffer = parts.pop() ?? '';
            for (const part of parts) {
              const line = part.trim();
              if (!line.startsWith('data:')) continue;
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
      })
      .then(() => {
        this.running = false;
        this.executingNodeId = null;
        if (this.lastRunResult?.status === 'Completed') this.notify.success('Test run completed.');
        else if (this.lastRunResult) this.notify.error(this.lastRunResult.errorMessage ?? 'Test run failed.');
      })
      .catch((err: any) => {
        this.running = false;
        this.executingNodeId = null;
        this.notify.error(err?.message ?? 'Failed to run workflow.');
      });
  }

  webhookUrl(): string {
    return this.workflow ? this.api.webhookUrl(this.workflow.webhookToken) : '';
  }
}
