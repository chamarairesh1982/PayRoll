import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BranchOption, CompanyOption, CostCenterOption } from '../models/organization.model';

@Injectable({ providedIn: 'root' })
export class OrganizationApiService {
  private baseUrl = `${environment.apiBaseUrl}/organizations`;

  constructor(private http: HttpClient) {}

  getCompanies(): Observable<CompanyOption[]> {
    return this.http.get<CompanyOption[]>(`${this.baseUrl}/companies`);
  }

  getBranches(companyId?: string): Observable<BranchOption[]> {
    let params = new HttpParams();
    if (companyId) {
      params = params.set('companyId', companyId);
    }

    return this.http.get<BranchOption[]>(`${this.baseUrl}/branches`, { params });
  }

  getCostCenters(companyId?: string, branchId?: string): Observable<CostCenterOption[]> {
    let params = new HttpParams();
    if (companyId) {
      params = params.set('companyId', companyId);
    }
    if (branchId) {
      params = params.set('branchId', branchId);
    }

    return this.http.get<CostCenterOption[]>(`${this.baseUrl}/cost-centers`, { params });
  }
}
