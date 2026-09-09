import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../../core/notification.service';
import { VehicleApiService, VehicleDto } from '../../../core/vehicle-api.service';
import {
  DeliveryLoadApiService, DeliveryLoadDto, DeliveryLoadShipmentDto
} from '../../../core/delivery-load-api.service';

@Component({
  selector: 'app-delivery',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ButtonModule, TagModule, ToastModule, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './delivery.component.html',
  styleUrl: './delivery.component.scss'
})
export class DeliveryComponent implements OnInit {
  vehicles: VehicleDto[] = [];
  loadingVehicles = false;

  selectedVehicle: VehicleDto | null = null;
  currentLoad: DeliveryLoadDto | null = null;
  availableShipments: DeliveryLoadShipmentDto[] = [];
  loadingPanel = false;
  dispatching = false;

  constructor(
    private vehicleApi: VehicleApiService,
    private loadApi: DeliveryLoadApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles() {
    this.loadingVehicles = true;
    this.vehicleApi.getAll().subscribe({
      next: rows => {
        this.vehicles = rows.filter(v => v.isActive);
        this.loadingVehicles = false;
        if (!this.selectedVehicle && this.vehicles.length) {
          this.selectVehicle(this.vehicles[0]);
        }
      },
      error: err => {
        this.loadingVehicles = false;
        this.notify.error(err.error?.message ?? 'Failed to load vehicles.');
      }
    });
  }

  selectVehicle(vehicle: VehicleDto) {
    this.selectedVehicle = vehicle;
    this.currentLoad = null;
    this.availableShipments = [];

    if (!vehicle.currentWarehouseId) {
      this.notify.warn(`${vehicle.code} has no current location set — assign one in Vehicles first.`);
      return;
    }

    this.loadingPanel = true;
    this.loadApi.create({ vehicleId: vehicle.id, warehouseId: vehicle.currentWarehouseId }).subscribe({
      next: load => {
        this.currentLoad = load;
        this.refreshAvailableShipments(vehicle.currentWarehouseId!);
      },
      error: err => {
        this.loadingPanel = false;
        this.notify.error(err.error?.message ?? 'Failed to open a delivery load for this vehicle.');
      }
    });
  }

  private refreshAvailableShipments(warehouseId: number) {
    this.loadApi.getAvailableShipments(warehouseId).subscribe({
      next: rows => {
        this.availableShipments = rows;
        this.loadingPanel = false;
      },
      error: err => {
        this.loadingPanel = false;
        this.notify.error(err.error?.message ?? 'Failed to load available shipments.');
      }
    });
  }

  isAssigned(shipmentId: number): boolean {
    return !!this.currentLoad?.shipments.some(s => s.shipmentId === shipmentId);
  }

  toggleShipment(shipmentId: number, checked: boolean) {
    if (!this.currentLoad) return;
    const req$ = checked
      ? this.loadApi.assignShipment(this.currentLoad.id, shipmentId)
      : this.loadApi.unassignShipment(this.currentLoad.id, shipmentId);

    req$.subscribe({
      next: load => {
        this.currentLoad = load;
        this.refreshAvailableShipments(this.selectedVehicle!.currentWarehouseId!);
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to update the load.')
    });
  }

  /// Rows shown = every candidate shipment at this warehouse, whether or not it's already on
  /// the current load — grouped by order/transfer reference, mirroring the mockup's per-order
  /// grouping even though we assign at the shipment level rather than individual line items.
  get combinedRows(): (DeliveryLoadShipmentDto & { assigned: boolean })[] {
    const assignedIds = new Set(this.currentLoad?.shipments.map(s => s.shipmentId) ?? []);
    const assignedRows = this.currentLoad?.shipments.map(s => ({ ...s, assigned: true })) ?? [];
    const availableRows = this.availableShipments
      .filter(s => !assignedIds.has(s.shipmentId))
      .map(s => ({ ...s, assigned: false }));
    return [...assignedRows, ...availableRows];
  }

  get groupedRows(): { label: string; rows: (DeliveryLoadShipmentDto & { assigned: boolean })[] }[] {
    const groups = new Map<string, (DeliveryLoadShipmentDto & { assigned: boolean })[]>();
    for (const row of this.combinedRows) {
      const label = row.referenceLabel ?? 'Unassigned';
      if (!groups.has(label)) groups.set(label, []);
      groups.get(label)!.push(row);
    }
    return Array.from(groups.entries()).map(([label, rows]) => ({ label, rows }));
  }

  get loadedPercent(): number {
    if (!this.currentLoad || this.currentLoad.vehicleCapacityKg === 0) return 0;
    return Math.min(100, Math.round((this.currentLoad.totalWeightKg / this.currentLoad.vehicleCapacityKg) * 100));
  }

  dispatch() {
    if (!this.currentLoad) return;
    this.confirm.confirm({
      message: `Dispatch ${this.selectedVehicle?.code} with ${this.currentLoad.shipments.length} shipment(s)? This will mark them Shipped and deduct stock.`,
      header: 'Confirm Dispatch',
      icon: 'pi pi-send',
      accept: () => {
        this.dispatching = true;
        this.loadApi.dispatch(this.currentLoad!.id).subscribe({
          next: () => {
            this.dispatching = false;
            this.notify.success(`${this.selectedVehicle?.code} dispatched.`);
            this.loadVehicles();
          },
          error: err => {
            this.dispatching = false;
            this.notify.error(err.error?.message ?? 'Failed to dispatch this load.');
          }
        });
      }
    });
  }

  /// Real photo per load condition, not per vehicle type. FULL_LOAD_THRESHOLD is "close enough
  /// to capacity to call it full" rather than requiring an exact 100% match.
  private static readonly FULL_LOAD_THRESHOLD = 95;
  private static readonly IMAGE_BY_STATE: Record<string, string> = {
    empty: '/images/vehicle-empty.png',
    loading: '/images/vehicle-loading.png',
    full: '/images/vehicle-full.png',
    transit: '/images/vehicle-transit.png'
  };

  /// The selected vehicle's condition is computed from its real current load — used for the
  /// large hero panel and the selected card's thumbnail, both of which have full load data.
  get selectedVehicleState(): 'empty' | 'loading' | 'full' | 'transit' {
    if (this.selectedVehicle?.status === 'InTransit') return 'transit';
    if (!this.currentLoad || !this.currentLoad.shipments.length) return 'empty';
    return this.loadedPercent >= DeliveryComponent.FULL_LOAD_THRESHOLD ? 'full' : 'loading';
  }

  /// Other vehicles in the list don't have their load fetched (would be an API call per row),
  /// so they only distinguish In Transit vs Parked/Available using the vehicle's own status.
  listRowState(v: VehicleDto): 'empty' | 'transit' {
    return v.status === 'InTransit' ? 'transit' : 'empty';
  }

  imageForState(state: 'empty' | 'loading' | 'full' | 'transit'): string {
    return DeliveryComponent.IMAGE_BY_STATE[state];
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Available') return 'success';
    if (status === 'InTransit') return 'warn';
    if (status === 'Maintenance') return 'danger';
    return 'info';
  }
}
