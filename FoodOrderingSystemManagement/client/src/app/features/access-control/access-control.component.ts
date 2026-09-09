import {
  Component, inject, signal, computed, OnInit, OnDestroy,
  ViewChild, ElementRef, effect
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { MatIconModule }            from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { MatSelectModule }          from '@angular/material/select';
import { MatButtonModule }          from '@angular/material/button';

import { RoleService }             from '../../core/services/role.service';
import { UserService }             from '../../core/services/user.service';
import { RolePermissionService }   from '../../core/services/role-permission.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { AppUser }                 from '../../core/models/user.model';
import { Role }                    from '../../core/models/role.model';
import { APP_FEATURES, ALL_FEATURE_KEYS } from '../../core/constants/app-features.constant';

// ── Tree types (hierarchy view) ───────────────────────────────────────────────
export interface TreeNode {
  id: string; type: 'root' | 'role' | 'user';
  label: string; sublabel: string;
  x: number; y: number; w: number; h: number; cx: number;
  color: string; bgColor: string; icon: string;
  permCount: number; totalPerms: number;
  userData?: AppUser; roleData?: Role;
}
export interface TreeEdge { id: string; path: string; color: string; }
export interface TreeLayout { nodes: TreeNode[]; edges: TreeEdge[]; svgW: number; svgH: number; }

// ── 3D Network types ──────────────────────────────────────────────────────────
interface Net3DNode {
  id: string; type: 'root' | 'role' | 'user';
  label: string; shortLabel: string;
  color: string; baseR: number;
  ox: number; oy: number; oz: number;
}
interface Net3DEdge { a: string; b: string; color: string; dashed: boolean; }

// ── View mode ─────────────────────────────────────────────────────────────────
export type ViewMode = 'hierarchy' | 'network3d';

// ── Layout constants (hierarchy) ──────────────────────────────────────────────
const ROOT_W = 160, ROOT_H = 60;
const ROLE_W = 170, ROLE_H = 78;
const USER_W = 150, USER_H = 64;
const USER_GAP = 18, ROLE_GAP = 40, PAD = 60;
const L0_TOP = 30, L1_TOP = 180, L2_TOP = 360, BTMPAD = 50;

const PALETTES: Record<string, { color: string; bg: string }> = {
  admin:            { color: '#d84315', bg: '#fbe9e7' },
  cashier:          { color: '#1565c0', bg: '#e3f2fd' },
  waiter:           { color: '#2e7d32', bg: '#e8f5e9' },
  inventorymanager: { color: '#5e35b1', bg: '#ede7f6' },
};
const DEFAULT_PALETTE = { color: '#546e7a', bg: '#eceff1' };
const CONN_COLOR = '#90a4ae';

@Component({
  selector: 'app-access-control',
  standalone: true,
  imports: [
    CommonModule, MatIconModule, MatProgressSpinnerModule,
    MatTooltipModule, MatSelectModule, MatButtonModule,
    LoadingSpinnerComponent,
  ],
  templateUrl: './access-control.component.html',
  styleUrl:    './access-control.component.scss',
})
export class AccessControlComponent implements OnInit, OnDestroy {
  private roleService  = inject(RoleService);
  private userService  = inject(UserService);
  private permService  = inject(RolePermissionService);

  users     = signal<AppUser[]>([]);
  roles     = signal<Role[]>([]);
  rolePerms = signal<Map<number, string[]>>(new Map());
  loading   = signal(true);

  viewMode     = signal<ViewMode>('hierarchy');
  selectedNode = signal<TreeNode | null>(null);
  isPlaying    = signal(true);

  hZoom      = signal(1.0);
  hZoomPct   = signal(100);
  private readonly H_ZOOM_MIN  = 0.3;
  private readonly H_ZOOM_MAX  = 2.5;
  private readonly H_ZOOM_STEP = 0.15;

  hZoomIn():    void { this._hApplyZoom(this.hZoom() + this.H_ZOOM_STEP); }
  hZoomOut():   void { this._hApplyZoom(this.hZoom() - this.H_ZOOM_STEP); }
  hZoomReset(): void { this._hApplyZoom(1.0); }
  private _hApplyZoom(z: number): void {
    const clamped = Math.max(this.H_ZOOM_MIN, Math.min(this.H_ZOOM_MAX, z));
    this.hZoom.set(clamped);
    this.hZoomPct.set(Math.round(clamped * 100));
  }

  readonly featureGroups = APP_FEATURES;
  readonly totalPerms    = ALL_FEATURE_KEYS.length;

  readonly viewOptions: { value: ViewMode; label: string; icon: string }[] = [
    { value: 'hierarchy', label: 'Organization Chart',  icon: 'account_tree' },
    { value: 'network3d', label: '3D Network View',     icon: 'bubble_chart' },
  ];

  // ── Canvas (3D network) ───────────────────────────────────────────────────
  @ViewChild('networkCanvas')
  set networkCanvasRef(ref: ElementRef<HTMLCanvasElement> | undefined) {
    if (ref?.nativeElement) {
      setTimeout(() => this.initCanvas(ref.nativeElement), 30);
    } else {
      this.stopCanvas();
    }
  }

  private _canvas: HTMLCanvasElement | null = null;
  private _animId  = 0;
  private _angle   = 0;
  private _isDrag  = false;
  private _lastX   = 0;
  private _vel     = 0;
  private _zoom    = 1.0;
  private readonly ZOOM_MIN = 0.3;
  private readonly ZOOM_MAX = 2.8;
  private readonly ZOOM_STEP = 0.18;

  zoomLevel = signal(100); // exposed as % for display

  // ── Hierarchy layout ──────────────────────────────────────────────────────
  layout = computed<TreeLayout>(() => {
    const roles = this.roles(), users = this.users(), pm = this.rolePerms();
    if (!roles.length) return { nodes: [], edges: [], svgW: 800, svgH: 500 };

    const cols = roles.map(r => {
      const ru = users.filter(u => u.roleId === r.id);
      const cW = ru.length <= 1
        ? Math.max(ROLE_W, USER_W)
        : ru.length * USER_W + (ru.length - 1) * USER_GAP;
      return { role: r, users: ru, colW: Math.max(cW, ROLE_W) };
    });

    const totalW = cols.reduce((s, c) => s + c.colW, 0) + (cols.length - 1) * ROLE_GAP;
    const svgW   = Math.max(totalW + 2 * PAD, 800);
    const svgH   = L2_TOP + USER_H + BTMPAD;
    const nodes: TreeNode[] = [], edges: TreeEdge[] = [];

    const rootCX = svgW / 2;
    nodes.push({
      id: 'root', type: 'root', label: 'Restaurant', sublabel: 'Organization',
      x: rootCX - ROOT_W / 2, y: L0_TOP, w: ROOT_W, h: ROOT_H, cx: rootCX,
      color: '#bf360c', bgColor: '#fbe9e7', icon: 'restaurant', permCount: 0, totalPerms: 0,
    });

    let colX = (svgW - totalW) / 2;
    const roleCXs: number[] = [];

    for (const col of cols) {
      const rcx = colX + col.colW / 2;
      roleCXs.push(rcx);
      const key = col.role.roleName.toLowerCase().replace(/\s+/g, '');
      const pal = PALETTES[key] ?? DEFAULT_PALETTE;
      const perms = pm.get(col.role.id) ?? [];
      const permCount = col.role.roleName === 'Admin' ? this.totalPerms : perms.length;

      nodes.push({
        id: `role-${col.role.id}`, type: 'role',
        label: col.role.roleName, sublabel: `${col.users.length} user${col.users.length !== 1 ? 's' : ''}`,
        x: rcx - ROLE_W / 2, y: L1_TOP, w: ROLE_W, h: ROLE_H, cx: rcx,
        color: pal.color, bgColor: pal.bg, icon: this.roleIcon(col.role.roleName),
        permCount, totalPerms: this.totalPerms, roleData: col.role,
      });

      const roleBottom = L1_TOP + ROLE_H, mY2 = roleBottom + (L2_TOP - roleBottom) / 2;
      const gW = col.users.length * USER_W + (col.users.length - 1) * USER_GAP;
      let ux = rcx - gW / 2;
      const uCXs: number[] = [];

      for (const u of col.users) {
        const ucx = ux + USER_W / 2;
        uCXs.push(ucx);
        nodes.push({
          id: `user-${u.id}`, type: 'user',
          label: u.name, sublabel: u.email,
          x: ux, y: L2_TOP, w: USER_W, h: USER_H, cx: ucx,
          color: pal.color, bgColor: pal.bg, icon: '',
          permCount, totalPerms: this.totalPerms, userData: u, roleData: col.role,
        });
        ux += USER_W + USER_GAP;
      }

      if (uCXs.length === 1) {
        edges.push({ id: `e-r${col.role.id}-u`, path: `M ${rcx},${roleBottom} L ${uCXs[0]},${L2_TOP}`, color: CONN_COLOR });
      } else if (uCXs.length > 1) {
        edges.push({ id: `e-r${col.role.id}-stem`, path: `M ${rcx},${roleBottom} L ${rcx},${mY2}`, color: CONN_COLOR });
        edges.push({ id: `e-r${col.role.id}-bar`,  path: `M ${uCXs[0]},${mY2} L ${uCXs[uCXs.length-1]},${mY2}`, color: CONN_COLOR });
        uCXs.forEach((cx, i) => edges.push({ id: `e-r${col.role.id}-ud${i}`, path: `M ${cx},${mY2} L ${cx},${L2_TOP}`, color: CONN_COLOR }));
      }

      colX += col.colW + ROLE_GAP;
    }

    const rootBottom = L0_TOP + ROOT_H, mY1 = rootBottom + (L1_TOP - rootBottom) / 2;
    if (roleCXs.length === 1) {
      edges.push({ id: 'e-root-s', path: `M ${rootCX},${rootBottom} L ${roleCXs[0]},${L1_TOP}`, color: CONN_COLOR });
    } else {
      edges.push({ id: 'e-root-stem', path: `M ${rootCX},${rootBottom} L ${rootCX},${mY1}`, color: CONN_COLOR });
      edges.push({ id: 'e-root-bar',  path: `M ${roleCXs[0]},${mY1} L ${roleCXs[roleCXs.length-1]},${mY1}`, color: CONN_COLOR });
      roleCXs.forEach((cx, i) => edges.push({ id: `e-root-r${i}`, path: `M ${cx},${mY1} L ${cx},${L1_TOP}`, color: CONN_COLOR }));
    }

    return { nodes, edges, svgW, svgH };
  });

  // ── 3D Network data ───────────────────────────────────────────────────────
  netNodes = computed<Net3DNode[]>(() => {
    const roles = this.roles(), users = this.users();
    const nodes: Net3DNode[] = [{
      id: 'root', type: 'root', label: 'Restaurant', shortLabel: 'HQ',
      color: '#c62828', baseR: 40, ox: 0, oy: 0, oz: 0,
    }];

    const N = roles.length;
    roles.forEach((role, i) => {
      const θ = (2 * Math.PI * i / N) - Math.PI / 6;
      const R = 180;
      const key = role.roleName.toLowerCase().replace(/\s+/g, '');
      const pal = PALETTES[key] ?? DEFAULT_PALETTE;

      nodes.push({
        id: `role-${role.id}`, type: 'role',
        label: role.roleName, shortLabel: role.roleName,
        color: pal.color, baseR: 28,
        ox: R * Math.sin(θ), oy: -25 + 18 * Math.sin(i * 1.9), oz: R * Math.cos(θ),
      });

      const roleUsers = users.filter(u => u.roleId === role.id);
      const UR = 330, spread = Math.PI / 5;
      const startθ = θ - (spread * (roleUsers.length - 1)) / 2;

      roleUsers.forEach((u, j) => {
        const uθ = startθ + j * spread;
        nodes.push({
          id: `user-${u.id}`, type: 'user',
          label: u.name, shortLabel: u.name.split(' ')[0],
          color: pal.color, baseR: 21,
          ox: UR * Math.sin(uθ), oy: 55 + 25 * (j % 2 === 0 ? 1 : -1), oz: UR * Math.cos(uθ),
        });
      });
    });

    return nodes;
  });

  netEdges = computed<Net3DEdge[]>(() => {
    const roles = this.roles(), users = this.users();
    const edges: Net3DEdge[] = [];
    roles.forEach(r => {
      const key = r.roleName.toLowerCase().replace(/\s+/g, '');
      const col = (PALETTES[key] ?? DEFAULT_PALETTE).color;
      edges.push({ a: 'root', b: `role-${r.id}`, color: col, dashed: false });
      users.filter(u => u.roleId === r.id).forEach(u =>
        edges.push({ a: `role-${r.id}`, b: `user-${u.id}`, color: col, dashed: true })
      );
    });
    return edges;
  });

  // ── Detail panel ──────────────────────────────────────────────────────────
  detailPerms = computed<string[]>(() => {
    const node = this.selectedNode();
    if (!node) return [];
    const roleName = node.roleData?.roleName ?? node.userData?.roleName ?? '';
    if (roleName === 'Admin') return ALL_FEATURE_KEYS;
    const id = node.roleData?.id ?? node.userData?.roleId;
    return id != null ? (this.rolePerms().get(id) ?? []) : [];
  });

  detailGroups = computed(() => {
    const has = new Set(this.detailPerms());
    return this.featureGroups
      .map(g => ({ ...g, features: g.features.filter(f => has.has(f.key)) }))
      .filter(g => g.features.length > 0);
  });

  // ── Lifecycle ─────────────────────────────────────────────────────────────
  constructor() {
    effect(() => {
      if (this.viewMode() !== 'network3d') {
        this.stopCanvas();
      }
    });
  }

  ngOnInit(): void {
    forkJoin({ users: this.userService.getAll(), roles: this.roleService.getAll() })
      .subscribe({
        next: ({ users, roles }) => {
          this.users.set(users.data ?? []);
          this.roles.set(roles.data ?? []);
          this.loadAllPerms(roles.data ?? []);
        },
        error: () => this.loading.set(false),
      });
  }

  ngOnDestroy(): void { this.stopCanvas(); }

  private loadAllPerms(roles: Role[]): void {
    if (!roles.length) { this.loading.set(false); return; }
    forkJoin(
      roles.map(r =>
        this.permService.getForRole(r.id).pipe(
          map(res => ({ id: r.id, perms: (res.data ?? []) as string[] })),
          catchError(() => of({ id: r.id, perms: [] as string[] }))
        )
      )
    ).subscribe(results => {
      const m = new Map<number, string[]>();
      results.forEach(r => m.set(r.id, r.perms));
      this.rolePerms.set(m);
      this.loading.set(false);
    });
  }

  // ── Canvas 3D engine ──────────────────────────────────────────────────────
  private initCanvas(canvas: HTMLCanvasElement): void {
    this._canvas = canvas;
    const parent = canvas.parentElement!;
    const dpr    = window.devicePixelRatio || 1;
    canvas.width  = parent.clientWidth  * dpr;
    canvas.height = parent.clientHeight * dpr;
    canvas.style.width  = parent.clientWidth  + 'px';
    canvas.style.height = parent.clientHeight + 'px';
    canvas.getContext('2d')!.scale(dpr, dpr);

    canvas.addEventListener('mousedown',  this._onDown.bind(this));
    canvas.addEventListener('mousemove',  this._onMove.bind(this));
    canvas.addEventListener('mouseup',    this._onUp.bind(this));
    canvas.addEventListener('mouseleave', this._onUp.bind(this));
    canvas.addEventListener('touchstart', (e) => { e.preventDefault(); this._isDrag = true; this._lastX = e.touches[0].clientX; this._vel = 0; }, { passive: false });
    canvas.addEventListener('touchmove',  (e) => { e.preventDefault(); if (!this._isDrag) return; const dx = e.touches[0].clientX - this._lastX; this._angle += dx * 0.007; this._vel = dx * 0.007; this._lastX = e.touches[0].clientX; }, { passive: false });
    canvas.addEventListener('touchend',   () => this._isDrag = false);
    canvas.addEventListener('wheel', this._onWheel.bind(this), { passive: false });

    this.stopCanvas();
    this._animate();
  }

  private _onDown(e: MouseEvent) { this._isDrag = true; this._lastX = e.clientX; this._vel = 0; }
  private _onMove(e: MouseEvent) {
    if (!this._isDrag) return;
    const dx = e.clientX - this._lastX;
    this._angle += dx * 0.007;
    this._vel = dx * 0.007;
    this._lastX = e.clientX;
  }
  private _onUp() { this._isDrag = false; }
  private _onWheel(e: WheelEvent) {
    e.preventDefault();
    const delta = e.deltaY > 0 ? -this.ZOOM_STEP : this.ZOOM_STEP;
    this._applyZoom(this._zoom + delta);
  }

  private _applyZoom(z: number): void {
    this._zoom = Math.max(this.ZOOM_MIN, Math.min(this.ZOOM_MAX, z));
    this.zoomLevel.set(Math.round(this._zoom * 100));
  }

  zoomIn():    void { this._applyZoom(this._zoom + this.ZOOM_STEP); }
  zoomOut():   void { this._applyZoom(this._zoom - this.ZOOM_STEP); }
  zoomReset(): void { this._applyZoom(1.0); }

  private _animate(): void {
    const canvas = this._canvas;
    if (!canvas) return;
    if (!this._isDrag && this.isPlaying()) {
      this._angle += 0.005 + this._vel * 0.3;
      this._vel   *= 0.94;
    }
    this._draw(canvas);
    this._animId = requestAnimationFrame(() => this._animate());
  }

  private stopCanvas(): void {
    cancelAnimationFrame(this._animId);
    this._animId = 0;
  }

  private _draw(canvas: HTMLCanvasElement): void {
    const ctx = canvas.getContext('2d')!;
    const W   = canvas.clientWidth;
    const H   = canvas.clientHeight;
    const CX  = W / 2, CY = H / 2;
    const FOV = 520;
    const θ   = this._angle;

    ctx.clearRect(0, 0, W, H);

    // Subtle radial background
    const bg = ctx.createRadialGradient(CX, CY, 0, CX, CY, Math.max(W, H) * 0.6);
    bg.addColorStop(0, '#f0f4ff');
    bg.addColorStop(1, '#e8ecf4');
    ctx.fillStyle = bg;
    ctx.fillRect(0, 0, W, H);

    const nodes = this.netNodes();
    const edges = this.netEdges();

    type PNode = Net3DNode & { sx: number; sy: number; sz: number; sc: number; opa: number; r: number };

    const Z = this._zoom;

    const proj: PNode[] = nodes.map(n => {
      const rx =  n.ox * Math.cos(θ) + n.oz * Math.sin(θ);
      const rz = -n.ox * Math.sin(θ) + n.oz * Math.cos(θ);
      const ry =  n.oy;
      const sc  = FOV / (FOV + rz);
      return {
        ...n,
        sx: CX + rx * sc * Z,
        sy: CY + ry * sc * Z,
        sz: rz, sc,
        opa: Math.max(0.22, Math.min(1, (FOV * 0.9 - rz) / FOV)),
        r: n.baseR * Math.max(0.5, sc) * Z,
      };
    });

    const nodeMap = new Map(proj.map(p => [p.id, p]));

    // Draw edges
    edges.forEach(e => {
      const f = nodeMap.get(e.a), t = nodeMap.get(e.b);
      if (!f || !t) return;
      ctx.globalAlpha = Math.min(f.opa, t.opa) * 0.65;
      ctx.strokeStyle = e.color;
      ctx.lineWidth   = e.dashed ? 1.2 : 2;
      ctx.setLineDash(e.dashed ? [5, 5] : []);
      ctx.beginPath();
      ctx.moveTo(f.sx, f.sy);
      ctx.lineTo(t.sx, t.sy);
      ctx.stroke();
    });
    ctx.setLineDash([]);
    ctx.globalAlpha = 1;

    // Draw nodes (back → front)
    proj.sort((a, b) => b.sz - a.sz).forEach(n => {
      ctx.globalAlpha = n.opa;

      // Glow
      ctx.shadowBlur  = n.type === 'root' ? 24 : 14;
      ctx.shadowColor = n.color + 'aa';

      // Gradient fill
      const grad = ctx.createRadialGradient(n.sx - n.r * 0.28, n.sy - n.r * 0.28, n.r * 0.08, n.sx, n.sy, n.r);
      grad.addColorStop(0, this._lighten(n.color));
      grad.addColorStop(1, n.color);

      ctx.beginPath();
      ctx.arc(n.sx, n.sy, n.r, 0, Math.PI * 2);
      ctx.fillStyle = grad;
      ctx.fill();
      ctx.shadowBlur = 0;

      // White ring
      ctx.strokeStyle = 'rgba(255,255,255,0.85)';
      ctx.lineWidth   = Math.max(1.5, 2.5 * n.sc);
      ctx.stroke();

      // Second ring for root
      if (n.type === 'root') {
        ctx.beginPath();
        ctx.arc(n.sx, n.sy, n.r + 6 * n.sc, 0, Math.PI * 2);
        ctx.strokeStyle = n.color + '40';
        ctx.lineWidth   = 2;
        ctx.stroke();
      }

      // Avatar letter
      const fs = Math.max(9, n.r * 0.68);
      ctx.fillStyle = '#fff';
      ctx.font      = `800 ${fs}px 'Segoe UI', sans-serif`;
      ctx.textAlign    = 'center';
      ctx.textBaseline = 'middle';
      ctx.globalAlpha  = n.opa;
      ctx.fillText(n.label.charAt(0).toUpperCase(), n.sx, n.sy);

      // Name label (only when node is large/close enough)
      if (n.r > 14) {
        const lfs = Math.max(9, Math.min(12, 11 * n.sc));
        ctx.font      = `600 ${lfs}px 'Segoe UI', sans-serif`;
        ctx.fillStyle = '#333';
        ctx.globalAlpha = n.opa * 0.85;
        ctx.fillText(n.shortLabel, n.sx, n.sy + n.r + lfs * 0.9);
      }
      ctx.globalAlpha = 1;
    });
  }

  private _lighten(hex: string): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `rgb(${Math.min(255, r + 75)},${Math.min(255, g + 75)},${Math.min(255, b + 75)})`;
  }

  // ── Interaction ───────────────────────────────────────────────────────────
  selectNode(node: TreeNode): void {
    if (node.type === 'root') return;
    this.selectedNode.set(this.selectedNode()?.id === node.id ? null : node);
  }
  closeDetail(): void { this.selectedNode.set(null); }

  togglePlay(): void {
    this.isPlaying.set(!this.isPlaying());
    if (this.isPlaying() && this._canvas && !this._animId) this._animate();
  }

  permPercent(node: TreeNode): number {
    return node.totalPerms ? Math.round((node.permCount / node.totalPerms) * 100) : 0;
  }

  getRoleColor(name: string): string {
    const key = name.toLowerCase().replace(/\s+/g, '');
    return (PALETTES[key] ?? DEFAULT_PALETTE).color;
  }

  roleIcon(name: string): string {
    const n = name.toLowerCase().replace(/\s+/g, '');
    if (n === 'admin')            return 'admin_panel_settings';
    if (n === 'cashier')          return 'point_of_sale';
    if (n === 'waiter')           return 'room_service';
    if (n === 'inventorymanager') return 'inventory_2';
    return 'badge';
  }

  trackNode(_: number, n: TreeNode): string { return n.id; }
  trackEdge(_: number, e: TreeEdge): string { return e.id; }
}
