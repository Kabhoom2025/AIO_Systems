import { AfterViewChecked, Directive, ElementRef, Renderer2 } from '@angular/core';

/**
 * Scans the host's rendered markdown for <pre> code blocks and injects a floating
 * "Copy" button on each one. Re-runs after every view check but is a no-op once a
 * block already has its button, so it stays cheap on unrelated change-detection ticks.
 */
@Directive({
  selector: '[appCodeCopy]',
  standalone: true
})
export class CodeCopyDirective implements AfterViewChecked {
  constructor(
    private readonly host: ElementRef<HTMLElement>,
    private readonly renderer: Renderer2
  ) {}

  ngAfterViewChecked(): void {
    const blocks = this.host.nativeElement.querySelectorAll('pre:not([data-copy-ready])');
    blocks.forEach((block) => this.decorate(block as HTMLElement));
  }

  private decorate(pre: HTMLElement): void {
    pre.setAttribute('data-copy-ready', 'true');
    pre.style.position = 'relative';

    const button = this.renderer.createElement('button') as HTMLButtonElement;
    button.type = 'button';
    button.className = 'code-copy-btn';
    button.setAttribute('aria-label', 'Copy code');
    button.textContent = 'Copy';

    this.renderer.listen(button, 'click', () => {
      const code = pre.querySelector('code')?.textContent ?? pre.textContent ?? '';
      navigator.clipboard.writeText(code).then(() => {
        button.textContent = 'Copied!';
        setTimeout(() => (button.textContent = 'Copy'), 1500);
      });
    });

    this.renderer.appendChild(pre, button);
  }
}
