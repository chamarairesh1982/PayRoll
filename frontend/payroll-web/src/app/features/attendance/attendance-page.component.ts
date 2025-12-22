import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-attendance-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './attendance-page.component.html',
  styleUrls: ['./attendance-page.component.scss'],
})
export class AttendancePageComponent {
  periodLabel = 'April 01 - April 30';
}
