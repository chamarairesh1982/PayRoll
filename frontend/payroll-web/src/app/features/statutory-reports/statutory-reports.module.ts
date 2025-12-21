import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ExportToolbarComponent } from './components/export-toolbar/export-toolbar.component';
import { ReportFiltersComponent } from './components/report-filters/report-filters.component';
import { ReportTableComponent } from './components/report-table/report-table.component';
import { ReportViewerComponent } from './pages/report-viewer/report-viewer.component';
import { ReportsHomeComponent } from './pages/reports-home/reports-home.component';
import { StatutoryReportsRoutingModule } from './statutory-reports-routing.module';

@NgModule({
  declarations: [
    ReportsHomeComponent,
    ReportViewerComponent,
    ReportFiltersComponent,
    ReportTableComponent,
    ExportToolbarComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, StatutoryReportsRoutingModule],
})
export class StatutoryReportsModule {}
