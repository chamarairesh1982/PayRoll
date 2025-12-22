import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
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

  menuItems: AppMenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'pi pi-home',
      routerLink: '/dashboard',
    },
    {
      label: 'People',
      icon: 'pi pi-users',
      items: [
        { label: 'Employees', icon: 'pi pi-id-card', routerLink: '/people/employees' },
      ],
    },
    {
      label: 'Time',
      icon: 'pi pi-clock',
      items: [
        { label: 'Attendance', icon: 'pi pi-calendar', routerLink: '/time/attendance' },
        { label: 'Overtime', icon: 'pi pi-stopwatch', disabled: true },
        { label: 'Leave', icon: 'pi pi-sun', disabled: true },
        { label: 'No-Pay', icon: 'pi pi-ban', disabled: true },
      ],
    },
    {
      label: 'Payroll',
      icon: 'pi pi-wallet',
      items: [
        { label: 'Pay Runs', icon: 'pi pi-calculator', routerLink: '/payroll/pay-runs' },
        { label: 'Payslips', icon: 'pi pi-file', routerLink: '/payroll/payslips' },
        { label: 'Pay Items', icon: 'pi pi-plus-circle', disabled: true },
        { label: 'Loans & Advances', icon: 'pi pi-wallet', disabled: true },
      ],
    },
    {
      label: 'Statutory',
      icon: 'pi pi-shield',
      items: [
        { label: 'EPF/ETF', icon: 'pi pi-bookmark', routerLink: '/statutory/epf-etf' },
        { label: 'PAYE/APIT', icon: 'pi pi-file-excel', disabled: true },
      ],
    },
    {
      label: 'Reports',
      icon: 'pi pi-chart-bar',
      items: [
        { label: 'Bank Transfer', icon: 'pi pi-credit-card', routerLink: '/reports/bank-transfer' },
        { label: 'Payroll Summary', icon: 'pi pi-chart-line', routerLink: '/reports/payroll-summary' },
        { label: 'Statutory Reports', icon: 'pi pi-file', routerLink: '/reports/statutory' },
      ],
    },
    {
      label: 'Settings',
      icon: 'pi pi-cog',
      items: [
        { label: 'Company', icon: 'pi pi-building', routerLink: '/settings/company' },
        { label: 'Pay Schedules', icon: 'pi pi-calendar-plus', routerLink: '/settings/pay-schedules' },
        { label: 'Pay Items', icon: 'pi pi-list', routerLink: '/settings/pay-items' },
        { label: 'Approval Flow', icon: 'pi pi-sitemap', routerLink: '/settings/approval-flow' },
        { label: 'Roles & Permissions', icon: 'pi pi-lock', routerLink: '/settings/roles-permissions' },
        { label: 'Integrations', icon: 'pi pi-link', routerLink: '/settings/integrations' },
      ],
    },
  ];

  constructor(
    private renderer: Renderer2,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.applyDensity(this.selectedDensity.value);
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
}
