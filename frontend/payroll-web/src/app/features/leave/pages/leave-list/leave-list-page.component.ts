import { formatDate } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LazyLoadEvent } from 'primeng/api';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { LeaveRequestsApiService } from '../../services/leave-requests-api.service';
import { LeaveRequest, LeaveStatus } from '../../models/leave-request.model';

interface LeaveRequestRow extends LeaveRequest {
  employeeDisplay: string;
  periodDisplay: string;
}

@Component({
  selector: 'app-leave-list-page',
  templateUrl: './leave-list-page.component.html',
  styleUrls: ['./leave-list-page.component.scss'],
})
export class LeaveListPageComponent implements OnInit {
  requests: LeaveRequestRow[] = [];
  columns = [
    { field: 'requestedAt', header: 'Requested At', type: 'datetime' },
    { field: 'employeeDisplay', header: 'Employee' },
    { field: 'leaveType', header: 'Leave Type' },
    { field: 'periodDisplay', header: 'Period' },
    { field: 'status', header: 'Status' },
  ];
  statusOptions = ['Pending', 'Approved', 'Rejected', 'Cancelled'];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;

  selectedStatus: LeaveStatus | '' = '';
  selectedEmployeeId?: string;
  dateFrom?: string;
  dateTo?: string;

  showConfirm = false;
  requestToDelete: LeaveRequest | null = null;

  constructor(private leaveRequestsApi: LeaveRequestsApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;
    this.leaveRequestsApi
      .getLeaveRequests({
        page: this.page,
        pageSize: this.pageSize,
        employeeId: this.selectedEmployeeId,
        status: this.selectedStatus,
      })
      .subscribe({
        next: (result: PaginatedResult<LeaveRequest>) => {
          this.requests = result.items.map(item => ({
            ...item,
            employeeDisplay: item.employeeName || item.employeeCode || item.employeeId,
            periodDisplay: `${this.formatDisplayDate(item.startDate)} – ${this.formatDisplayDate(item.endDate)}`,
          }));
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load leave requests', err);
          this.isLoading = false;
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadRequests();
  }

  resetFilters(): void {
    this.selectedStatus = '';
    this.selectedEmployeeId = undefined;
    this.dateFrom = undefined;
    this.dateTo = undefined;
    this.applyFilters();
  }

  goToCreate(): void {
    this.router.navigate(['/leave/new']);
  }

  viewRequest(request: LeaveRequest): void {
    this.router.navigate(['/leave', request.id]);
  }

  editRequest(request: LeaveRequest): void {
    this.router.navigate(['/leave', request.id, 'edit']);
  }

  confirmDelete(request: LeaveRequest): void {
    this.requestToDelete = request;
    this.showConfirm = true;
  }

  cancelDelete(): void {
    this.requestToDelete = null;
    this.showConfirm = false;
  }

  deleteRequest(): void {
    if (!this.requestToDelete) {
      return;
    }

    this.leaveRequestsApi.deleteLeaveRequest(this.requestToDelete.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.loadRequests();
      },
      error: err => {
        console.error('Failed to delete leave request', err);
        this.cancelDelete();
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
    this.loadRequests();
  }

  get tableFirst(): number {
    return (this.page - 1) * this.pageSize;
  }

  private formatDisplayDate(value?: string): string {
    if (!value) {
      return '--';
    }

    return formatDate(value, 'yyyy-MM-dd', 'en-US');
  }
}
