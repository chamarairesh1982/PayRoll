import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { PanelMenuModule } from 'primeng/panelmenu';
import { SidebarModule } from 'primeng/sidebar';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { filter } from 'rxjs/operators';

import { BreadcrumbComponent } from '../shared/components/breadcrumb/breadcrumb.component';
import { AppMenuItem } from '../shared/models/menu.model';

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
    BreadcrumbComponent,
  ],
  templateUrl: './app-layout.component.html',
  styleUrls: ['./app-layout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppLayoutComponent implements OnInit {
  sidebarVisible = false;
  densityOptions: DensityOption[] = [
    { label: 'Comfortable', value: 'comfortable' },
    { label: 'Compact', value: 'compact' },
  ];
  selectedDensity: DensityOption = this.densityOptions[0];
  companyOptions: TenantOption[] = [
    { label: 'WorldBets Holdings', value: 'worldbets' },
    { label: 'WorldBets Retail', value: 'worldbets-retail' },
  ];
  branchOptions: TenantOption[] = [
    { label: 'Colombo HQ', value: 'colombo-hq' },
    { label: 'Kandy Branch', value: 'kandy-branch' },
  ];
  costCenterOptions: TenantOption[] = [
    { label: 'Finance', value: 'finance' },
    { label: 'Operations', value: 'operations' },
  ];
  selectedCompany: TenantOption = this.companyOptions[0];
  selectedBranch: TenantOption = this.branchOptions[0];
  selectedCostCenter: TenantOption = this.costCenterOptions[0];
  userMenuItems: MenuItem[] = [
    { label: 'Profile', icon: 'pi pi-user' },
    { label: 'Notifications', icon: 'pi pi-bell' },
    { label: 'Sign Out', icon: 'pi pi-sign-out', command: () => this.signOut() },
  ];

  private readonly baseMenuItems: AppMenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'pi pi-home',
      routerLink: '/dashboard',
    },
    {
      label: 'People',
      icon: 'pi pi-users',
      items: [
        { label: 'Employees', icon: 'pi pi-id-card', routerLink: '/employees' },
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
      items: [
        { label: 'Pay Runs', icon: 'pi pi-calculator', routerLink: '/payroll' },
        { label: 'Payslips', icon: 'pi pi-file', disabled: true },
        { label: 'Recurring Pay Items', icon: 'pi pi-plus-circle', routerLink: '/payroll/recurring-rules' },
        { label: 'Loans & Advances', icon: 'pi pi-wallet', disabled: true },
      ],
    },
    {
      label: 'Statutory',
      icon: 'pi pi-shield',
      items: [
        { label: 'Statutory Reports', icon: 'pi pi-file', routerLink: '/statutory-reports' },
        { label: 'EPF/ETF Rules', icon: 'pi pi-bookmark', routerLink: '/config/epf-etf' },
        { label: 'PAYE/APIT', icon: 'pi pi-file-excel', routerLink: '/tax' },
      ],
    },
    {
      label: 'Reports',
      icon: 'pi pi-chart-bar',
      items: [
        { label: 'Reports', icon: 'pi pi-chart-line', routerLink: '/reports' },
        { label: 'Tax Reports', icon: 'pi pi-file-excel', routerLink: '/reports/tax' },
      ],
    },
    {
      label: 'Settings',
      icon: 'pi pi-cog',
      items: [
        { label: 'Allowances', icon: 'pi pi-list', routerLink: '/config/allowances' },
        { label: 'Banks', icon: 'pi pi-building', routerLink: '/config/banks' },
        { label: 'Bank Branches', icon: 'pi pi-sitemap', routerLink: '/config/bank-branches' },
        { label: 'Deductions', icon: 'pi pi-minus-circle', routerLink: '/config/deductions' },
        { label: 'Overtime Rules', icon: 'pi pi-clock', routerLink: '/config/overtime' },
        { label: 'Tax Rules', icon: 'pi pi-lock', routerLink: '/config/tax-rules' },
        { label: 'Approval Routes', icon: 'pi pi-share-alt', routerLink: '/approvals/config' },
        { label: 'Admin', icon: 'pi pi-user', routerLink: '/admin' },
      ],
    },
  ];
  menuItems: AppMenuItem[] = [];

  constructor(
    private renderer: Renderer2,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.applyDensity(this.selectedDensity.value);
    this.syncMenuState();
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      this.syncMenuState();
    });
  }

  toggleSidebar(): void {
    this.sidebarVisible = !this.sidebarVisible;
  }

  onDensityChange(option: DensityOption): void {
    this.applyDensity(option.value);
  }

  private applyDensity(value: DensityOption['value']): void {
    const body = document.body;
    this.renderer.removeClass(body, 'density-compact');
    if (value === 'compact') {
      this.renderer.addClass(body, 'density-compact');
    }
  }

  private signOut(): void {
    this.router.navigateByUrl('/login');
  }

  private syncMenuState(): void {
    this.menuItems = this.baseMenuItems.map(item => this.applyExpandedState(item));
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
}
