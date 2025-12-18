export interface OvertimeRuleConfig {
  weekdayOvertimeMultiplier: number;
  weekendOvertimeMultiplier: number;
  holidayOvertimeMultiplier: number;
  overtimeRoundingMinutes: number;
  overtimeDailyCapHours: number;
  overtimePayRunCapHours: number;
}
