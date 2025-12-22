import { Routes } from '@angular/router';
import { AppLayoutComponent } from './layout/app-layout.component';
import { LoginPageComponent } from './pages/login/login-page.component';

export const appRoutes: Routes = [
  { path: 'login', component: LoginPageComponent, data: { breadcrumb: 'Login' } },
  {
    path: '',
    component: AppLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard-page.component').then(m => m.DashboardPageComponent),
        data: { breadcrumb: 'Dashboard' },
      },
      {
        path: 'people/employees',
        loadComponent: () => import('./features/employees/employees-page.component').then(m => m.EmployeesPageComponent),
        data: { breadcrumb: 'Employees' },
      },
      {
        path: 'time/attendance',
        loadComponent: () => import('./features/attendance/attendance-page.component').then(m => m.AttendancePageComponent),
        data: { breadcrumb: 'Attendance' },
      },
      {
        path: 'payroll/pay-runs',
        loadComponent: () => import('./features/pay-runs/pay-runs-page.component').then(m => m.PayRunsPageComponent),
        data: { breadcrumb: 'Pay Runs' },
      },
      {
        path: 'payroll/payslips',
        loadComponent: () => import('./features/payslips/payslips-page.component').then(m => m.PayslipsPageComponent),
        data: { breadcrumb: 'Payslips' },
      },
      {
        path: 'statutory/epf-etf',
        loadComponent: () =>
          import('./features/statutory-epf-etf/statutory-epf-etf-page.component').then(
            m => m.StatutoryEpfEtfPageComponent,
          ),
        data: { breadcrumb: 'EPF/ETF' },
      },
      {
        path: 'reports/bank-transfer',
        loadComponent: () => import('./features/reports/reports-page.component').then(m => m.ReportsPageComponent),
        data: { breadcrumb: 'Bank Transfer', reportType: 'Bank Transfer' },
      },
      {
        path: 'reports/payroll-summary',
        loadComponent: () => import('./features/reports/reports-page.component').then(m => m.ReportsPageComponent),
        data: { breadcrumb: 'Payroll Summary', reportType: 'Payroll Summary' },
      },
      {
        path: 'reports/statutory',
        loadComponent: () => import('./features/reports/reports-page.component').then(m => m.ReportsPageComponent),
        data: { breadcrumb: 'Statutory Reports', reportType: 'Statutory Reports' },
      },
      {
        path: 'settings/company',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Company', settingType: 'Company' },
      },
      {
        path: 'settings/pay-schedules',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Pay Schedules', settingType: 'Pay Schedules' },
      },
      {
        path: 'settings/pay-items',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Pay Items', settingType: 'Pay Items' },
      },
      {
        path: 'settings/approval-flow',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Approval Flow', settingType: 'Approval Flow' },
      },
      {
        path: 'settings/roles-permissions',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Roles & Permissions', settingType: 'Roles & Permissions' },
      },
      {
        path: 'settings/integrations',
        loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent),
        data: { breadcrumb: 'Integrations', settingType: 'Integrations' },
      },
      { path: '**', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: '' },
];
