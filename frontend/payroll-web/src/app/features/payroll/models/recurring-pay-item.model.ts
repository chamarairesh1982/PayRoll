export type RecurringPayItemType = 'Allowance' | 'Deduction';
export type RecurringFrequency = 'Monthly' | 'Weekly' | 'Custom';

export interface RecurringPayItemRule {
  id: string;
  name: string;
  ruleType: RecurringPayItemType;
  payComponentId: string;
  payComponentCode: string;
  payComponentName: string;
  frequency: RecurringFrequency;
  startDate: string;
  endDate?: string | null;
  amount: number;
  taxable: boolean;
  epfEtfContributable: boolean;
  prorate: boolean;
  isActive: boolean;
}

export interface RecurringPayItemAssignment {
  id: string;
  ruleId: string;
  ruleName: string;
  employeeId: string;
  employeeName: string;
  startDate: string;
  endDate?: string | null;
  isActive: boolean;
}

export interface RecurringPayItemSimulationItem {
  ruleId: string;
  assignmentId: string;
  name: string;
  ruleType: RecurringPayItemType;
  payComponentCode: string;
  payComponentName: string;
  effectiveStart: string;
  effectiveEnd: string;
  amount: number;
  prorated: boolean;
}

export interface RecurringPayItemSimulationResponse {
  items: RecurringPayItemSimulationItem[];
  totalAllowances: number;
  totalDeductions: number;
}
