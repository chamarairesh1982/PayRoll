import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportFilters, ReportFilterOptions } from '../../models/report.models';

@Component({
  selector: 'app-report-filters',
  templateUrl: './report-filters.component.html',
  styleUrls: ['./report-filters.component.scss'],
})
export class ReportFiltersComponent implements OnChanges {
  @Input() options: ReportFilterOptions | null = null;
  @Input() initialFilters: ReportFilters | null = null;
  @Input() loading = false;
  @Output() generate = new EventEmitter<ReportFilters>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      companyId: [null],
      branchId: [null],
      costCenterId: [null],
      payrollMonth: [null],
      employeeStatus: ['active'],
      outputMode: ['preview'],
      outputFormat: ['csv'],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialFilters'] && this.initialFilters) {
      this.form.patchValue(this.initialFilters);
    }
  }

  onGenerate(): void {
    if (this.form.valid) {
      this.generate.emit(this.form.value as ReportFilters);
    }
  }
}
