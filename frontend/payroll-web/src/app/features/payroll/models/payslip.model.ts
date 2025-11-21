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
  amount: number;
  isPreTax: boolean;
  isPostTax: boolean;
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

  currency: string;

  earnings: PaySlipEarningLine[];
  deductions: PaySlipDeductionLine[];
}
