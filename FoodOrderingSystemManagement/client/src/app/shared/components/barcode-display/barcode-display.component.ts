import {
  Component, Input, ViewChild, ElementRef,
  AfterViewInit, OnChanges, SimpleChanges,
} from '@angular/core';
import JsBarcode from 'jsbarcode';

@Component({
  selector: 'app-barcode-display',
  standalone: true,
  template: `
    <div class="barcode-wrap">
      <svg #svg></svg>
      @if (!value) {
        <span class="no-barcode">No barcode assigned</span>
      }
    </div>
  `,
  styles: [`
    .barcode-wrap { display: flex; flex-direction: column; align-items: center; }
    .no-barcode   { font-size: .8rem; color: #9e9e9e; font-style: italic; }
    svg           { max-width: 100%; }
  `],
})
export class BarcodeDisplayComponent implements AfterViewInit, OnChanges {
  @Input() value  = '';
  @Input() format = 'CODE128';
  @Input() width  = 2;
  @Input() height = 70;
  @Input() displayValue = true;
  @Input() fontSize = 14;

  @ViewChild('svg') svgRef!: ElementRef<SVGElement>;

  private initialized = false;

  ngAfterViewInit(): void {
    this.initialized = true;
    this.render();
  }

  ngOnChanges(_: SimpleChanges): void {
    if (this.initialized) this.render();
  }

  private render(): void {
    if (!this.value || !this.svgRef?.nativeElement) return;
    try {
      JsBarcode(this.svgRef.nativeElement, this.value, {
        format:       this.format,
        width:        this.width,
        height:       this.height,
        displayValue: this.displayValue,
        fontSize:     this.fontSize,
        margin:       8,
      });
    } catch {
      // Invalid barcode value for the chosen format — clear the SVG silently
    }
  }
}
