export type PayslipDocumentStatus = 'Pending' | 'Generated' | 'Failed';

export interface PayslipDocument {
  id: string;
  payRunId: string;
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  status: PayslipDocumentStatus;
  generatedAtUtc?: string | null;
  generatedByUserId?: string | null;
  generatedByUserName?: string | null;
  fileName?: string | null;
  checksumSha256?: string | null;
  errorSummary?: string | null;
}

export interface PayslipBulkGenerateResult {
  generatedCount: number;
  skippedCount: number;
  failedCount: number;
}
