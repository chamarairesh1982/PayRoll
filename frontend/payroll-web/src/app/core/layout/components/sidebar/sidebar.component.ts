import { Component } from '@angular/core';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent {
  links = [
    { label: 'Dashboard', path: '/' },
    { label: 'Employees', path: '/employees' },
    { label: 'Payroll', path: '/payroll' },
    { label: 'Recurring Pay Items', path: '/payroll/recurring-rules' },
    { label: 'Attendance', path: '/attendance' },
    { label: 'Leave', path: '/leave' },
    { label: 'Overtime', path: '/overtime' },
    { label: 'Overtime Approvals', path: '/overtime/approvals' },
    { label: 'Approvals Inbox', path: '/approvals/inbox' },
    { label: 'Reports', path: '/reports' },
    { label: 'Tax Reports', path: '/reports/tax' },
    { label: 'Statutory Reports', path: '/statutory-reports' },
  ];

  configLinks = [
    { label: 'Allowance Types', path: '/config/allowances' },
    { label: 'Banks', path: '/config/banks' },
    { label: 'Bank Branches', path: '/config/bank-branches' },
    { label: 'Deduction Types', path: '/config/deductions' },
    { label: 'EPF/ETF Rules', path: '/config/epf-etf' },
    { label: 'Overtime Rules', path: '/config/overtime' },
    { label: 'Tax Configuration', path: '/config/tax-rules' },
    { label: 'Approval Routes', path: '/approvals/config' },
  ];

  adminLinks = [{ label: 'Audit Logs', path: '/admin/audit-logs' }];

  constructor(private authService: AuthService) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }
}
