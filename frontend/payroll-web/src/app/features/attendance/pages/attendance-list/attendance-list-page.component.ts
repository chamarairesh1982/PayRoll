import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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

  selectedDate?: string;
  selectedEmployeeId?: string;

  showConfirm = false;
  recordToDelete: AttendanceRecord | null = null;

  showCreateForm = false;

  constructor(private attendanceApi: AttendanceApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadRecords();
  }

  loadRecords(): void {
    this.isLoading = true;
    this.attendanceApi
      .getAttendanceRecords({ page: this.page, pageSize: this.pageSize, date: this.selectedDate, employeeId: this.selectedEmployeeId })
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
          this.isLoading = false;
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadRecords();
  }

  resetFilters(): void {
    this.selectedDate = undefined;
    this.selectedEmployeeId = undefined;
    this.applyFilters();
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
  }

  handleCreate(payload: Partial<AttendanceRecord>): void {
    this.attendanceApi.createAttendanceRecord(payload).subscribe({
      next: () => {
        this.showCreateForm = false;
        this.loadRecords();
      },
      error: err => console.error('Failed to create attendance record', err),
    });
  }

  editRecord(record: AttendanceRecord): void {
    this.router.navigate(['/attendance', record.id, 'edit']);
  }

  confirmDelete(record: AttendanceRecord): void {
    this.recordToDelete = record;
    this.showConfirm = true;
  }

  cancelDelete(): void {
    this.recordToDelete = null;
    this.showConfirm = false;
  }

  deleteRecord(): void {
    if (!this.recordToDelete) {
      return;
    }

    this.attendanceApi.deleteAttendanceRecord(this.recordToDelete.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.loadRecords();
      },
      error: err => {
        console.error('Failed to delete attendance record', err);
        this.cancelDelete();
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
