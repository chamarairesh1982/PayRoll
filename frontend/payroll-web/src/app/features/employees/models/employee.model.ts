export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  initials?: string | null;
  callingName?: string | null;
  nicNumber: string;
  maskedNicNumber?: string | null;
  epfNumber?: string | null;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  maritalStatus: 'Single' | 'Married' | 'Other';
  employmentStartDate: string;
  probationEndDate?: string | null;
  confirmationDate?: string | null;
  baseSalary: number;
  hourlyRate?: number | null;
  bankName?: string | null;
  bankCode?: string | null;
  branchCode?: string | null;
  bankAccountNumber?: string | null;
  maskedBankAccountNumber?: string | null;
  companyId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  branchName?: string | null;
  costCenterName?: string | null;
  isActive: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
