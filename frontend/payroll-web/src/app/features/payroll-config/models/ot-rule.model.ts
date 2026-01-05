export type OvertimeType = 'Normal' | 'Weekend' | 'Holiday';

export type OvertimeRoundingMode = 'Down' | 'Nearest' | 'Up';

export interface OTRule {
  id: string;
  type: OvertimeType;
  multiplier: number;
  roundToMinutes: number;
  roundingMode: OvertimeRoundingMode;
  dailyHoursCap?: number | null;
  monthlyHoursCap?: number | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
}

export type OTRulePayload = Omit<OTRule, 'id'>;
