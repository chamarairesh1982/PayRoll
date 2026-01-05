import { Component, OnInit } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { AttendanceApiService } from '../../services/attendance-api.service';
import { AttendanceRecord } from '../../models/attendance-record.model';
import { PaginatedResult } from '../../../employees/models/employee.model';

interface AttendanceRecordRow extends AttendanceRecord {
  employeeDisplay: string;
}

@Component({
  selector: 'app-attendance-list-page',
  templateUrl: './attendance-list-page.component.html',
  styleUrls: ['./attendance-list-page.component.scss'],
})
export class AttendanceListPageComponent implements OnInit {
  records: AttendanceRecordRow[] = [];
  columns = [
    { field: 'periodStart', header: 'Period Start', type: 'date' },
    { field: 'periodEnd', header: 'Period End', type: 'date' },
    { field: 'employeeDisplay', header: 'Employee' },
    { field: 'hoursWorked', header: 'Hours Worked', type: 'number' },
  ];
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
          this.records = result.items.map(item => ({
            ...item,
            employeeDisplay: item.employeeName || item.employeeId,
          }));
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

  handleLazyLoad(event: LazyLoadEvent): void {
    const nextRows = event.rows ?? this.pageSize;
    const nextFirst = event.first ?? 0;
    const nextPage = Math.floor(nextFirst / nextRows) + 1;

    if (nextPage === this.page && nextRows === this.pageSize) {
      return;
    }

    this.page = nextPage;
    this.pageSize = nextRows;
    this.loadRecords();
  }

  get tableFirst(): number {
    return (this.page - 1) * this.pageSize;
  }
}
