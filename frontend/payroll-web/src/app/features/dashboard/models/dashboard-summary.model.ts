export interface DashboardSummary {
  kpis: DashboardKpis;
  health: DashboardHealth;
  recentActivity: DashboardActivity[];
}

export interface DashboardKpis {
  nextPayDate?: string | null;
  employeeCount: number;
  latestPayRunStatus: string;
  exceptionsCount: number;
}

export interface DashboardHealth {
  missingEpf: number;
  missingBank: number;
  pendingAttendance: number;
  pendingOt: number;
  negativeNet: number;
}

export interface DashboardActivity {
  id?: string;
  occurredAt: string;
  activity: string;
  actor: string;
  context: string;
}

export interface DashboardSummaryQuery {
  periodStart?: string;
  periodEnd?: string;
  companyId?: string;
  branchId?: string;
  costCenterId?: string;
}
