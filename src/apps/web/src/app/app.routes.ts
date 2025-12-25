import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
    },
    {
        path: 'employees',
        canActivate: [authGuard],
        loadChildren: () => import('./features/employees/employees.module').then(m => m.EmployeesModule)
    },
    {
        path: 'payrolls',
        canActivate: [authGuard],
        loadChildren: () => import('./features/payrolls/payrolls.module').then(m => m.PayrollsModule)
    },
    { path: '**', redirectTo: 'employees' }
];
