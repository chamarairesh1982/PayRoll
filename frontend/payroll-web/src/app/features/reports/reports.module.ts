import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ReportsRoutingModule } from './reports-routing.module';
import { StatutoryReportsPageComponent } from './pages/statutory-reports-page/statutory-reports-page.component';

@NgModule({
  declarations: [StatutoryReportsPageComponent],
  imports: [SharedModule, FormsModule, ReportsRoutingModule],
})
export class ReportsModule {}
