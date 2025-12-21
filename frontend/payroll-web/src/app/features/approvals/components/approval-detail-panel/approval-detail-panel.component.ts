import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ApprovalAction, ApprovalRequest } from '../../models/approval.models';

@Component({
  selector: 'app-approval-detail-panel',
  templateUrl: './approval-detail-panel.component.html',
  styleUrls: ['./approval-detail-panel.component.scss'],
})
export class ApprovalDetailPanelComponent {
  @Input() visible = false;
  @Input() request?: ApprovalRequest;
  @Output() closed = new EventEmitter<void>();
  @Output() actionSelected = new EventEmitter<ApprovalAction>();

  get hasAmount(): boolean {
    return !!this.request?.amount;
  }

  closePanel(): void {
    this.closed.emit();
  }

  triggerAction(action: ApprovalAction): void {
    this.actionSelected.emit(action);
  }
}
