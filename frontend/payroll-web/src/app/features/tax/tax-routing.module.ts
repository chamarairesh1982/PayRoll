import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TaxAuditLogPageComponent } from './pages/tax-audit-log/tax-audit-log-page.component';
import { TaxConfigPageComponent } from './pages/tax-config/tax-config-page.component';
import { TaxPreviewPageComponent } from './pages/tax-preview/tax-preview-page.component';
import { TaxProfilesPageComponent } from './pages/tax-profiles/tax-profiles-page.component';
import { TaxShellComponent } from './pages/tax-shell/tax-shell.component';

const routes: Routes = [
  {
    path: '',
    component: TaxShellComponent,
    data: { breadcrumb: 'Tax' },
    children: [
      { path: '', redirectTo: 'preview', pathMatch: 'full' },
      { path: 'preview', component: TaxPreviewPageComponent, data: { breadcrumb: 'Tax Preview' } },
      { path: 'config', component: TaxConfigPageComponent, data: { breadcrumb: 'Tax Configuration' } },
      { path: 'profiles', component: TaxProfilesPageComponent, data: { breadcrumb: 'Tax Profiles' } },
      { path: 'audit', component: TaxAuditLogPageComponent, data: { breadcrumb: 'Tax Audit Log' } },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TaxRoutingModule {}
