export type GlAccountType = 'Asset' | 'Liability' | 'Expense' | 'Equity' | 'Revenue';
export type GlPayComponentType = 'Earning' | 'Deduction' | 'EmployerContribution';
export type GlPostingSideRule = 'DebitWhenPositive' | 'CreditWhenPositive';
export type GlJournalBatchStatus = 'Draft' | 'Generated' | 'Approved' | 'Exported' | 'Failed';

export interface GlAccount {
  id: string;
  code: string;
  name: string;
  type: GlAccountType;
  isActive: boolean;
}

export interface GlMappingEntry {
  id?: string | null;
  payComponentCode: string;
  payComponentName: string;
  payComponentType: GlPayComponentType;
  debitAccountId?: string | null;
  debitAccountCode?: string | null;
  debitAccountName?: string | null;
  creditAccountId?: string | null;
  creditAccountCode?: string | null;
  creditAccountName?: string | null;
  postingSideRule: GlPostingSideRule;
  costCenterId?: string | null;
  costCenterCode?: string | null;
  costCenterName?: string | null;
  notes?: string | null;
}

export interface GlJournalLine {
  id: string;
  postingDate: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  description: string;
  debitAmount: number;
  creditAmount: number;
  employeeId?: string | null;
  payComponentCode?: string | null;
  payComponentType?: GlPayComponentType | null;
  costCenterId?: string | null;
  costCenterCode?: string | null;
  branchId?: string | null;
  branchCode?: string | null;
  reference: string;
}

export interface GlJournalBatchSummary {
  id: string;
  payRunId: string;
  status: GlJournalBatchStatus;
  generatedAtUtc?: string | null;
  generatedByUserName?: string | null;
  approvedAtUtc?: string | null;
  approvedByUserName?: string | null;
  exportedAtUtc?: string | null;
  exportedByUserName?: string | null;
  notes?: string | null;
  totalDebits: number;
  totalCredits: number;
  isBalanced: boolean;
}

export interface GlJournalBatchDetail extends GlJournalBatchSummary {
  lines: GlJournalLine[];
}

export interface GlBatchActionRequest {
  comment?: string | null;
}
