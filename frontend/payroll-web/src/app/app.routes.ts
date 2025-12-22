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
        path: 'employees',
        loadChildren: () => import('./features/employees/employees.module').then(m => m.EmployeesModule),
        data: { breadcrumb: 'Employees' },
      },
      {
        path: 'attendance',
        loadChildren: () => import('./features/attendance/attendance.module').then(m => m.AttendanceModule),
        data: { breadcrumb: 'Attendance' },
      },
      {
        path: 'payroll',
        loadChildren: () => import('./features/payroll/payroll.module').then(m => m.PayrollModule),
        data: { breadcrumb: 'Payroll' },
      },
      {
        path: 'reports',
        loadChildren: () => import('./features/reports/reports.module').then(m => m.ReportsModule),
        data: { breadcrumb: 'Reports' },
      },
      {
        path: 'statutory-reports',
        loadChildren: () =>
          import('./features/statutory-reports/statutory-reports.module').then(m => m.StatutoryReportsModule),
        data: { breadcrumb: 'Statutory Reports' },
      },
      {
        path: 'config',
        loadChildren: () => import('./features/payroll-config/payroll-config.module').then(m => m.PayrollConfigModule),
        data: { breadcrumb: 'Configuration' },
      },
      {
        path: 'approvals',
        loadChildren: () => import('./features/approvals/approvals.module').then(m => m.ApprovalsModule),
        data: { breadcrumb: 'Approvals' },
      },
      {
        path: 'overtime',
        loadChildren: () => import('./features/overtime/overtime.module').then(m => m.OvertimeModule),
        data: { breadcrumb: 'Overtime' },
      },
      {
        path: 'leave',
        loadChildren: () => import('./features/leave/leave.module').then(m => m.LeaveModule),
        data: { breadcrumb: 'Leave' },
      },
      {
        path: 'tax',
        loadChildren: () => import('./features/tax/tax.module').then(m => m.TaxModule),
        data: { breadcrumb: 'Tax' },
      },
      {
        path: 'admin',
        loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
        data: { breadcrumb: 'Admin' },
      },
      { path: '**', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: '' },
];
