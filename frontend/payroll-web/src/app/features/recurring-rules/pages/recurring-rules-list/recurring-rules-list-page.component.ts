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
  ) {}

  ngOnInit(): void {
    this.loadOrganizations();
    this.recurringRuleService
      .getRules()
      .pipe(takeUntil(this.destroy$))
      .subscribe(rules => {
        this.rules = rules;
        this.applyFilters();
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
    if (this.rulesTable) {
      this.rulesTable.filterGlobal(value, 'contains');
    }
  }

  clearFilters(): void {
    this.filterType = '';
    this.filterStatus = '';
    this.filterFrequency = '';
    this.filterDateRange = [];
    this.filterCompanyId = null;
    this.filterBranchId = null;
    this.filterCostCenterId = null;
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
    this.confirmationService.confirm({
      header: `${rule.status === 'Active' ? 'Deactivate' : 'Activate'} Rule`,
      message: `Are you sure you want to ${rule.status === 'Active' ? 'deactivate' : 'activate'} ${rule.name}?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: rule.status === 'Active' ? 'Deactivate' : 'Activate',
      rejectLabel: 'Cancel',
      accept: () => {
        const nextStatus = rule.status === 'Active' ? 'Inactive' : 'Active';
        this.recurringRuleService.setStatus(rule.id, nextStatus).subscribe(() => {
          this.messageService.add({
            severity: 'success',
            summary: 'Status updated',
            detail: `${rule.name} is now ${nextStatus.toLowerCase()}.`,
          });
        });
      },
    });
  }

  confirmDelete(rule: RecurringRule): void {
    this.confirmationService.confirm({
      header: 'Delete Rule',
      message: `Delete ${rule.name}? This cannot be undone.`,
      icon: 'pi pi-info-circle',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      accept: () => {
        this.recurringRuleService.deleteRule(rule.id).subscribe(() => {
          this.messageService.add({
            severity: 'success',
            summary: 'Rule deleted',
            detail: `${rule.name} has been removed.`,
          });
        });
      },
    });
  }

  bulkUpdate(status: RecurringRuleStatus): void {
    const ids = this.selectedRules.map(rule => rule.id);
    if (!ids.length) {
      return;
    }

    this.confirmationService.confirm({
      header: `${status === 'Active' ? 'Activate' : 'Deactivate'} Rules`,
      message: `Apply ${status.toLowerCase()} status to ${ids.length} selected rules?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: status === 'Active' ? 'Activate' : 'Deactivate',
      rejectLabel: 'Cancel',
      accept: () => {
        this.recurringRuleService.bulkUpdateStatus(ids, status).subscribe(() => {
          this.messageService.add({
            severity: 'success',
            summary: 'Bulk update complete',
            detail: `Updated ${ids.length} rules.`,
          });
          this.selectedRules = [];
        });
      },
    });
  }

  exportCsv(): void {
    if (this.rulesTable) {
      this.rulesTable.exportCSV();
    }
  }

  getScopeLabel(rule: RecurringRule): string {
    if (rule.scope.type === 'All') {
      return 'All employees';
    }
    if (rule.scope.type === 'Selected') {
      const count = rule.scope.employeeIds?.length ?? 0;
      return `Selected (${count})`;
    }

    const parts = [
      rule.scope.companyId ? `Company: ${rule.scope.companyId}` : null,
      rule.scope.branchId ? `Branch: ${rule.scope.branchId}` : null,
      rule.scope.costCenterId ? `Cost center: ${rule.scope.costCenterId}` : null,
      rule.scope.employeeCategory ? `Category: ${rule.scope.employeeCategory}` : null,
    ].filter(Boolean);

    return parts.length ? parts.join(' · ') : 'Group (all)';
  }

  private applyFilters(): void {
    let data = [...this.rules];

    if (this.filterType) {
      data = data.filter(rule => rule.type === this.filterType);
    }

    if (this.filterStatus) {
      data = data.filter(rule => rule.status === this.filterStatus);
    }

    if (this.filterFrequency) {
      data = data.filter(rule => rule.frequency === this.filterFrequency);
    }

    if (this.filterCompanyId) {
      data = data.filter(rule => rule.scope.type === 'Group' && rule.scope.companyId === this.filterCompanyId);
    }

    if (this.filterBranchId) {
      data = data.filter(rule => rule.scope.type === 'Group' && rule.scope.branchId === this.filterBranchId);
    }

    if (this.filterCostCenterId) {
      data = data.filter(rule => rule.scope.type === 'Group' && rule.scope.costCenterId === this.filterCostCenterId);
    }

    if (this.filterDateRange?.length === 2) {
      const [start, end] = this.filterDateRange;
      const rangeStart = start ? new Date(start) : null;
      const rangeEnd = end ? new Date(end) : null;

      data = data.filter(rule => {
        const ruleStart = new Date(rule.startDate);
        const ruleEnd = rule.endDate ? new Date(rule.endDate) : null;
        const startsBeforeEnd = rangeEnd ? ruleStart <= rangeEnd : true;
        const endsAfterStart = rangeStart ? (!ruleEnd || ruleEnd >= rangeStart) : true;
        return startsBeforeEnd && endsAfterStart;
      });
    }

    this.filteredRules = data;

    if (this.rulesTable) {
      this.rulesTable.filterGlobal(this.searchTerm, 'contains');
    }
  }
}
