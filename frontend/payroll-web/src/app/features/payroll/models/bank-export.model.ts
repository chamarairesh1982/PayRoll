export type BankExportStatus = 'Pending' | 'Generated' | 'Downloaded' | 'Failed';

export type BankExportFormat = 'Csv' | 'Txt' | 'FixedWidth';

export interface BankExportTemplate {
  id: string;
  name: string;
  format: BankExportFormat;
  isActive: boolean;
  delimiter?: string | null;
  headerRowCount: number;
}

export interface BankExportValidationError {
  employeeId?: string | null;
  employeeCode?: string | null;
  field: string;
  message: string;
}

export interface PayRunBankExport {
  id: string;
  payRunId: string;
  templateId: string;
  templateName: string;
  status: BankExportStatus;
  generatedAtUtc?: string | null;
  generatedByUserId?: string | null;
  generatedByUserName?: string | null;
  downloadedAtUtc?: string | null;
  downloadedByUserId?: string | null;
  downloadedByUserName?: string | null;
  fileName?: string | null;
  checksumSha256?: string | null;
  errorSummary?: string | null;
  createdAtUtc: string;
  errorCount: number;
}

export interface BankExportGenerateResult {
  export: PayRunBankExport;
  validationErrors: BankExportValidationError[];
}
