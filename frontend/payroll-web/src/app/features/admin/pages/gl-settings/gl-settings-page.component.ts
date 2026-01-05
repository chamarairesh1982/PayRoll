import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';
import { GlApiService } from '../../../payroll/services/gl-api.service';
import { GlAccount, GlAccountType, GlMappingEntry, GlPostingSideRule } from '../../../payroll/models/gl.model';

@Component({
  selector: 'app-gl-settings-page',
  templateUrl: './gl-settings-page.component.html',
  styleUrls: ['./gl-settings-page.component.scss'],
  providers: [MessageService]
})
export class GlSettingsPageComponent implements OnInit {
  accounts: GlAccount[] = [];
  mappings: GlMappingEntry[] = [];
  isLoadingAccounts = false;
  isLoadingMappings = false;

  accountColumns: DataTableColumn<GlAccount>[] = [
    { field: 'code', header: 'Account Code', sortable: true },
    { field: 'name', header: 'Description', sortable: true },
    { field: 'type', header: 'Category', sortable: true },
    { field: 'isActive', header: 'Status', type: 'boolean' }
  ];

  mappingColumns: DataTableColumn<GlMappingEntry>[] = [
    { field: 'payComponentCode', header: 'Component', sortable: true },
    { field: 'payComponentType', header: 'Type', sortable: true },
    { field: 'debitAccountId', header: 'Debit A/C' },
    { field: 'creditAccountId', header: 'Credit A/C' },
    { field: 'postingSideRule', header: 'Posting Logic' }
  ];

  accountForm: Partial<GlAccount> = { type: 'Expense', isActive: true };
  editingAccountId: string | null = null;
  accountTypes: GlAccountType[] = ['Asset', 'Liability', 'Expense', 'Equity', 'Revenue'];
  postingRules: GlPostingSideRule[] = ['DebitWhenPositive', 'CreditWhenPositive'];
  showAccountDialog = false;

  constructor(
    private glApi: GlApiService,
    private messageService: MessageService
  ) { }

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
        this.messageService.add({ severity: 'error', summary: 'Retrieval Error', detail: 'Failed to synchronize institutional chart of accounts.' });
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
        this.messageService.add({ severity: 'error', summary: 'Mapping Error', detail: 'Failed to synchronize component-to-ledger mappings.' });
        this.isLoadingMappings = false;
      },
    });
  }

  initNewAccount(): void {
    this.resetAccountForm();
    this.showAccountDialog = true;
  }

  editAccount(account: GlAccount): void {
    this.editingAccountId = account.id;
    this.accountForm = { ...account };
    this.showAccountDialog = true;
  }

  resetAccountForm(): void {
    this.editingAccountId = null;
    this.accountForm = { type: 'Expense', isActive: true };
  }

  saveAccount(): void {
    if (!this.accountForm.code || !this.accountForm.name || !this.accountForm.type) {
      this.messageService.add({ severity: 'warn', summary: 'Input Required', detail: 'Account identifier, name, and category are mandatory.' });
      return;
    }

    this.glApi.upsertAccount({ id: this.editingAccountId ?? undefined, ...this.accountForm }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Account persistence', detail: 'Fiscal account has been successfully synchronized.' });
        this.showAccountDialog = false;
        this.resetAccountForm();
        this.loadAccounts();
        this.loadMappings();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Persistence Failure', detail: err.error?.message || 'Failed to persist fiscal account.' });
      },
    });
  }

  deleteAccount(account: GlAccount): void {
    this.glApi.deleteAccount(account.id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'warn', summary: 'Account Removed', detail: `Account ${account.code} has been decommissioned.` });
        this.loadAccounts();
        this.loadMappings();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Revocation Error', detail: err.error?.message || 'Failed to decommission account.' });
      },
    });
  }

  getAccountLabel(id: string): string {
    const acc = this.accounts.find(a => a.id === id);
    return acc ? `${acc.code} - ${acc.name}` : id;
  }
}
