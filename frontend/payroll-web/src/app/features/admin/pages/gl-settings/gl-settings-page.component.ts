import { Component, OnInit } from '@angular/core';
import { GlApiService } from '../../../payroll/services/gl-api.service';
import { GlAccount, GlAccountType, GlMappingEntry, GlPostingSideRule } from '../../../payroll/models/gl.model';

@Component({
  selector: 'app-gl-settings-page',
  templateUrl: './gl-settings-page.component.html',
  styleUrls: ['./gl-settings-page.component.scss'],
})
export class GlSettingsPageComponent implements OnInit {
  accounts: GlAccount[] = [];
  mappings: GlMappingEntry[] = [];
  isLoadingAccounts = false;
  isLoadingMappings = false;
  errorMessage: string | null = null;
  accountForm: Partial<GlAccount> = { type: 'Expense', isActive: true };
  editingAccountId: string | null = null;
  accountTypes: GlAccountType[] = ['Asset', 'Liability', 'Expense', 'Equity', 'Revenue'];
  postingRules: GlPostingSideRule[] = ['DebitWhenPositive', 'CreditWhenPositive'];

  constructor(private glApi: GlApiService) {}

  ngOnInit(): void {
    this.loadAccounts();
    this.loadMappings();
  }

  loadAccounts(): void {
    this.isLoadingAccounts = true;
    this.glApi.getAccounts().subscribe({
      next: accounts => {
        this.accounts = accounts;
        this.isLoadingAccounts = false;
      },
      error: err => {
        console.error('Failed to load GL accounts', err);
        this.errorMessage = err.error?.message || 'Failed to load GL accounts.';
        this.isLoadingAccounts = false;
      },
    });
  }

  loadMappings(): void {
    this.isLoadingMappings = true;
    this.glApi.getMappings().subscribe({
      next: mappings => {
        this.mappings = mappings;
        this.isLoadingMappings = false;
      },
      error: err => {
        console.error('Failed to load GL mappings', err);
        this.errorMessage = err.error?.message || 'Failed to load GL mappings.';
        this.isLoadingMappings = false;
      },
    });
  }

  editAccount(account: GlAccount): void {
    this.editingAccountId = account.id;
    this.accountForm = { ...account };
  }

  resetAccountForm(): void {
    this.editingAccountId = null;
    this.accountForm = { type: 'Expense', isActive: true };
  }

  saveAccount(): void {
    if (!this.accountForm.code || !this.accountForm.name || !this.accountForm.type) {
      this.errorMessage = 'Account code, name, and type are required.';
      return;
    }

    this.glApi.upsertAccount({ id: this.editingAccountId ?? undefined, ...this.accountForm }).subscribe({
      next: () => {
        this.resetAccountForm();
        this.loadAccounts();
        this.loadMappings();
      },
      error: err => {
        console.error('Failed to save GL account', err);
        this.errorMessage = err.error?.message || 'Failed to save GL account.';
      },
    });
  }

  deleteAccount(account: GlAccount): void {
    if (!confirm(`Delete account ${account.code}?`)) {
      return;
    }

    this.glApi.deleteAccount(account.id).subscribe({
      next: () => {
        this.loadAccounts();
        this.loadMappings();
      },
      error: err => {
        console.error('Failed to delete GL account', err);
        this.errorMessage = err.error?.message || 'Failed to delete GL account.';
      },
    });
  }

  saveMapping(mapping: GlMappingEntry): void {
    this.glApi
      .upsertMapping({
        id: mapping.id,
        payComponentCode: mapping.payComponentCode,
        payComponentType: mapping.payComponentType,
        debitAccountId: mapping.debitAccountId,
        creditAccountId: mapping.creditAccountId,
        postingSideRule: mapping.postingSideRule,
        costCenterId: mapping.costCenterId,
        notes: mapping.notes,
      })
      .subscribe({
        next: updated => {
          const index = this.mappings.findIndex(entry => entry.payComponentCode === updated.payComponentCode && entry.payComponentType === updated.payComponentType && entry.costCenterId === updated.costCenterId);
          if (index >= 0) {
            this.mappings[index] = updated;
          }
        },
        error: err => {
          console.error('Failed to save GL mapping', err);
          this.errorMessage = err.error?.message || 'Failed to save GL mapping.';
        },
      });
  }
}
