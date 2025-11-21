export type OvertimeType = 'Weekday' | 'Weekend' | 'PublicHoliday';

export type OvertimeStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';

export interface OvertimeRecord {
  id: string;

  employeeId: string;
  employeeCode?: string;
  employeeName?: string;

  date: string;

  hours: number;

  type: OvertimeType;

  status: OvertimeStatus;

  reason?: string | null;

  approvedById?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;

  isLockedForPayroll: boolean;
}
