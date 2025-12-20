import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuditLogListPageComponent } from './pages/audit-log-list/audit-log-list-page.component';
import { GlSettingsPageComponent } from './pages/gl-settings/gl-settings-page.component';

const routes: Routes = [
  { path: '', redirectTo: 'audit-logs', pathMatch: 'full' },
  { path: 'audit-logs', component: AuditLogListPageComponent },
  { path: 'general-ledger', component: GlSettingsPageComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
