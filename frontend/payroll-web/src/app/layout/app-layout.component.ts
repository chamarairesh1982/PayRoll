import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  Renderer2,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule, DropdownChangeEvent } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { PanelMenuModule } from 'primeng/panelmenu';
import { SidebarModule } from 'primeng/sidebar';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

import { BreadcrumbComponent } from '../shared/components/breadcrumb/breadcrumb.component';
import { AppMenuItem } from '../shared/models/menu.model';
import { AuthService } from '../core/services/auth.service';

interface DensityOption {
  label: string;
  value: 'comfortable' | 'compact';
}

interface TenantOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    PanelMenuModule,
    SidebarModule,
    ToolbarModule,
    InputTextModule,
    DropdownModule,
    ButtonModule,
    MenuModule,
    ToastModule,
    ConfirmDialogModule,
    AvatarModule,
    BreadcrumbComponent,
  ],
  templateUrl: './app-layout.component.html',
  styleUrls: ['./app-layout.component.scss'],
  // changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppLayoutComponent implements OnInit, OnDestroy {
  public sidebarVisible = false;
  public densityOptions: DensityOption[] = [
    { label: 'Comfortable', value: 'comfortable' },
    { label: 'Compact', value: 'compact' },
  ];
  public selectedDensity: DensityOption = this.densityOptions[0];
  public companyOptions: TenantOption[] = [
    { label: 'WorldBets Holdings', value: 'worldbets' },
    { label: 'WorldBets Retail', value: 'worldbets-retail' },
  ];
  public branchOptions: TenantOption[] = [
    { label: 'Colombo HQ', value: 'colombo-hq' },
    { label: 'Kandy Branch', value: 'kandy-branch' },
  ];
  public costCenterOptions: TenantOption[] = [
    { label: 'Finance', value: 'finance' },
    { label: 'Operations', value: 'operations' },
  ];
  public selectedCompany: TenantOption = this.companyOptions[0];
  public selectedBranch: TenantOption = this.branchOptions[0];
  public selectedCostCenter: TenantOption = this.costCenterOptions[0];
  public userMenuItems: MenuItem[] = [
    { label: 'Profile', icon: 'pi pi-user' },
    { label: 'Notifications', icon: 'pi pi-bell' },
    { label: 'Sign Out', icon: 'pi pi-sign-out', command: () => this.signOut() },
  ];
  public userName = 'User';
  public userRole = 'Employee';

  private readonly destroy$ = new Subject<void>();
  private userRoles: string[] = [];

  private readonly baseMenuItems: AppMenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'pi pi-home',
      routerLink: '/dashboard',
      roles: ['admin', 'approver', 'employee'],
    },
    {
      label: 'People',
      icon: 'pi pi-users',
      roles: ['admin', 'approver'],
      items: [
        { label: 'Employees', icon: 'pi pi-id-card', routerLink: '/employees', roles: ['admin', 'approver'] },
      ],
    },
    {
      label: 'Time',
      icon: 'pi pi-clock',
      items: [
        { label: 'Attendance', icon: 'pi pi-calendar', routerLink: '/attendance' },
        { label: 'Overtime', icon: 'pi pi-stopwatch', routerLink: '/overtime' },
        { label: 'Leave', icon: 'pi pi-sun', routerLink: '/leave' },
        { label: 'No-Pay', icon: 'pi pi-ban', disabled: true },
      ],
    },
    {
      label: 'Payroll',
      icon: 'pi pi-wallet',
      roles: ['admin', 'approver'],
      items: [
        { label: 'Pay Runs', icon: 'pi pi-calculator', routerLink: '/payroll', roles: ['admin', 'approver'] },
        {
          label: 'Recurring Pay Items',
          icon: 'pi pi-plus-circle',
          routerLink: '/payroll/recurring-rules',
          roles: ['admin', 'approver'],
        },
        { label: 'Payroll Insights', icon: 'pi pi-chart-line', disabled: true, roles: ['admin', 'approver'] },
      ],
    },
    {
      label: 'Statutory',
      icon: 'pi pi-shield',
      roles: ['admin', 'approver'],
      items: [
        { label: 'Statutory Reports', icon: 'pi pi-file', routerLink: '/statutory-reports', roles: ['admin', 'approver'] },
        { label: 'EPF/ETF Rules', icon: 'pi pi-bookmark', routerLink: '/config/epf-etf', roles: ['admin', 'approver'] },
        { label: 'PAYE/APIT', icon: 'pi pi-file-excel', routerLink: '/tax', roles: ['admin', 'approver'] },
      ],
    },
    {
      label: 'Reports',
      icon: 'pi pi-chart-bar',
      roles: ['admin', 'approver'],
      items: [
        { label: 'Operational Reports', icon: 'pi pi-chart-line', routerLink: '/reports', roles: ['admin', 'approver'] },
        { label: 'Tax Reports', icon: 'pi pi-file-excel', routerLink: '/reports/tax', roles: ['admin', 'approver'] },
      ],
    },
    {
      label: 'Configuration',
      icon: 'pi pi-cog',
      roles: ['admin'],
      items: [
        { label: 'Allowances', icon: 'pi pi-list', routerLink: '/config/allowances', roles: ['admin'] },
        { label: 'Banks', icon: 'pi pi-building', routerLink: '/config/banks', roles: ['admin'] },
        { label: 'Bank Branches', icon: 'pi pi-sitemap', routerLink: '/config/bank-branches', roles: ['admin'] },
        { label: 'Deductions', icon: 'pi pi-minus-circle', routerLink: '/config/deductions', roles: ['admin'] },
        { label: 'Overtime Rules', icon: 'pi pi-clock', routerLink: '/config/overtime', roles: ['admin'] },
        { label: 'Tax Rules', icon: 'pi pi-lock', routerLink: '/config/tax-rules', roles: ['admin'] },
        { label: 'Approval Routes', icon: 'pi pi-share-alt', routerLink: '/approvals/config', roles: ['admin'] },
      ],
    },
    {
      label: 'Administration',
      icon: 'pi pi-briefcase',
      roles: ['admin'],
      items: [
        { label: 'Audit Logs', icon: 'pi pi-history', routerLink: '/admin/audit-logs', roles: ['admin'] },
        { label: 'Rule Versions', icon: 'pi pi-book', routerLink: '/admin/rule-versions', roles: ['admin'] },
        { label: 'General Ledger', icon: 'pi pi-table', routerLink: '/admin/general-ledger', roles: ['admin'] },
      ],
    },
    {
      label: 'My Workspace',
      icon: 'pi pi-user',
      roles: ['employee'],
      items: [
        { label: 'My Payslips', icon: 'pi pi-file', routerLink: '/payroll', roles: ['employee'] },
        { label: 'My Requests', icon: 'pi pi-inbox', routerLink: '/approvals/inbox', roles: ['employee'] },
      ],
    },
  ];
  menuItems: AppMenuItem[] = [];

  constructor(
    private renderer: Renderer2,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
  ) { }

  ngOnInit(): void {
    this.userRoles = this.authService.getRoles().map(role => role.toLowerCase());
    if (!this.userRoles.length) {
      this.userRoles = ['employee'];
    }
    this.userName = this.authService.getUserName() ?? 'User';
    this.userRole = this.formatRoleLabel(this.userRoles[0]);
    this.applyDensity(this.selectedDensity.value);
    this.syncMenuState();
    this.cdr.markForCheck();
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe(() => {
        this.syncMenuState();
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleSidebar(): void {
    this.sidebarVisible = !this.sidebarVisible;
  }

  onMenuSelect(): void {
    this.sidebarVisible = false;
  }

  onDensityChange(option: DensityOption): void {
    this.applyDensity(option.value);
  }

  get selectedCompanyLabel(): string {
    return this.selectedCompany?.label ?? 'Select company';
  }

  onCompanyChange(event: DropdownChangeEvent): void {
    this.selectedCompany = (event.value as TenantOption) ?? this.selectedCompany;
    this.cdr.markForCheck();
  }

  private applyDensity(value: DensityOption['value']): void {
    const body = document.body;
    this.renderer.removeClass(body, 'density-compact');
    if (value === 'compact') {
      this.renderer.addClass(body, 'density-compact');
    }
  }

  private signOut(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  private syncMenuState(): void {
    const filteredMenu = this.filterByRole(this.baseMenuItems, this.userRoles);
    this.menuItems = filteredMenu.map(item => this.applyExpandedState(item));
  }

  private applyExpandedState(item: AppMenuItem): AppMenuItem {
    const items = item.items?.map(child => this.applyExpandedState(child));
    const expanded = Boolean(items?.some(child => child.expanded || this.isItemActive(child)));
    return {
      ...item,
      items,
      expanded,
    };
  }

  private isItemActive(item: AppMenuItem): boolean {
    if (!item.routerLink) {
      return false;
    }
    const commands = Array.isArray(item.routerLink) ? item.routerLink : [item.routerLink];
    return this.router.isActive(this.router.createUrlTree(commands), {
      paths: 'exact',
      queryParams: 'ignored',
      fragment: 'ignored',
      matrixParams: 'ignored',
    });
  }

  private filterByRole(items: AppMenuItem[], roles: string[]): AppMenuItem[] {
    const roleSet = new Set(roles.map(role => role.toLowerCase()));

    return items
      .map(item => {
        const children = item.items ? this.filterByRole(item.items, roles) : undefined;
        const normalizedRoles = item.roles?.map(role => role.toLowerCase());
        const allowed = !normalizedRoles || normalizedRoles.some(role => roleSet.has(role));
        const visible = allowed || Boolean(children?.length);

        return {
          ...item,
          items: children,
          visible,
        } satisfies AppMenuItem;
      })
      .filter(item => item.visible);
  }

  private formatRoleLabel(role?: string): string {
    if (!role) {
      return 'User';
    }
    return role.charAt(0).toUpperCase() + role.slice(1);
  }
}
