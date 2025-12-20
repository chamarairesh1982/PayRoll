import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { OvertimeApiService } from '../../services/overtime-api.service';
import { OTEntry, OvertimeStatus } from '../../models/ot-entry.model';

@Component({
  selector: 'app-overtime-list-page',
  templateUrl: './overtime-list-page.component.html',
  styleUrls: ['./overtime-list-page.component.scss'],
})
export class OvertimeListPageComponent implements OnInit {
  records: OTEntry[] = [];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;

  selectedEmployeeId?: string;
  selectedFrom?: string;
  selectedTo?: string;
  selectedStatus: OvertimeStatus | '' = '';

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
        from: this.selectedFrom,
        to: this.selectedTo,
        status: this.selectedStatus,
      })
      .subscribe({
        next: (result: PaginatedResult<OTEntry>) => {
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
    this.selectedFrom = undefined;
    this.selectedTo = undefined;
    this.selectedStatus = '';
    this.applyFilters();
  }

  goToCreate(): void {
    this.router.navigate(['/overtime/new']);
  }

  viewRecord(record: OTEntry): void {
    this.router.navigate(['/overtime', record.id]);
  }

  editRecord(record: OTEntry): void {
    this.router.navigate(['/overtime', record.id, 'edit']);
  }

  submitRecord(record: OTEntry): void {
    if (record.isLockedForPayroll) {
      alert('This OT record is locked and cannot be submitted.');
      return;
    }

    this.overtimeApi.submitOvertimeRecord(record.id).subscribe({
      next: () => this.loadRecords(),
      error: err => {
        console.error('Failed to submit overtime record', err);
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

  canEdit(record: OTEntry): boolean {
    return record.status === 'Draft' && !record.isLockedForPayroll;
  }

  canSubmit(record: OTEntry): boolean {
    return record.status === 'Draft' && !record.isLockedForPayroll;
  }
}
