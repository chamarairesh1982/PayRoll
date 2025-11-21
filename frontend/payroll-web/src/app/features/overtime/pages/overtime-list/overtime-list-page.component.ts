import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { OvertimeApiService } from '../../services/overtime-api.service';
import { OvertimeRecord, OvertimeStatus } from '../../models/overtime-record.model';

@Component({
  selector: 'app-overtime-list-page',
  templateUrl: './overtime-list-page.component.html',
  styleUrls: ['./overtime-list-page.component.scss'],
})
export class OvertimeListPageComponent implements OnInit {
  records: OvertimeRecord[] = [];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;

  selectedEmployeeId?: string;
  selectedDate?: string;
  selectedStatus: OvertimeStatus | '' = '';

  showConfirm = false;
  recordToDelete: OvertimeRecord | null = null;

  constructor(private overtimeApi: OvertimeApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadRecords();
  }

  loadRecords(): void {
    this.isLoading = true;
    this.overtimeApi
      .getOvertimeRecords({
        page: this.page,
        pageSize: this.pageSize,
        employeeId: this.selectedEmployeeId,
        date: this.selectedDate,
        status: this.selectedStatus,
      })
      .subscribe({
        next: (result: PaginatedResult<OvertimeRecord>) => {
          this.records = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load overtime records', err);
          this.isLoading = false;
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadRecords();
  }

  resetFilters(): void {
    this.selectedEmployeeId = undefined;
    this.selectedDate = undefined;
    this.selectedStatus = '';
    this.applyFilters();
  }

  goToCreate(): void {
    this.router.navigate(['/overtime/new']);
  }

  viewRecord(record: OvertimeRecord): void {
    this.router.navigate(['/overtime', record.id]);
  }

  editRecord(record: OvertimeRecord): void {
    this.router.navigate(['/overtime', record.id, 'edit']);
  }

  confirmDelete(record: OvertimeRecord): void {
    if (record.isLockedForPayroll) {
      alert('This OT record is locked and cannot be deleted.');
      return;
    }

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

    this.overtimeApi.deleteOvertimeRecord(this.recordToDelete.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.loadRecords();
      },
      error: err => {
        console.error('Failed to delete overtime record', err);
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
