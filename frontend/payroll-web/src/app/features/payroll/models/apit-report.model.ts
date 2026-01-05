import { PayPeriodType } from './pay-run.model';

export type TaxReliefType = 'IncomeRelief' | 'TaxRebate';
export type TaxReliefFrequency = 'Monthly' | 'Annual';

export interface ApitReportEmployee {
  paySlipId: string;
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  taxableEarnings: number;
  preTaxDeductions: number;
  reliefAmount: number;
  rebateAmount: number;
  taxableAfterRelief: number;
  apitWithheld: number;
  yearToDateApit: number;
}

export interface ApitReport {
  payRunId: string;
  payRunCode: string;
  payRunName: string;
  periodStart: string;
  periodEnd: string;
  payDate: string;
  periodType: PayPeriodType;
  employeeCount: number;
  totalTaxForPeriod: number;
  totalTaxYearToDate: number;
  employees: ApitReportEmployee[];
}

export interface FileExportResult {
  fileName: string;
  contentType: string;
  contentBase64: string;
}
