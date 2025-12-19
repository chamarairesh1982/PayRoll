export interface OTRule {
  id: string;
  name: string;
  weekdayMultiplier: number;
  weekendMultiplier: number;
  holidayMultiplier: number;
  roundingMinutes: number;
  dailyCapHours: number;
  payRunCapHours: number;
  appliesOnWeekend: boolean;
  appliesOnHoliday: boolean;
  isActive: boolean;
}

export type OTRulePayload = Omit<OTRule, 'id'>;
