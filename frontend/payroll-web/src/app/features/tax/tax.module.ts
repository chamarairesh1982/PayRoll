import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { TaxAuditLogPageComponent } from './pages/tax-audit-log/tax-audit-log-page.component';
import { TaxConfigPageComponent } from './pages/tax-config/tax-config-page.component';
import { TaxPreviewPageComponent } from './pages/tax-preview/tax-preview-page.component';
import { TaxProfilesPageComponent } from './pages/tax-profiles/tax-profiles-page.component';
import { TaxShellComponent } from './pages/tax-shell/tax-shell.component';
import { TaxRoutingModule } from './tax-routing.module';

@NgModule({
  declarations: [
    TaxShellComponent,
    TaxConfigPageComponent,
    TaxProfilesPageComponent,
    TaxPreviewPageComponent,
    TaxAuditLogPageComponent,
  ],
  imports: [SharedModule, TaxRoutingModule],
})
export class TaxModule {}
