export interface PaySlipEarningLine {
  id: string;
  code: string;
  description: string;
  amount: number;
  isEpfApplicable: boolean;
  isEtfApplicable: boolean;
  isTaxable: boolean;
}

export interface PaySlipDeductionLine {
  id: string;
  code: string;
  description: string;
  source: string;
  amount: number;
  isPreTax: boolean;
  isPostTax: boolean;
}

export interface TaxCalculationBreakdownLine {
  bandFrom: number;
  bandTo?: number | null;
  rate: number;
  taxableInBand: number;
  taxForBand: number;
}

export interface TaxCalculationSummary {
  slabSetId?: string | null;
  taxableEarnings: number;
  reliefTotal: number;
  taxableBase: number;
  tax: number;
  breakdown: TaxCalculationBreakdownLine[];
}

export interface PaySlip {
  id: string;
  payRunId: string;
  employeeId: string;
  employeeCode?: string;
  employeeName?: string;

  basicSalary: number;
  totalEarnings: number;
  totalDeductions: number;
  netPay: number;

  employeeEpf: number;
  employerEpf: number;
  employerEtf: number;
  payeTax: number;
  taxCalculation?: TaxCalculationSummary | null;

  currency: string;

  earnings: PaySlipEarningLine[];
  deductions: PaySlipDeductionLine[];
}
