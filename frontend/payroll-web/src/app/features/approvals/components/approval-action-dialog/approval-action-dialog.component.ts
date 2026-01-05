import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ApprovalAction, ApprovalRequest } from '../../models/approval.models';

@Component({
  selector: 'app-approval-action-dialog',
  templateUrl: './approval-action-dialog.component.html',
  styleUrls: ['./approval-action-dialog.component.scss'],
})
export class ApprovalActionDialogComponent implements OnChanges {
  @Input() visible = false;
  @Input() action: ApprovalAction = 'Approve';
  @Input() request?: ApprovalRequest;
  @Output() closed = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<{ action: ApprovalAction; comment: string }>();

  comment = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.comment = '';
    }
  }

  get requiresComment(): boolean {
    return this.action === 'Reject' || this.action === 'Send Back';
  }

  get canSubmit(): boolean {
    if (!this.requiresComment) {
      return true;
    }
    return this.comment.trim().length > 0;
  }

  closeDialog(): void {
    this.closed.emit();
  }

  submit(): void {
    if (!this.canSubmit) {
      return;
    }
    this.confirmed.emit({ action: this.action, comment: this.comment.trim() });
  }
}
