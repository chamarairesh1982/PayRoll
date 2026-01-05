import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuditLogListPageComponent } from './pages/audit-log-list/audit-log-list-page.component';
import { GlSettingsPageComponent } from './pages/gl-settings/gl-settings-page.component';
import { RuleVersionsPageComponent } from './pages/rule-versions/rule-versions-page.component';

const routes: Routes = [
  { path: '', redirectTo: 'audit-logs', pathMatch: 'full' },
  { path: 'audit-logs', component: AuditLogListPageComponent, data: { breadcrumb: 'Audit Logs' } },
  { path: 'general-ledger', component: GlSettingsPageComponent, data: { breadcrumb: 'General Ledger' } },
  { path: 'rule-versions', component: RuleVersionsPageComponent, data: { breadcrumb: 'Rule Versions' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
