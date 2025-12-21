import { Component } from '@angular/core';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-tax-shell',
  templateUrl: './tax-shell.component.html',
  styleUrls: ['./tax-shell.component.scss'],
})
export class TaxShellComponent {
  items: MenuItem[] = [
    { label: 'Preview', icon: 'pi pi-chart-line', routerLink: '/tax/preview' },
    { label: 'Configurations', icon: 'pi pi-sliders-h', routerLink: '/tax/config' },
    { label: 'Employee Profiles', icon: 'pi pi-id-card', routerLink: '/tax/profiles' },
    { label: 'Audit Log', icon: 'pi pi-shield', routerLink: '/tax/audit' },
  ];
}
