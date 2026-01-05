import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss'],
})
export class ReportsPageComponent {
  reportType = this.route.snapshot.data['reportType'] ?? 'Payroll Reports';

  constructor(private route: ActivatedRoute) {}
}
