import { Component, Input } from '@angular/core';
import { ApprovalHistoryEntry } from '../../models/approval.models';

@Component({
  selector: 'app-approval-history',
  templateUrl: './approval-history.component.html',
  styleUrls: ['./approval-history.component.scss'],
})
export class ApprovalHistoryComponent {
  @Input() history: ApprovalHistoryEntry[] = [];

  get hasHistory(): boolean {
    return this.history && this.history.length > 0;
  }

  getStatusSeverity(status?: string): 'success' | 'danger' | 'info' | 'warning' | undefined {
    if (!status) {
      return undefined;
    }
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
}
