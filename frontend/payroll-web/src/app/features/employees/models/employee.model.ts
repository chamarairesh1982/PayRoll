export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  initials?: string | null;
  callingName?: string | null;
  nicNumber: string;
  dateOfBirth: string;
  gender: 'Male' | 'Female' | 'Other';
  maritalStatus: 'Single' | 'Married' | 'Other';
  employmentStartDate: string;
  probationEndDate?: string | null;
  confirmationDate?: string | null;
  baseSalary: number;
  isActive: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
