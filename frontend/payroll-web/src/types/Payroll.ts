export type PayRun = {
  id: string;
  reference: string;
  periodStart: string;
  periodEnd: string;
  status: string;
};

export type PaySlip = {
  id: string;
  employeeId: string;
  netPay: number;
};
