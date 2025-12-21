import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequest } from '../../models/leave-request.model';
import { MessageService } from 'primeng/api';
import { ApprovalService } from '../../../approvals/services/approval.service';
import { LeaveRequestsApiService } from '../../services/leave-requests-api.service';

@Component({
  selector: 'app-leave-detail-page',
  templateUrl: './leave-detail-page.component.html',
  styleUrls: ['./leave-detail-page.component.scss'],
})
export class LeaveDetailPageComponent implements OnInit {
  leaveRequest?: LeaveRequest;
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private leaveRequestsApi: LeaveRequestsApiService,
    private router: Router,
    private approvalService: ApprovalService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isLoading = true;
      this.leaveRequestsApi.getLeaveRequest(id).subscribe({
        next: request => {
          this.leaveRequest = request;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load leave request', err);
          this.isLoading = false;
        },
      });
    }
  }

  editRequest(): void {
    if (this.leaveRequest) {
      this.router.navigate(['/leave', this.leaveRequest.id, 'edit']);
    }
  }

  goBack(): void {
    this.router.navigate(['/leave']);
  }

  submitForApproval(): void {
    if (!this.leaveRequest) {
      return;
    }
    this.approvalService.submitRequest({
      type: 'Leave Request',
      employee: this.leaveRequest.employeeName || this.leaveRequest.employeeCode || this.leaveRequest.employeeId,
      payload: {
        leaveType: this.leaveRequest.leaveType,
        startDate: this.leaveRequest.startDate,
        endDate: this.leaveRequest.endDate,
        totalDays: this.leaveRequest.totalDays,
        reason: this.leaveRequest.reason,
      },
    });
    this.messageService.add({
      severity: 'success',
      summary: 'Submitted',
      detail: 'Leave request sent for approval.',
    });
  }
}
