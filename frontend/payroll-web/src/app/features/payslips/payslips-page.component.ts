import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-payslips-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './payslips-page.component.html',
  styleUrls: ['./payslips-page.component.scss'],
})
export class PayslipsPageComponent {
  deliveryStatus = 'Pending Release';
}
