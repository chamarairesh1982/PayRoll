import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
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
        path: 'config',
        loadChildren: () =>
          import('./features/payroll-config/payroll-config.module').then(m => m.PayrollConfigModule),
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
