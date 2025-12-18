import { Component, OnInit } from '@angular/core';
import { AttendanceApiService } from '../../services/attendance-api.service';
import { AttendanceRecord } from '../../models/attendance-record.model';
import { PaginatedResult } from '../../../employees/models/employee.model';

@Component({
  selector: 'app-attendance-list-page',
  templateUrl: './attendance-list-page.component.html',
  styleUrls: ['./attendance-list-page.component.scss'],
})
export class AttendanceListPageComponent implements OnInit {
  records: AttendanceRecord[] = [];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;

  showCreateForm = false;
  errorMessage: string | null = null;

  constructor(private attendanceApi: AttendanceApiService) {}

  ngOnInit(): void {
    this.loadRecords();
  }

  loadRecords(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.attendanceApi
      .getAttendanceRecords({ page: this.page, pageSize: this.pageSize })
      .subscribe({
        next: (result: PaginatedResult<AttendanceRecord>) => {
          this.records = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load attendance records', err);
          this.errorMessage = 'Failed to load attendance records. Please try again.';
          this.isLoading = false;
        },
      });
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
  }

  handleCreate(payload: Partial<AttendanceRecord>): void {
    this.errorMessage = null;
    this.attendanceApi.recordAttendance(payload).subscribe({
      next: () => {
        this.showCreateForm = false;
        this.loadRecords();
      },
      error: err => {
        console.error('Failed to record attendance', err);
        this.errorMessage = 'Failed to record attendance. Please check the details and try again.';
      },
    });
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.loadRecords();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadRecords();
    }
  }

  get totalPages(): number {
    return this.pageSize ? Math.ceil(this.totalCount / this.pageSize) : 1;
  }
}
