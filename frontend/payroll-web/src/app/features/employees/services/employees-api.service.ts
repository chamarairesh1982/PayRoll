import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Employee, PaginatedResult } from '../models/employee.model';

@Injectable({ providedIn: 'root' })
export class EmployeesApiService {
  private baseUrl = `${environment.apiBaseUrl}/employees`;

  constructor(private http: HttpClient) {}

  getEmployees(page: number, pageSize: number): Observable<PaginatedResult<Employee>> {
    return this.http.get<PaginatedResult<Employee>>(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
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
