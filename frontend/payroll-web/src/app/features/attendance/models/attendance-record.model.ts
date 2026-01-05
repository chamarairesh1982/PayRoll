export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName?: string | null;
  hoursWorked: number;
  periodStart: string;
  periodEnd: string;
}
