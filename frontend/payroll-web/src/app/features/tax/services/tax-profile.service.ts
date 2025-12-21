import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { EmployeeTaxProfile, TaxCategory, TaxExemptions } from '../models/tax-profile.model';

@Injectable({ providedIn: 'root' })
export class TaxProfileService {
  private profilesSubject = new BehaviorSubject<EmployeeTaxProfile[]>(this.seedProfiles());

  getProfiles(): Observable<EmployeeTaxProfile[]> {
    return this.profilesSubject.asObservable();
  }

  getProfileByEmployee(employeeId: string): Observable<EmployeeTaxProfile | null> {
    const profile = this.profilesSubject.value.find(item => item.employeeId === employeeId) ?? null;
    return of(profile);
  }

  saveProfile(profile: EmployeeTaxProfile): void {
    const profiles = this.profilesSubject.value;
    const existing = profiles.find(item => item.employeeId === profile.employeeId);
    const updated = existing
      ? profiles.map(item => (item.employeeId === profile.employeeId ? profile : item))
      : [profile, ...profiles];

    this.profilesSubject.next(updated);
  }

  createProfile(employeeId: string): EmployeeTaxProfile {
    return {
      id: `tax_profile_${Math.random().toString(36).slice(2, 10)}`,
      employeeId,
      taxCategory: 'Resident',
      isResident: true,
      exemptions: this.defaultExemptions(),
      lastUpdated: new Date().toISOString(),
    };
  }

  getTaxCategories(): { label: string; value: TaxCategory }[] {
    return [
      { label: 'Resident', value: 'Resident' },
      { label: 'Non-resident', value: 'NonResident' },
      { label: 'Special rate', value: 'SpecialRate' },
    ];
  }

  defaultExemptions(): TaxExemptions {
    return {
      PrimaryEmployment: false,
      SeniorCitizen: false,
      Disabled: false,
      ForeignIncome: false,
    };
  }

  private seedProfiles(): EmployeeTaxProfile[] {
    return [
      {
        id: 'tax_profile_seed_01',
        employeeId: 'EMP-001',
        taxCategory: 'Resident',
        isResident: true,
        exemptions: {
          PrimaryEmployment: true,
          SeniorCitizen: false,
          Disabled: false,
          ForeignIncome: false,
        },
        lastUpdated: new Date().toISOString(),
      },
    ];
  }
}
