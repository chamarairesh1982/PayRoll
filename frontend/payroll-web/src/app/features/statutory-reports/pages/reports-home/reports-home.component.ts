import { Component } from '@angular/core';
import { ReportDefinition } from '../../models/report.models';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-reports-home',
  templateUrl: './reports-home.component.html',
  styleUrls: ['./reports-home.component.scss'],
})
export class ReportsHomeComponent {
  reports: ReportDefinition[] = [];

  constructor(private reportService: ReportService) {
    this.reports = this.reportService.getReportDefinitions();
  }
}
