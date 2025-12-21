import { Injectable } from '@angular/core';
import { TaxRelief, TaxScheme, TaxSlab } from '../models/tax-scheme.model';

export interface TaxBreakdownLine {
  slab: TaxSlab;
  taxableInBand: number;
  taxForBand: number;
}

export interface TaxCalculationResult {
  grossTaxable: number;
  reliefTotal: number;
  taxableIncome: number;
  calculatedTax: number;
  breakdown: TaxBreakdownLine[];
}

@Injectable({ providedIn: 'root' })
export class TaxCalculationService {
  calculateMonthlyTax(grossTaxable: number, scheme: TaxScheme, reliefs: TaxRelief[] = []): TaxCalculationResult {
    const normalizedTaxable = Math.max(0, grossTaxable || 0);
    const reliefTotal = this.calculateMonthlyRelief(reliefs);
    const taxableIncome = Math.max(0, normalizedTaxable - reliefTotal);

    const breakdown = this.calculateBreakdown(taxableIncome, scheme.slabs);
    const taxBeforeRebate = breakdown.reduce((sum, line) => sum + line.taxForBand, 0);
    const rebateTotal = this.calculateMonthlyRebate(reliefs);
    const calculatedTax = Math.max(0, taxBeforeRebate - rebateTotal);

    return {
      grossTaxable: normalizedTaxable,
      reliefTotal,
      taxableIncome,
      calculatedTax,
      breakdown,
    };
  }

  private calculateMonthlyRelief(reliefs: TaxRelief[]): number {
    return reliefs
      .filter(relief => relief.reliefType === 'IncomeRelief')
      .reduce((sum, relief) => sum + this.normalizeRelief(relief), 0);
  }

  private calculateMonthlyRebate(reliefs: TaxRelief[]): number {
    return reliefs
      .filter(relief => relief.reliefType === 'TaxRebate')
      .reduce((sum, relief) => sum + this.normalizeRelief(relief), 0);
  }

  private normalizeRelief(relief: TaxRelief): number {
    if (relief.frequency === 'Annual') {
      return relief.amount / 12;
    }
    return relief.amount;
  }

  private calculateBreakdown(taxableIncome: number, slabs: TaxSlab[]): TaxBreakdownLine[] {
    const breakdown: TaxBreakdownLine[] = [];
    let remaining = taxableIncome;

    for (const slab of slabs) {
      if (remaining <= 0) {
        break;
      }

      const upperBound = slab.to ?? Number.POSITIVE_INFINITY;
      const slabRange = Math.max(0, upperBound - slab.from);
      const taxableInBand = Math.min(remaining, slabRange);
      const taxForBand = taxableInBand * (slab.rate / 100);

      breakdown.push({
        slab,
        taxableInBand,
        taxForBand,
      });

      remaining -= taxableInBand;
    }

    return breakdown;
  }
}
