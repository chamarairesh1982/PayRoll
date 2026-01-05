export type ReportKey = 'epf-etf' | 'paye' | 'payroll-register' | 'bank-transfer' | 'employer-cost';

export interface ReportColumn {
  field: string;
  header: string;
  type?: 'text' | 'number' | 'currency';
  align?: 'left' | 'right' | 'center';
}

export interface ReportRow {
  [key: string]: string | number | null | undefined;
}

export interface ReportDefinition {
  key: ReportKey;
  title: string;
  description: string;
  cadence: string;
  groupBy?: string;
  groupByLabel?: string;
  columns: ReportColumn[];
}

export interface ReportFilters {
  companyId: string | null;
  branchId: string | null;
  costCenterId: string | null;
  payrollMonth: Date | null;
  employeeStatus: 'active' | 'all';
  outputMode: 'preview' | 'export';
  outputFormat: 'csv' | 'excel' | 'pdf';
}

export interface ReportResult {
  key: ReportKey;
  title: string;
  generatedOn: Date;
  generatedBy: string;
  filtersUsed: Record<string, string>;
  isPeriodClosed: boolean;
  warnings: string[];
  groupBy?: string;
  groupByLabel?: string;
  columns: ReportColumn[];
  rows: ReportRow[];
}

export interface FilterOption {
  label: string;
  value: string;
}

export interface ReportFilterOptions {
  companies: FilterOption[];
  branches: FilterOption[];
  costCenters: FilterOption[];
  employeeStatuses: FilterOption[];
  outputModes: FilterOption[];
  outputFormats: FilterOption[];
}
