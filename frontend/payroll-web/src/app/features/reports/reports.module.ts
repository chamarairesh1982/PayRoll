import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ReportsRoutingModule } from './reports-routing.module';
import { StatutoryReportsPageComponent } from './pages/statutory-reports-page/statutory-reports-page.component';
import { TaxReportsPageComponent } from './pages/tax-reports-page/tax-reports-page.component';
import { CostAnalysisPageComponent } from './pages/cost-analysis-page/cost-analysis-page.component';

@NgModule({
  declarations: [StatutoryReportsPageComponent, TaxReportsPageComponent, CostAnalysisPageComponent],
  imports: [SharedModule, FormsModule, ReportsRoutingModule],
})
export class ReportsModule { }
