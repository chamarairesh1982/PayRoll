import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Table } from 'primeng/table';
import { Subject, takeUntil } from 'rxjs';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import {
  RecurringFrequency,
  RecurringRule,
  RecurringRuleStatus,
  RecurringRuleType,
} from '../../models/recurring-rule.model';
import { RecurringRuleService } from '../../services/recurring-rule.service';

import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-recurring-rules-list-page',
  templateUrl: './recurring-rules-list-page.component.html',
  styleUrls: ['./recurring-rules-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class RecurringRulesListPageComponent implements OnInit, OnDestroy {
  @ViewChild('rulesTable') rulesTable?: Table;

  rules: RecurringRule[] = [];
  filteredRules: RecurringRule[] = [];
  selectedRules: RecurringRule[] = [];
  searchTerm = '';
  isLoading = true;

  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  filterType: RecurringRuleType | '' = '';
  filterStatus: RecurringRuleStatus | '' = '';
  filterFrequency: RecurringFrequency | '' = '';
  filterDateRange: Date[] = [];
  filterCompanyId: string | null = null;
  filterBranchId: string | null = null;
  filterCostCenterId: string | null = null;

  columns: DataTableColumn<RecurringRule>[] = [
    { field: 'name', header: 'Rule Name', sortable: true, filterable: true },
    { field: 'type', header: 'Type', sortable: true, filterable: true, minWidth: '150px' },
    { field: 'amountValue', header: 'Amount', sortable: true, minWidth: '150px', type: 'amount' },
    { field: 'frequency', header: 'Frequency', sortable: true, minWidth: '130px' },
    { field: 'startDate', header: 'Effective Date', sortable: true, minWidth: '150px', type: 'date' },
    { field: 'status', header: 'Status', sortable: true, minWidth: '120px', type: 'status' },
  ];

  typeOptions = [
    { label: 'Allowance', value: 'Allowance' as RecurringRuleType },
    { label: 'Deduction', value: 'Deduction' as RecurringRuleType },
    { label: 'Loan installment', value: 'Loan installment' as RecurringRuleType },
    { label: 'Overtime rule', value: 'Overtime rule' as RecurringRuleType },
  ];

  statusOptions = [
    { label: 'Active', value: 'Active' as RecurringRuleStatus },
    { label: 'Inactive', value: 'Inactive' as RecurringRuleStatus },
  ];

  frequencyOptions = [
    { label: 'Monthly', value: 'Monthly' as RecurringFrequency },
    { label: 'Weekly', value: 'Weekly' as RecurringFrequency },
    { label: 'Fortnightly', value: 'Fortnightly' as RecurringFrequency },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private recurringRuleService: RecurringRuleService,
    private organizationApi: OrganizationApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.loadOrganizations();
    this.loadRules();
  }

  loadRules(): void {
    this.isLoading = true;
    this.recurringRuleService
      .getRules()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rules => {
          this.rules = rules;
          this.applyFilters();
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrganizations(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      this.loadBranches();
      this.loadCostCenters();
    });
  }

  loadBranches(): void {
    this.organizationApi.getBranches(this.filterCompanyId || undefined).subscribe(branches => {
      this.branches = branches;
    });
  }

  loadCostCenters(): void {
    this.organizationApi
      .getCostCenters(this.filterCompanyId || undefined, this.filterBranchId || undefined)
      .subscribe(costCenters => {
        this.costCenters = costCenters;
      });
  }

  onCompanyChange(value: string | null): void {
    this.filterCompanyId = value;
    this.filterBranchId = null;
    this.filterCostCenterId = null;
    this.loadBranches();
    this.loadCostCenters();
    this.applyFilters();
  }

  onBranchChange(value: string | null): void {
    this.filterBranchId = value;
    this.filterCostCenterId = null;
    this.loadCostCenters();
    this.applyFilters();
  }

  onCostCenterChange(value: string | null): void {
    this.filterCostCenterId = value;
    this.applyFilters();
  }

  onDateRangeChange(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  onGlobalSearch(value: string): void {
    this.searchTerm = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.filterType = '';
    this.filterStatus = '';
    this.filterFrequency = '';
    this.filterDateRange = [];
    this.filterCompanyId = null;
    this.filterBranchId = null;
    this.filterCostCenterId = null;
    this.searchTerm = '';
    this.selectedRules = [];
    this.loadBranches();
    this.loadCostCenters();
    this.applyFilters();
  }

  goToCreate(): void {
    this.router.navigate(['/payroll/recurring-rules/new']);
  }

  viewRule(rule: RecurringRule): void {
    this.router.navigate(['/payroll/recurring-rules', rule.id]);
  }

  editRule(rule: RecurringRule): void {
    this.router.navigate(['/payroll/recurring-rules', rule.id, 'edit']);
  }

  confirmDeactivate(rule: RecurringRule): void {
    const isActivating = rule.status === 'Inactive';
    this.confirmationService.confirm({
      header: `${isActivating ? 'Activate' : 'Deactivate'} Rule`,
      message: `Are you sure you want to ${isActivating ? 'activate' : 'deactivate'} ${rule.name}?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: isActivating ? 'Activate' : 'Deactivate',
      rejectLabel: 'Cancel',
      accept: () => {
        const nextStatus: RecurringRuleStatus = isActivating ? 'Active' : 'Inactive';
        this.recurringRuleService.setStatus(rule.id, nextStatus).subscribe(() => {
          this.messageService.add({ severity: 'success', summary: 'Updated', detail: `${rule.name} updated.` });
          this.loadRules();
        });
      },
    });
  }

  confirmDelete(rule: RecurringRule): void {
    this.confirmationService.confirm({
      header: 'Delete Rule',
      message: `Delete ${rule.name}?`,
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'Delete',
      accept: () => {
        this.recurringRuleService.deleteRule(rule.id).subscribe(() => {
          this.messageService.add({ severity: 'success', summary: 'Deleted', detail: `${rule.name} removed.` });
          this.loadRules();
        });
      },
    });
  }

  bulkUpdate(status: RecurringRuleStatus): void {
    const ids = this.selectedRules.map(rule => rule.id);
    if (!ids.length) return;

    this.confirmationService.confirm({
      header: 'Bulk Update',
      message: `Update ${ids.length} rules to ${status}?`,
      accept: () => {
        this.recurringRuleService.bulkUpdateStatus(ids, status).subscribe(() => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Rules updated.' });
          this.selectedRules = [];
          this.loadRules();
        });
      },
    });
  }

  getScopeLabel(rule: RecurringRule): string {
    if (rule.scope.type === 'All') return 'Everyone';
    if (rule.scope.type === 'Selected') return `Selected (${rule.scope.employeeIds?.length ?? 0})`;

    const parts = [
      rule.scope.companyId ? `Company: ${rule.scope.companyId}` : null,
      rule.scope.branchId ? `Branch: ${rule.scope.branchId}` : null,
    ].filter(Boolean);

    return parts.length ? parts.join(' · ') : 'Group';
  }

  private applyFilters(): void {
    let data = [...this.rules];

    if (this.filterType) data = data.filter(r => r.type === this.filterType);
    if (this.filterStatus) data = data.filter(r => r.status === this.filterStatus);
    if (this.filterFrequency) data = data.filter(r => r.frequency === this.filterFrequency);

    if (this.searchTerm) {
      const s = this.searchTerm.toLowerCase();
      data = data.filter(r => r.name.toLowerCase().includes(s) || r.type.toLowerCase().includes(s));
    }

    this.filteredRules = data;
  }
}
