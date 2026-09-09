import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ScanStatus } from '../models/scan.models';

export interface ScanStageEvent {
  scanId: string;
  stage: ScanStatus;
  message: string;
}

/**
 * One hub connection per component lifetime (created in the scan-detail page, destroyed on
 * navigation away) rather than a single app-wide singleton — a scan-detail page only ever
 * cares about one scan's group at a time.
 */
@Injectable()
export class ScanSignalrService {
  private connection: signalR.HubConnection | null = null;
  private readonly stageChanged$ = new Subject<ScanStageEvent>();

  readonly onStageChanged = this.stageChanged$.asObservable();

  async connectAndJoin(scanId: string): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.scanHubUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('scanStageChanged', (event: ScanStageEvent) => this.stageChanged$.next(event));

    try {
      await this.connection.start();
      await this.connection.invoke('JoinScanGroup', scanId);
    } catch {
      // Live updates are a convenience, not a requirement — the scan-detail page still works
      // by polling GET /api/v1/scans/{id} if the hub connection fails.
    }
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
  }
}
