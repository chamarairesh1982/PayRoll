import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-pay-runs-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './pay-runs-page.component.html',
  styleUrls: ['./pay-runs-page.component.scss'],
})
export class PayRunsPageComponent {
  flow = 'Draft → Calculated → Approved → Finalized → Paid';
}
