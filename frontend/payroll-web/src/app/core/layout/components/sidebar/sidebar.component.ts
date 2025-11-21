import { Component } from '@angular/core';

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
    { label: 'Attendance', path: '/attendance' },
    { label: 'Leave', path: '/leave' },
    { label: 'Overtime', path: '/overtime' },
    { label: 'Reports', path: '/reports' },
  ];

  configLinks = [
    { label: 'Allowance Types', path: '/config/allowances' },
    { label: 'Deduction Types', path: '/config/deductions' },
    { label: 'EPF/ETF Rules', path: '/config/epf-etf' },
    { label: 'PAYE Tax Rules', path: '/config/tax-rules' },
  ];
}
