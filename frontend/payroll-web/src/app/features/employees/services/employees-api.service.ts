import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Employee, PaginatedResult } from '../models/employee.model';

@Injectable({ providedIn: 'root' })
export class EmployeesApiService {
  private baseUrl = `${environment.apiBaseUrl}/employees`;

  constructor(private http: HttpClient) {}

  getEmployees(
    page: number,
    pageSize: number,
    options?: { companyId?: string; branchId?: string; costCenterId?: string },
  ): Observable<PaginatedResult<Employee>> {
    const params = new URLSearchParams();
    params.set('page', String(page));
    params.set('pageSize', String(pageSize));

    if (options?.companyId) {
      params.set('companyId', options.companyId);
    }

    if (options?.branchId) {
      params.set('branchId', options.branchId);
    }

    if (options?.costCenterId) {
      params.set('costCenterId', options.costCenterId);
    }

    return this.http.get<PaginatedResult<Employee>>(`${this.baseUrl}?${params.toString()}`);
  }

  getEmployee(id: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.baseUrl}/${id}`);
  }

  createEmployee(payload: Partial<Employee>): Observable<Employee> {
    return this.http.post<Employee>(this.baseUrl, payload);
  }

  updateEmployee(id: string, payload: Partial<Employee>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteEmployee(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
