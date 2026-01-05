import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-employees-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './employees-page.component.html',
  styleUrls: ['./employees-page.component.scss'],
})
export class EmployeesPageComponent {
  status = 'Active';
}
