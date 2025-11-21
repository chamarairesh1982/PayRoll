export type Employee = {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  initials?: string;
  callingName?: string;
  nicNumber: string;
  dateOfBirth: string;
  gender: "Male" | "Female" | "Other";
  maritalStatus: "Single" | "Married" | "Other";
  employmentStartDate: string;
  probationEndDate?: string | null;
  confirmationDate?: string | null;
  baseSalary: number;
  isActive: boolean;
};
