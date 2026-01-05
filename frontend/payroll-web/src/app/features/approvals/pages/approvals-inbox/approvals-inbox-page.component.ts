import { Component, OnDestroy, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { Subject, takeUntil } from 'rxjs';
import { ApprovalAction, ApprovalRequest, ApprovalRequestType, ApprovalStatus } from '../../models/approval.models';
import { ApprovalService } from '../../services/approval.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

const STATUS_FILTERS: Array<ApprovalStatus | 'All'> = ['All', 'Pending', 'Approved', 'Rejected', 'Returned'];
const TYPE_FILTERS: Array<ApprovalRequestType | 'All'> = [
  'All',
  'Leave Request',
  'Overtime',
  'Loan',
  'Payroll Adjustment',
  'Employee Master Data Change',
];

@Component({
  selector: 'app-approvals-inbox-page',
  templateUrl: './approvals-inbox-page.component.html',
  styleUrls: ['./approvals-inbox-page.component.scss'],
})
export class ApprovalsInboxPageComponent implements OnInit, OnDestroy {
  columns: DataTableColumn<ApprovalRequest>[] = [
    { field: 'status', header: 'Status', type: 'status', sortable: true, minWidth: '120px' },
    { field: 'type', header: 'Workflow Type', sortable: true, minWidth: '180px' },
    { field: 'requestedBy', header: 'Petitioner', sortable: true, minWidth: '180px' },
    { field: 'requestedDate', header: 'Submitted', type: 'date', sortable: true, minWidth: '130px' },
    { field: 'employee', header: 'Staff Entity', sortable: true, minWidth: '180px' },
    { field: 'amount', header: 'Magnitude', type: 'amount', sortable: true, minWidth: '130px' },
    { field: 'id', header: 'Aging (Days)', type: 'number', sortable: false, minWidth: '100px' }, // We'll map aging here
  ];

  isLoading = false;
  requests: ApprovalRequest[] = [];
  filteredRequests: ApprovalRequest[] = [];
  statusFilters = STATUS_FILTERS;
  typeFilters = TYPE_FILTERS;
  selectedStatus: ApprovalStatus | 'All' = 'All';
  selectedType: ApprovalRequestType | 'All' = 'All';

  selectedRequest?: ApprovalRequest;
  detailVisible = false;

  actionDialogVisible = false;
  selectedAction: ApprovalAction = 'Approve';

  private destroy$ = new Subject<void>();

  constructor(private approvalService: ApprovalService, private messageService: MessageService) { }

  ngOnInit(): void {
    this.approvalService
      .getInboxItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe(requests => {
        this.requests = requests;
        this.applyFilters();
        if (this.selectedRequest) {
          this.selectedRequest = this.requests.find(item => item.id === this.selectedRequest?.id);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  applyFilters(): void {
    this.filteredRequests = this.requests.filter(request => {
      const statusMatch = this.selectedStatus === 'All' || request.status === this.selectedStatus;
      const typeMatch = this.selectedType === 'All' || request.type === this.selectedType;
      return statusMatch && typeMatch;
    });
  }

  selectStatus(filter: ApprovalStatus | 'All'): void {
    this.selectedStatus = filter;
    this.applyFilters();
  }

  selectType(filter: ApprovalRequestType | 'All'): void {
    this.selectedType = filter;
    this.applyFilters();
  }

  getAgingDays(date: string): number {
    const requested = new Date(date).getTime();
    const now = new Date().getTime();
    const diff = now - requested;
    return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)));
  }

  getStatusSeverity(status: ApprovalStatus): 'success' | 'danger' | 'info' | 'warning' {
    switch (status) {
      case 'Approved':
        return 'success';
      case 'Rejected':
        return 'danger';
      case 'Returned':
        return 'warning';
      default:
        return 'info';
    }
  }

  openDetail(request: ApprovalRequest): void {
    this.selectedRequest = request;
    this.detailVisible = true;
  }

  closeDetail(): void {
    this.detailVisible = false;
  }

  openAction(request: ApprovalRequest, action: ApprovalAction): void {
    this.selectedRequest = request;
    this.selectedAction = action;
    this.actionDialogVisible = true;
  }

  closeActionDialog(): void {
    this.actionDialogVisible = false;
  }

  handleActionConfirm(result: { action: ApprovalAction; comment: string }): void {
    if (!this.selectedRequest) {
      return;
    }
    this.approvalService.performAction(this.selectedRequest.id, result.action, result.comment);
    this.messageService.add({
      severity: 'success',
      summary: 'Approval updated',
      detail: `${this.selectedRequest.type} has been ${this.formatActionMessage(result.action)}.`,
    });
    this.actionDialogVisible = false;
  }

  handlePanelAction(action: ApprovalAction): void {
    if (!this.selectedRequest) {
      return;
    }
    this.openAction(this.selectedRequest, action);
  }

  private formatActionMessage(action: ApprovalAction): string {
    switch (action) {
      case 'Approve':
        return 'approved';
      case 'Reject':
        return 'rejected';
      case 'Send Back':
        return 'sent back';
      default:
        return 'commented on';
    }
  }
}
