import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ReportsHomeComponent } from './pages/reports-home/reports-home.component';
import { ReportViewerComponent } from './pages/report-viewer/report-viewer.component';

const routes: Routes = [
  { path: '', component: ReportsHomeComponent },
  { path: 'view/:reportKey', component: ReportViewerComponent },
  { path: 'print/:reportKey', component: ReportViewerComponent, data: { print: true } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StatutoryReportsRoutingModule {}
