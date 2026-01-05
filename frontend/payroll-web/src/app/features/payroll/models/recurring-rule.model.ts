export type RecurringRuleType = 'Allowance' | 'Deduction';
export type RecurringFrequency = 'Monthly' | 'Weekly' | 'Custom';

export interface RecurringRule {
  id: string;
  code: string;
  name: string;
  ruleType: RecurringRuleType;
  frequency: RecurringFrequency;
  startDate: string;
  endDate?: string | null;
  amount: number;
  employeeId: string;
  employeeName?: string;
  isEpfApplicable: boolean;
  isEtfApplicable: boolean;
  isTaxable: boolean;
  isActive: boolean;
}

export interface RecurringRuleSimulationRequest {
  rule: Partial<RecurringRule> & { frequency: RecurringFrequency; ruleType: RecurringRuleType; startDate: string; amount: number; code: string; name: string; employeeId: string };
  periods: number;
  startFrom?: string;
}

export interface RecurringRuleSimulationResult {
  periodStart: string;
  periodEnd: string;
  amount: number;
}
