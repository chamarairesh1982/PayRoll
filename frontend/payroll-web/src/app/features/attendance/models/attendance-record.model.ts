export type AttendanceStatus = 'Present' | 'Absent' | 'Leave' | 'HalfDay';

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeCode?: string;
  employeeName?: string;
  date: string;
  inTime?: string | null;
  outTime?: string | null;
  totalHours?: number | null;
  isLate?: boolean | null;
  otHours?: number | null;
  status: AttendanceStatus;
  remarks?: string | null;
  isLocked?: boolean | null;
}
