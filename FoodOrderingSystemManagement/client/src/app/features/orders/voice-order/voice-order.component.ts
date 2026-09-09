import {
  Component, EventEmitter, Input, OnDestroy, OnInit, Output, signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface VoiceMenuItem {
  id: number;
  itemName: string;
  price: number;
  image?: string | null;
  categoryName?: string;
}

@Component({
  selector: 'app-voice-order',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './voice-order.component.html',
  styleUrl: './voice-order.component.scss',
})
export class VoiceOrderComponent implements OnInit, OnDestroy {
  @Input() menuItems: VoiceMenuItem[] = [];
  @Output() itemSelected = new EventEmitter<VoiceMenuItem>();

  supported  = signal(false);
  listening  = signal(false);
  transcript = signal('');
  matches    = signal<VoiceMenuItem[]>([]);
  error      = signal<string | null>(null);

  private recognition: any = null;

  ngOnInit(): void {
    const SR =
      (window as any).SpeechRecognition ||
      (window as any).webkitSpeechRecognition;

    if (!SR) {
      this.error.set('Voice ordering requires Chrome or Edge browser.');
      return;
    }

    this.supported.set(true);
    this.recognition         = new SR();
    this.recognition.continuous      = false;
    this.recognition.interimResults  = true;
    this.recognition.lang            = 'en-US';

    this.recognition.onresult = (event: any) => {
      const text = Array.from(event.results as any[])
        .map((r: any) => r[0].transcript)
        .join(' ');
      this.transcript.set(text);
      this.findMatches(text);
    };

    this.recognition.onend   = () => this.listening.set(false);
    this.recognition.onerror = (e: any) => {
      this.error.set(`Voice error: ${e.error}`);
      this.listening.set(false);
    };
  }

  ngOnDestroy(): void {
    if (this.recognition && this.listening()) this.recognition.stop();
  }

  toggle(): void {
    if (!this.supported()) return;
    if (this.listening()) {
      this.recognition.stop();
      this.listening.set(false);
    } else {
      this.transcript.set('');
      this.matches.set([]);
      this.error.set(null);
      this.recognition.start();
      this.listening.set(true);
    }
  }

  select(item: VoiceMenuItem): void {
    this.itemSelected.emit(item);
    this.matches.set([]);
    this.transcript.set('');
  }

  private findMatches(text: string): void {
    const words = text.toLowerCase().split(/\s+/).filter(w => w.length > 2);
    const results = this.menuItems
      .map(item => ({
        item,
        hits: words.filter(w => item.itemName.toLowerCase().includes(w)).length,
      }))
      .filter(x => x.hits > 0)
      .sort((a, b) => b.hits - a.hits)
      .slice(0, 5)
      .map(x => x.item);
    this.matches.set(results);
  }
}
