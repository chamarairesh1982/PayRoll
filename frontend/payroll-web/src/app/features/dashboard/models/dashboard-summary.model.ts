export interface DashboardSummary {
  kpis: DashboardKpis;
  health: DashboardHealth;
  recentActivity: DashboardActivity[];
  trends?: DashboardTrends;
  intelligence?: DashboardIntelligence;
}

export interface DashboardKpis {
  nextPayDate?: string | null;
  employeeCount: number;
  latestPayRunStatus: string;
  exceptionsCount: number;
  totalGrossPay?: number;
  totalNetPay?: number;
  totalTax?: number;
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

export interface DashboardTrends {
  payrollCosts: TrendDataPoint[];
  headcount: TrendDataPoint[];
}

export interface TrendDataPoint {
  label: string;
  value: number;
  secondary?: number;
}

export interface DashboardIntelligence {
  departmentDistribution: DistributionDataPoint[];
  costCenterDistribution: DistributionDataPoint[];
  statutoryBreakdown: DistributionDataPoint[];
}

export interface DistributionDataPoint {
  label: string;
  value: number;
  percentage?: number;
}

export interface DashboardSummaryQuery {
  periodStart?: string;
  periodEnd?: string;
  companyId?: string;
  branchId?: string;
  costCenterId?: string;
}
