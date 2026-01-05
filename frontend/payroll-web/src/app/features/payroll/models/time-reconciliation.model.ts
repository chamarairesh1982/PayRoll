export interface TimeReconciliationResult {
  payRunId: string;
  periodStart: string;
  periodEnd: string;
  employees: TimeReconciliationEmployeeSummary[];
  conflicts: TimeReconciliationConflict[];
}

export interface TimeReconciliationEmployeeSummary {
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  workedDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  absentDays: number;
  noPayDays: number;
  noPayHours: number;
}

export interface TimeReconciliationConflict {
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  date: string;
  warning: string;
}
