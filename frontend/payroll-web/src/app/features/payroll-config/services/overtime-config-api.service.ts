import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { OTRule, OTRulePayload } from '../models/ot-rule.model';

@Injectable({ providedIn: 'root' })
export class OvertimeConfigApiService {
  private baseUrl = `${environment.apiBaseUrl}/ot/rules`;

  constructor(private http: HttpClient) {}

  getRules(): Observable<OTRule[]> {
    return this.http.get<OTRule[]>(this.baseUrl);
  }

  getRule(id: string): Observable<OTRule> {
    return this.http.get<OTRule>(`${this.baseUrl}/${id}`);
  }

  createRule(payload: OTRulePayload): Observable<OTRule> {
    return this.http.post<OTRule>(this.baseUrl, payload);
  }

  updateRule(id: string, payload: OTRulePayload): Observable<OTRule> {
    return this.http.put<OTRule>(`${this.baseUrl}/${id}`, payload);
  }

  deleteRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
