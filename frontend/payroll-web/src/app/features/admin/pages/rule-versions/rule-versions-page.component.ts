import { Component, OnInit } from '@angular/core';
import { RulePackage, RulePackageType, RulePackageVersion } from '../../models/rule-package.model';
import { RulePackagesApiService } from '../../services/rule-packages-api.service';

@Component({
  selector: 'app-rule-versions-page',
  templateUrl: './rule-versions-page.component.html',
  styleUrls: ['./rule-versions-page.component.scss'],
})
export class RuleVersionsPageComponent implements OnInit {
  ruleTypes: RulePackageType[] = ['Tax', 'Epf', 'Etf', 'Other'];
  selectedType: RulePackageType = 'Tax';
  packages: RulePackage[] = [];
  versions: RulePackageVersion[] = [];
  selectedPackageId: string | null = null;
  isLoadingPackages = false;
  isLoadingVersions = false;
  errorMessage: string | null = null;

  packageForm = { name: '', companyId: '' };
  versionForm = { effectiveFrom: '', effectiveTo: '', contentJson: '' };

  constructor(private rulePackagesApi: RulePackagesApiService) {}

  ngOnInit(): void {
    this.loadPackages();
  }

  loadPackages(): void {
    this.isLoadingPackages = true;
    this.errorMessage = null;
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
        console.error('Failed to load rule packages', err);
        this.errorMessage = err.error?.message || 'Failed to load rule packages.';
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
    if (!this.selectedPackageId) {
      return;
    }

    this.isLoadingVersions = true;
    this.errorMessage = null;
    this.rulePackagesApi.getVersions(this.selectedPackageId).subscribe({
      next: versions => {
        this.versions = versions;
        this.isLoadingVersions = false;
      },
      error: err => {
        console.error('Failed to load rule versions', err);
        this.errorMessage = err.error?.message || 'Failed to load rule versions.';
        this.isLoadingVersions = false;
      },
    });
  }

  createPackage(): void {
    const name = this.packageForm.name.trim();
    const companyId = this.packageForm.companyId.trim();
    if (!name || !companyId) {
      this.errorMessage = 'Package name and company ID are required.';
      return;
    }

    this.rulePackagesApi
      .createPackage({ name, companyId, ruleType: this.selectedType })
      .subscribe({
        next: created => {
          this.packages = [created, ...this.packages];
          this.packageForm = { name: '', companyId: companyId };
          this.selectedPackageId = created.id;
          this.loadVersions();
        },
        error: err => {
          console.error('Failed to create rule package', err);
          this.errorMessage = err.error?.message || 'Failed to create rule package.';
        },
      });
  }

  createVersion(): void {
    if (!this.selectedPackageId) {
      this.errorMessage = 'Select a rule package before adding a version.';
      return;
    }

    const effectiveFrom = this.versionForm.effectiveFrom.trim();
    const contentJson = this.versionForm.contentJson.trim();
    if (!effectiveFrom || !contentJson) {
      this.errorMessage = 'Effective from date and JSON content are required.';
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
        },
        error: err => {
          console.error('Failed to create rule version', err);
          this.errorMessage = err.error?.message || 'Failed to create rule version.';
        },
      });
  }

  activateVersion(version: RulePackageVersion): void {
    if (!this.selectedPackageId) {
      return;
    }

    if (!confirm(`Activate version ${version.versionNumber}?`)) {
      return;
    }

    this.rulePackagesApi.activateVersion(this.selectedPackageId, version.id).subscribe({
      next: () => {
        this.loadVersions();
      },
      error: err => {
        console.error('Failed to activate rule version', err);
        this.errorMessage = err.error?.message || 'Failed to activate rule version.';
      },
    });
  }
}
