export type LeaveRequest = {
  id: string;
  employeeId: string;
  leaveType: string;
  status: string;
  reason?: string;
};
