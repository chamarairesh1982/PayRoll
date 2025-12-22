import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-statutory-epf-etf-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './statutory-epf-etf-page.component.html',
  styleUrls: ['./statutory-epf-etf-page.component.scss'],
})
export class StatutoryEpfEtfPageComponent {
  period = 'April 2024';
}
