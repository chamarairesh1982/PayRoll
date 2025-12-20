export type GeneratedTaxDocumentType = 'MonthlyReport' | 'AnnualReport' | 'EmployeeCertificate';
export type GeneratedTaxDocumentStatus = 'Pending' | 'Generated' | 'Failed';

export interface TaxDocumentMetadata {
  id: string;
  type: GeneratedTaxDocumentType;
  periodStart?: string;
  periodEnd?: string;
  year?: number;
  employeeId?: string | null;
  status: GeneratedTaxDocumentStatus;
  generatedAtUtc?: string;
  fileName?: string | null;
  checksumSha256?: string | null;
  downloadUrl?: string | null;
  errorSummary?: string | null;
}

export interface TaxDocumentHistory {
  id: string;
  type: GeneratedTaxDocumentType;
  periodStart?: string;
  periodEnd?: string;
  year?: number;
  employeeId?: string | null;
  employeeCode?: string | null;
  employeeName?: string | null;
  status: GeneratedTaxDocumentStatus;
  generatedAtUtc?: string;
  fileName?: string | null;
}

export interface MonthlyTaxReportRequest {
  year: number;
  month: number;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  format: 'csv' | 'pdf';
}

export interface AnnualTaxReportRequest {
  year: number;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  format: 'csv' | 'pdf';
}

export interface TaxCertificateRequest {
  employeeId: string;
  year: number;
}
