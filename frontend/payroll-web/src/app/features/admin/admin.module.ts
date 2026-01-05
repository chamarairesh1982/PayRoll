import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AdminRoutingModule } from './admin-routing.module';
import { AuditLogListPageComponent } from './pages/audit-log-list/audit-log-list-page.component';
import { GlSettingsPageComponent } from './pages/gl-settings/gl-settings-page.component';
import { RuleVersionsPageComponent } from './pages/rule-versions/rule-versions-page.component';

@NgModule({
  declarations: [AuditLogListPageComponent, GlSettingsPageComponent, RuleVersionsPageComponent],
  imports: [SharedModule, FormsModule, AdminRoutingModule],
})
export class AdminModule {}
