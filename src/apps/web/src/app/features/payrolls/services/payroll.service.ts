import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PayRun {
    id: string;
    name: string;
    periodStart: string;
    periodEnd: string;
    paymentDate: string;
    status: string;
    totalGross: number;
    totalDeductions: number;
    totalNet: number;
    employeeCount: number;
}

export interface PayRunDetail extends PayRun {
    lineItems: PayRunLineItem[];
}

export interface PayRunLineItem {
    id: string;
    employeeName: string;
    employeeCode: string;
    baseSalary: number;
    allowances: number;
    grossAmount: number;
    epfEmployee: number;
    tax: number;
    totalDeductions: number;
    netAmount: number;
}

@Injectable({
    providedIn: 'root'
})
export class PayrollService {
    private apiUrl = '/api/payruns';

    constructor(private http: HttpClient) { }

    getPayRuns(): Observable<PayRun[]> {
        return this.http.get<PayRun[]>(this.apiUrl);
    }

    getPayRun(id: string): Observable<PayRunDetail> {
        return this.http.get<PayRunDetail>(`${this.apiUrl}/${id}`);
    }

    createPayRun(data: { name: string, periodStart: string, periodEnd: string, paymentDate: string }): Observable<string> {
        return this.http.post<string>(this.apiUrl, data);
    }

    downloadPayslip(payRunId: string, employeeId: string): Observable<Blob> {
        return this.http.get(`${this.apiUrl}/${payRunId}/employees/${employeeId}/payslip`, {
            responseType: 'blob'
        });
    }
}
