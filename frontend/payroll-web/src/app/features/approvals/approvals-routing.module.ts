import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminGuard } from '../../core/guards/admin.guard';
import { ApprovalConfigPageComponent } from './pages/approval-config/approval-config-page.component';
import { ApprovalsInboxPageComponent } from './pages/approvals-inbox/approvals-inbox-page.component';

const routes: Routes = [
  { path: '', redirectTo: 'inbox', pathMatch: 'full' },
  { path: 'inbox', component: ApprovalsInboxPageComponent },
  { path: 'config', component: ApprovalConfigPageComponent, canActivate: [AdminGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ApprovalsRoutingModule {}
