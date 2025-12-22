import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RulePackage, RulePackageType, RulePackageVersion } from '../models/rule-package.model';

@Injectable({ providedIn: 'root' })
export class RulePackagesApiService {
  private baseUrl = `${environment.apiUrl}/rule-packages`;

  constructor(private http: HttpClient) {}

  getPackages(params: { type?: RulePackageType; companyId?: string }): Observable<RulePackage[]> {
    let httpParams = new HttpParams();
    if (params.type) {
      httpParams = httpParams.set('type', params.type);
    }
    if (params.companyId) {
      httpParams = httpParams.set('companyId', params.companyId);
    }
    return this.http.get<RulePackage[]>(this.baseUrl, { params: httpParams });
  }

  getVersions(packageId: string): Observable<RulePackageVersion[]> {
    return this.http.get<RulePackageVersion[]>(`${this.baseUrl}/${packageId}/versions`);
  }

  createPackage(payload: { companyId: string; ruleType: RulePackageType; name: string }): Observable<RulePackage> {
    return this.http.post<RulePackage>(this.baseUrl, payload);
  }

  createVersion(
    packageId: string,
    payload: { effectiveFrom: string; effectiveTo?: string | null; contentJson: string }
  ): Observable<RulePackageVersion> {
    return this.http.post<RulePackageVersion>(`${this.baseUrl}/${packageId}/versions`, payload);
  }

  activateVersion(packageId: string, versionId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${packageId}/versions/${versionId}/activate`, {});
  }
}
