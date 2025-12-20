import { BankExportStatus } from './bank-export.model';
import { PaySlip } from './payslip.model';

export type PayRunStatus = 'Draft' | 'Prepared' | 'Approved' | 'Locked';

export type PayPeriodType = 'Monthly' | 'Weekly' | 'Custom';

export interface PayRunSummary {
  id: string;
  code: string;
  name: string;
  periodType: PayPeriodType;
  periodStart: string;
  periodEnd: string;
  payDate: string;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  isConsolidated: boolean;
  status: PayRunStatus;
  exportStatus: BankExportStatus;
  exportedBank?: string | null;
  exportedAt?: string | null;
  exportDownloadedAt?: string | null;
  isLocked: boolean;
  employeeCount: number;
  totalNetPay: number;
}

export interface PayRunDetail extends PayRunSummary {
  paySlips: PaySlip[];
  attendanceNoPaySummaries: AttendanceNoPaySummary[];
  statusHistory?: PayRunStatusHistory[];
  preparedAt?: string | null;
  preparedByUserName?: string | null;
  approvedAt?: string | null;
  approvedByUserName?: string | null;
  lockedAt?: string | null;
  lockedByUserName?: string | null;
}

export interface AttendanceNoPaySummary {
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  noPayAmount: number;
}

export interface PayRunStatusHistory {
  id: string;
  fromStatus: PayRunStatus;
  toStatus: PayRunStatus;
  actorDisplayName?: string | null;
  actorUserId?: string | null;
  comment?: string | null;
  timestampUtc: string;
}
