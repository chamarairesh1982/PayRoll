import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';
import { RulePackage, RulePackageType, RulePackageVersion } from '../../models/rule-package.model';
import { RulePackagesApiService } from '../../services/rule-packages-api.service';

@Component({
  selector: 'app-rule-versions-page',
  templateUrl: './rule-versions-page.component.html',
  styleUrls: ['./rule-versions-page.component.scss'],
  providers: [MessageService]
})
export class RuleVersionsPageComponent implements OnInit {
  ruleTypes: { label: string, value: RulePackageType }[] = [
    { label: 'Fiscal Tax (APIT)', value: 'Tax' },
    { label: 'Social Security (EPF)', value: 'Epf' },
    { label: 'Social Security (ETF)', value: 'Etf' },
    { label: 'Other Mandates', value: 'Other' }
  ];
  selectedType: RulePackageType = 'Tax';
  packages: RulePackage[] = [];
  versions: RulePackageVersion[] = [];
  selectedPackageId: string | null = null;
  isLoadingPackages = false;
  isLoadingVersions = false;

  columns: DataTableColumn<RulePackageVersion>[] = [
    { field: 'versionNumber', header: 'Manifest v#', sortable: true },
    { field: 'effectiveFrom', header: 'Enforcement Start', type: 'date', sortable: true },
    { field: 'effectiveTo', header: 'Enforcement End', type: 'date', sortable: true },
    { field: 'createdAt', header: 'Initialized At', type: 'datetime', sortable: true },
    { field: 'status', header: 'Current State', type: 'badge' }
  ];

  packageForm = { name: '', companyId: '' };
  versionForm = { effectiveFrom: '', effectiveTo: '', contentJson: '' };

  constructor(
    private rulePackagesApi: RulePackagesApiService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadPackages();
  }

  loadPackages(): void {
    this.isLoadingPackages = true;
    this.rulePackagesApi.getPackages({ type: this.selectedType }).subscribe({
      next: packages => {
        this.packages = packages;
        this.isLoadingPackages = false;
        if (!this.selectedPackageId && packages.length > 0) {
          this.selectedPackageId = packages[0].id;
          this.loadVersions();
        }
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Synchronization Failure',
          detail: 'Failed to synchronize institutional rule manifest.'
        });
        this.isLoadingPackages = false;
      },
    });
  }

  onTypeChange(): void {
    this.selectedPackageId = null;
    this.versions = [];
    this.loadPackages();
  }

  selectPackage(packageId: string): void {
    this.selectedPackageId = packageId;
    this.loadVersions();
  }

  loadVersions(): void {
    if (!this.selectedPackageId) return;

    this.isLoadingVersions = true;
    this.rulePackagesApi.getVersions(this.selectedPackageId).subscribe({
      next: versions => {
        this.versions = versions.map(v => ({ ...v, isActive: v.status === 'Active' } as any));
        this.isLoadingVersions = false;
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Version Retrieval Error',
          detail: 'Failed to access version ledger for the selected package.'
        });
        this.isLoadingVersions = false;
      },
    });
  }

  createPackage(): void {
    const name = this.packageForm.name.trim();
    const companyId = this.packageForm.companyId.trim();
    if (!name || !companyId) {
      this.messageService.add({ severity: 'warn', summary: 'Input Required', detail: 'Mandate name and Institutional ID are mandatory.' });
      return;
    }

    this.rulePackagesApi
      .createPackage({ name, companyId, ruleType: this.selectedType })
      .subscribe({
        next: created => {
          this.packages = [created, ...this.packages];
          this.packageForm = { name: '', companyId: companyId };
          this.selectedPackageId = created.id;
          this.messageService.add({ severity: 'success', summary: 'Mandate Initialized', detail: `Governance package '${name}' established.` });
          this.loadVersions();
        },
        error: err => {
          this.messageService.add({ severity: 'error', summary: 'Initialization Error', detail: err.error?.message || 'Failed to establish rule mandate.' });
        },
      });
  }

  createVersion(): void {
    if (!this.selectedPackageId) {
      this.messageService.add({ severity: 'warn', summary: 'Context Required', detail: 'Select a rule mandate before establishing a version.' });
      return;
    }

    const effectiveFrom = this.versionForm.effectiveFrom.trim();
    const contentJson = this.versionForm.contentJson.trim();
    if (!effectiveFrom || !contentJson) {
      this.messageService.add({ severity: 'warn', summary: 'Input Required', detail: 'Temporal enforcement start and JSON manifest are required.' });
      return;
    }

    this.rulePackagesApi
      .createVersion(this.selectedPackageId, {
        effectiveFrom,
        effectiveTo: this.versionForm.effectiveTo?.trim() || null,
        contentJson,
      })
      .subscribe({
        next: version => {
          this.versions = [version, ...this.versions];
          this.versionForm = { effectiveFrom: '', effectiveTo: '', contentJson: '' };
          this.messageService.add({ severity: 'success', summary: 'Version Manifested', detail: 'New rule version has been appended to the ledger.' });
        },
        error: err => {
          this.messageService.add({ severity: 'error', summary: 'Manifest Error', detail: err.error?.message || 'Failed to manifest new rule version.' });
        },
      });
  }

  activateVersion(version: RulePackageVersion): void {
    if (!this.selectedPackageId) return;

    this.rulePackagesApi.activateVersion(this.selectedPackageId, version.id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Policy Enforced', detail: `Version ${version.versionNumber} is now the primary mandate.` });
        this.loadVersions();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Enforcement Failure', detail: err.error?.message || 'Failed to enforce the selected version.' });
      },
    });
  }
}
