import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { LeaveRequestsApiService } from '../../services/leave-requests-api.service';
import { LeaveRequest, LeaveStatus } from '../../models/leave-request.model';

@Component({
  selector: 'app-leave-list-page',
  templateUrl: './leave-list-page.component.html',
  styleUrls: ['./leave-list-page.component.scss'],
})
export class LeaveListPageComponent implements OnInit {
  requests: LeaveRequest[] = [];
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
          this.requests = result.items;
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

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.loadRequests();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadRequests();
    }
  }

  get totalPages(): number {
    return this.pageSize ? Math.ceil(this.totalCount / this.pageSize) : 1;
  }
}
