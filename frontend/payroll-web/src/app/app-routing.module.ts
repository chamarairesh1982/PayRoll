import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';
import { AuthGuard } from './core/guards/auth.guard';
import { ApproverGuard } from './core/guards/approver.guard';
import { MainLayoutComponent } from './core/layout/main-layout/main-layout.component';
import { LoginPageComponent } from './pages/login/login-page.component';

const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'employees', pathMatch: 'full' },
      {
        path: 'employees',
        loadChildren: () => import('./features/employees/employees.module').then(m => m.EmployeesModule),
      },
      {
        path: 'attendance',
        loadChildren: () => import('./features/attendance/attendance.module').then(m => m.AttendanceModule),
      },
      {
        path: 'leave',
        loadChildren: () => import('./features/leave/leave.module').then(m => m.LeaveModule),
      },
      {
        path: 'overtime',
        loadChildren: () => import('./features/overtime/overtime.module').then(m => m.OvertimeModule),
      },
      {
        path: 'payroll',
        loadChildren: () => import('./features/payroll/payroll.module').then(m => m.PayrollModule),
      },
      {
        path: 'tax',
        loadChildren: () => import('./features/tax/tax.module').then(m => m.TaxModule),
      },
      {
        path: 'config',
        loadChildren: () =>
          import('./features/payroll-config/payroll-config.module').then(m => m.PayrollConfigModule),
        canActivate: [AdminGuard],
      },
      {
        path: 'admin',
        loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
        canActivate: [AdminGuard],
      },
      {
        path: 'reports',
        loadChildren: () => import('./features/reports/reports.module').then(m => m.ReportsModule),
      },
      {
        path: 'statutory-reports',
        loadChildren: () =>
          import('./features/statutory-reports/statutory-reports.module').then(m => m.StatutoryReportsModule),
      },
      {
        path: 'approvals',
        loadChildren: () => import('./features/approvals/approvals.module').then(m => m.ApprovalsModule),
        canActivate: [ApproverGuard],
      },
    ],
  },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
