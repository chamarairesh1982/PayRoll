export type RecurringRuleType = 'Allowance' | 'Deduction' | 'Loan installment' | 'Overtime rule';
export type RecurringAmountType = 'Fixed' | 'Percentage';
export type RecurringFrequency = 'Monthly' | 'Weekly' | 'Fortnightly';
export type RecurringScopeType = 'All' | 'Group' | 'Selected';
export type RecurringRuleStatus = 'Active' | 'Inactive';

export interface RecurringRuleScope {
  type: RecurringScopeType;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  employeeCategory?: string | null;
  employeeIds?: string[];
}

export interface RecurringRule {
  id: string;
  name: string;
  type: RecurringRuleType;
  amountType: RecurringAmountType;
  amountValue: number;
  frequency: RecurringFrequency;
  startDate: string;
  endDate?: string | null;
  status: RecurringRuleStatus;
  priority: number;
  scope: RecurringRuleScope;
}

export interface RecurringRuleFormValue {
  name: string;
  type: RecurringRuleType;
  amountType: RecurringAmountType;
  amountValue: number;
  frequency: RecurringFrequency;
  startDate: string;
  endDate?: string | null;
  status: RecurringRuleStatus;
  priority: number;
  scopeType: RecurringScopeType;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  employeeCategory?: string | null;
  employeeIds?: string[];
}
