import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Employee {
    id: string;
    employeeCode: string;
    firstName: string;
    lastName: string;
    email: string;
    joinDate: string;
    baseSalary: number;
}

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    private apiUrl = '/api/employees';

    constructor(private http: HttpClient) { }

    getEmployees(): Observable<Employee[]> {
        return this.http.get<Employee[]>(this.apiUrl);
    }

    getEmployee(id: string): Observable<Employee> {
        return this.http.get<Employee>(`${this.apiUrl}/${id}`);
    }

    createEmployee(employee: Partial<Employee>): Observable<string> {
        return this.http.post<string>(this.apiUrl, employee);
    }

    updateEmployee(id: string, employee: Partial<Employee>): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, employee);
    }

    deleteEmployee(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
