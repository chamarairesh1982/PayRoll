import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, PRIMARY_OUTLET, Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { filter, startWith, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, BreadcrumbModule],
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss'],
})
export class BreadcrumbComponent implements OnInit, OnDestroy {
  items: MenuItem[] = [];
  home: MenuItem = { icon: 'pi pi-home', routerLink: '/dashboard' };
  private destroy$ = new Subject<void>();

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.router.events
      .pipe(
        startWith(new NavigationEnd(0, this.router.url, this.router.url)),
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe(() => {
        const breadcrumbs = this.buildBreadcrumbs(this.route.root);
        const deduped = breadcrumbs.filter((item, index) => {
          const next = breadcrumbs[index + 1];
          if (next?.routerLink && item.routerLink && item.routerLink === next.routerLink) {
            return false;
          }
          return true;
        });
        const lastIndex = deduped.length - 1;
        if (lastIndex >= 0) {
          deduped[lastIndex] = { ...deduped[lastIndex], routerLink: undefined };
        }
        this.items = deduped;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildBreadcrumbs(route: ActivatedRoute): MenuItem[] {
    const breadcrumbs: MenuItem[] = [];
    const snapshots = route.snapshot.pathFromRoot.filter(snapshot => snapshot.outlet === PRIMARY_OUTLET);
    let nextUrl = '';
    for (const snapshot of snapshots) {
      const routeURL = snapshot.url.map(segment => segment.path).join('/');
      if (routeURL) {
        nextUrl += `/${routeURL}`;
      }
      const label = snapshot.data['breadcrumb'] as string | undefined;
      if (label) {
        const routerLink = nextUrl || undefined;
        breadcrumbs.push({ label, routerLink });
      }
    }

    return breadcrumbs;
  }
}
