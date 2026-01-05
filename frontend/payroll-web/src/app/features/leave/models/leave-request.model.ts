export type LeaveStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled';

export type LeaveTypeCode =
  | 'ANNUAL'
  | 'CASUAL'
  | 'SICK'
  | 'MATERNITY'
  | 'NOPAY'
  | 'OTHER';

export interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeCode?: string;
  employeeName?: string;
  leaveType: LeaveTypeCode;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason?: string | null;
  status: LeaveStatus;
  approvedById?: string | null;
  approvedByName?: string | null;
  requestedAt: string;
  approvedAt?: string | null;
  isHalfDay?: boolean | null;
  halfDaySession?: 'AM' | 'PM' | null;
}
