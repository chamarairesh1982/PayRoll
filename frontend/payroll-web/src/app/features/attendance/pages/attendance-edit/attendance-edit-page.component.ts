import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AttendanceApiService } from '../../services/attendance-api.service';
import { AttendanceRecord } from '../../models/attendance-record.model';

@Component({
  selector: 'app-attendance-edit-page',
  templateUrl: './attendance-edit-page.component.html',
  styleUrls: ['./attendance-edit-page.component.scss'],
})
export class AttendanceEditPageComponent implements OnInit {
  record?: AttendanceRecord;

  constructor(private attendanceApi: AttendanceApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.attendanceApi.getAttendanceRecord(id).subscribe({
        next: attendance => (this.record = attendance),
        error: err => console.error('Failed to load attendance record', err),
      });
    }
  }

  handleSubmit(payload: Partial<AttendanceRecord>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.attendanceApi.updateAttendanceRecord(id, payload).subscribe({
      next: () => this.router.navigate(['/attendance']),
      error: err => console.error('Failed to update attendance record', err),
    });
  }
}
