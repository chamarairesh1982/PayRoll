import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, PRIMARY_OUTLET, Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { filter, Subject, takeUntil } from 'rxjs';

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

  private buildBreadcrumbs(route: ActivatedRoute, url: string = '', breadcrumbs: MenuItem[] = []): MenuItem[] {
    const primaryChild = route.children.find(child => child.outlet === PRIMARY_OUTLET);
    if (!primaryChild) {
      return breadcrumbs;
    }

    const routeURL = primaryChild.snapshot.url.map(segment => segment.path).join('/');
    const nextUrl = routeURL ? `${url}/${routeURL}` : url;
    const label = primaryChild.snapshot.data['breadcrumb'] as string | undefined;
    if (label) {
      const routerLink = routeURL ? nextUrl : undefined;
      breadcrumbs.push({ label, routerLink });
    }

    return this.buildBreadcrumbs(primaryChild, nextUrl, breadcrumbs);
  }
}
