import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';

export type PaletteNodeKind = 'action' | 'delay' | 'decision' | 'notification' | 'end';

interface PaletteCard {
  kind: PaletteNodeKind;
  icon: string;
  title: string;
  subtitle: string;
}

@Component({
  selector: 'app-node-palette',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  templateUrl: './node-palette.component.html',
  styleUrl: './node-palette.component.scss'
})
export class NodePaletteComponent {
  triggerCard = { icon: 'pi-bolt', title: 'Trigger', subtitle: 'Initiate workflows' };

  cards: PaletteCard[] = [
    { kind: 'action', icon: 'pi-play-circle', title: 'Action', subtitle: 'Perform actions based on triggers' },
    { kind: 'delay', icon: 'pi-clock', title: 'Delay', subtitle: 'Pause the workflow' },
    { kind: 'decision', icon: 'pi-sitemap', title: 'Decision', subtitle: 'Route the workflow' },
    { kind: 'notification', icon: 'pi-bell', title: 'Notification', subtitle: 'Send alerts or notifications' },
    { kind: 'end', icon: 'pi-flag', title: 'End', subtitle: 'Terminate the workflow' }
  ];

  onDragStart(event: DragEvent, kind: PaletteNodeKind) {
    event.dataTransfer?.setData('text/kind', kind);
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'copy';
  }
}
