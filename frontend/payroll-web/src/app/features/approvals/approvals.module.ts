import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ApprovalActionDialogComponent } from './components/approval-action-dialog/approval-action-dialog.component';
import { ApprovalDetailPanelComponent } from './components/approval-detail-panel/approval-detail-panel.component';
import { ApprovalHistoryComponent } from './components/approval-history/approval-history.component';
import { ApprovalsRoutingModule } from './approvals-routing.module';
import { ApprovalConfigPageComponent } from './pages/approval-config/approval-config-page.component';
import { ApprovalsInboxPageComponent } from './pages/approvals-inbox/approvals-inbox-page.component';

@NgModule({
  declarations: [
    ApprovalsInboxPageComponent,
    ApprovalConfigPageComponent,
    ApprovalDetailPanelComponent,
    ApprovalActionDialogComponent,
    ApprovalHistoryComponent,
  ],
  imports: [SharedModule, FormsModule, ReactiveFormsModule, ApprovalsRoutingModule],
})
export class ApprovalsModule {}
