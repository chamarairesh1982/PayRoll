export type LoanStatus = 'Pending' | 'Active' | 'Disbursed' | 'Settled' | 'Defaulted';
export type LoanType = 'Company Loan' | 'Salary Advance' | 'Personal Loan' | 'Emergency Relief';

export interface Loan {
  id: string;
  employeeId: string;
  employeeName: string;
  loanType: LoanType;
  principal: number;
  interestRate: number;
  totalRepayable: number;
  repaidAmount: number;
  outstanding: number;
  installmentAmount: number;
  remainingInstallments: number;
  disbursementDate: string;
  nextInstallmentDate: string;
  status: LoanStatus;
}
