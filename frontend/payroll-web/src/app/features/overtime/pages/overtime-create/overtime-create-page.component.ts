import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { OvertimeRecord } from '../../models/overtime-record.model';
import { OvertimeApiService } from '../../services/overtime-api.service';

@Component({
  selector: 'app-overtime-create-page',
  templateUrl: './overtime-create-page.component.html',
  styleUrls: ['./overtime-create-page.component.scss'],
})
export class OvertimeCreatePageComponent {
  constructor(private overtimeApi: OvertimeApiService, private router: Router) {}

  onSubmitted(payload: Partial<OvertimeRecord>): void {
    this.overtimeApi.createOvertimeRecord(payload).subscribe({
      next: () => this.router.navigate(['/overtime']),
      error: err => console.error('Failed to create overtime record', err),
    });
  }
}
