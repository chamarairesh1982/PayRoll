import { PaySlip } from './payslip.model';

export type PayRunStatus = 'Draft' | 'Calculated' | 'UnderReview' | 'Approved' | 'Posted' | 'Cancelled';

export type PayPeriodType = 'Monthly' | 'Weekly' | 'Custom';

export interface PayRunSummary {
  id: string;
  code: string;
  name: string;
  periodType: PayPeriodType;
  periodStart: string;
  periodEnd: string;
  payDate: string;
  status: PayRunStatus;
  isLocked: boolean;
  employeeCount: number;
  totalNetPay: number;
}

export interface PayRunDetail extends PayRunSummary {
  paySlips: PaySlip[];
}
