import { Component, signal } from '@angular/core';
import { ActivatedRouteSnapshot, NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, startWith } from 'rxjs';

interface Crumb {
  label: string;
  url: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [RouterLink],
  template: `
    <nav class="breadcrumb">
      <a routerLink="/medicines">Home</a>
      @for (crumb of crumbs(); track crumb.url) {
        <span class="sep">/</span>
        <a [routerLink]="crumb.url">{{ crumb.label }}</a>
      }
    </nav>
  `,
  styles: [`
    .breadcrumb { display: flex; align-items: center; gap: 6px; font-size: 0.85rem; }
    .breadcrumb a { color: inherit; text-decoration: none; opacity: 0.85; }
    .breadcrumb a:last-child { opacity: 1; font-weight: 600; }
    .sep { opacity: 0.5; }
  `]
})
export class BreadcrumbComponent {
  crumbs = signal<Crumb[]>([]);

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd), startWith(null))
      .subscribe(() => this.crumbs.set(this.buildCrumbs(this.router.routerState.snapshot.root)));
  }

  private buildCrumbs(snapshot: ActivatedRouteSnapshot, url = '', crumbs: Crumb[] = []): Crumb[] {
    const child = snapshot.children[0];
    if (!child) return crumbs;

    const segment = child.url.map(s => s.path).join('/');
    const nextUrl = segment ? `${url}/${segment}` : url;
    const label = child.data['breadcrumb'];

    if (label) crumbs.push({ label, url: nextUrl });
    return this.buildCrumbs(child, nextUrl, crumbs);
  }
}
