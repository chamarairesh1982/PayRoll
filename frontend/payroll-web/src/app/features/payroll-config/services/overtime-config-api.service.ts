import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { OvertimeRuleConfig } from '../models/overtime-rule-config.model';

@Injectable({ providedIn: 'root' })
export class OvertimeConfigApiService {
  private baseUrl = `${environment.apiBaseUrl}/ot/config`;

  constructor(private http: HttpClient) {}

  getConfig(): Observable<OvertimeRuleConfig> {
    return this.http.get<OvertimeRuleConfig>(this.baseUrl);
  }

  updateConfig(payload: OvertimeRuleConfig): Observable<OvertimeRuleConfig> {
    return this.http.put<OvertimeRuleConfig>(this.baseUrl, payload);
  }
}
