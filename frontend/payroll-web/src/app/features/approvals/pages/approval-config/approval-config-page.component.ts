import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ApprovalRouteConfig, ApprovalRequestType } from '../../models/approval.models';
import { ApprovalService } from '../../services/approval.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

const REQUEST_TYPES: ApprovalRequestType[] = [
  'Leave Request',
  'Overtime',
  'Loan',
  'Payroll Adjustment',
  'Employee Master Data Change',
];

const LEVEL_OPTIONS = ['Level 1 Manager', 'Level 2 HR', 'Finance'];

@Component({
  selector: 'app-approval-config-page',
  templateUrl: './approval-config-page.component.html',
  styleUrls: ['./approval-config-page.component.scss'],
})
export class ApprovalConfigPageComponent implements OnInit, OnDestroy {
  columns: DataTableColumn<ApprovalRouteConfig>[] = [
    { field: 'type', header: 'Workflow Specification', sortable: true, minWidth: '200px' },
    { field: 'levels', header: 'Authorization Chain', sortable: false, minWidth: '250px' },
    { field: 'approvers', header: 'Designated Approvers', sortable: false, minWidth: '200px' },
    { field: 'threshold', header: 'Activation Threshold', type: 'amount', sortable: true, minWidth: '150px' },
  ];

  isLoading = false;
  configs: ApprovalRouteConfig[] = [];
  requestTypes = REQUEST_TYPES;
  levelOptions = LEVEL_OPTIONS;

  configForm = this.fb.group({
    id: new FormControl('', { nonNullable: true }),
    type: new FormControl('', { nonNullable: true, validators: Validators.required }),
    levels: new FormControl<string[]>([], { nonNullable: true, validators: Validators.required }),
    approvers: new FormControl('', { nonNullable: true, validators: Validators.required }),
    threshold: new FormControl<number | null>(null),
  });

  private destroy$ = new Subject<void>();

  constructor(private approvalService: ApprovalService, private fb: FormBuilder) { }

  ngOnInit(): void {
    this.approvalService
      .getConfigs()
      .pipe(takeUntil(this.destroy$))
      .subscribe(configs => {
        this.configs = configs;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectConfig(config: ApprovalRouteConfig): void {
    this.configForm.patchValue({
      id: config.id,
      type: config.type,
      levels: config.levels,
      approvers: config.approvers.join(', '),
      threshold: config.threshold ?? null,
    });
  }

  saveConfig(): void {
    if (this.configForm.invalid) {
      this.configForm.markAllAsTouched();
      return;
    }

    const value = this.configForm.value;
    const approvers = (value.approvers || '')
      .split(',')
      .map(item => item.trim())
      .filter(Boolean);

    this.approvalService.saveConfig({
      id: value.id || '',
      type: value.type as ApprovalRequestType,
      levels: (value.levels as string[]) || [],
      approvers,
      threshold: value.threshold ?? null,
    });

    this.resetForm();
  }

  resetForm(): void {
    this.configForm.reset({
      id: '',
      type: '',
      levels: [],
      approvers: '',
      threshold: null,
    });
  }
}
