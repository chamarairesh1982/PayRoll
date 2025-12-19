export type StatutoryReportType = 'EpfEtf';

export type StatutoryReportStatus = 'Generated' | 'Failed';

export interface EpfEtfReportWarning {
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  message: string;
}

export interface EpfEtfReportEmployee {
  paySlipId: string;
  employeeId: string;
  employeeCode?: string | null;
  employeeName?: string | null;
  nicNumber?: string | null;
  epfNumber?: string | null;
  contributableBase: number;
  employeeEpf: number;
  employerEpf: number;
  employerEtf: number;
}

export interface FileExportResult {
  fileName: string;
  contentType: string;
  contentBase64: string;
}

export interface EpfEtfReportResult {
  reportId: string;
  payRunId: string;
  payRunCode: string;
  payRunName: string;
  periodStart: string;
  periodEnd: string;
  payDate: string;
  employeeCount: number;
  contributableBase: number;
  employeeEpfTotal: number;
  employerEpfTotal: number;
  employerEtfTotal: number;
  file: FileExportResult;
  warningFile?: FileExportResult | null;
  employees: EpfEtfReportEmployee[];
  warnings: EpfEtfReportWarning[];
}

export interface StatutoryReportHistory {
  id: string;
  type: StatutoryReportType;
  payRunId: string;
  payRunCode: string;
  payRunName: string;
  periodStart: string;
  periodEnd: string;
  generatedAtUtc: string;
  generatedBy: string;
  status: StatutoryReportStatus;
  fileName: string;
  warningCount: number;
}
