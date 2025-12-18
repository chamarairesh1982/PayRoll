import { PaySlip } from './payslip.model';

export type BankExportStatus = 'Pending' | 'Generated' | 'Downloaded';

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
  statusHistory?: PayRunStatusHistory[];
  preparedAt?: string | null;
  preparedByUserName?: string | null;
  approvedAt?: string | null;
  approvedByUserName?: string | null;
  lockedAt?: string | null;
  lockedByUserName?: string | null;
}

export interface BankExportFailure {
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  reason: string;
}

export interface BankExportResult {
  status: BankExportStatus;
  bank: string;
  fileName: string;
  contentType: string;
  contentBase64: string;
  failures: BankExportFailure[];
}

export interface PayRunStatusHistory {
  id: string;
  fromStatus: PayRunStatus;
  toStatus: PayRunStatus;
  actorUserName: string;
  actorUserId?: string | null;
  comment?: string | null;
  actionedAt: string;
}
