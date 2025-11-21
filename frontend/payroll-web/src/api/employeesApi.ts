import httpClient from "./httpClient";
import { Employee } from "../types/Employee";
import { PaginatedResponse } from "../types/Common";

export async function getEmployees(
  page = 1,
  pageSize = 25
): Promise<PaginatedResponse<Employee>> {
  const response = await httpClient.get<PaginatedResponse<Employee>>("/employees", {
    params: { page, pageSize }
  });
  return response.data;
}

export async function getEmployee(id: string): Promise<Employee> {
  const response = await httpClient.get<Employee>(`/employees/${id}`);
  return response.data;
}

export async function createEmployee(payload: Partial<Employee>): Promise<Employee> {
  const response = await httpClient.post<Employee>("/employees", payload);
  return response.data;
}

export async function updateEmployee(
  id: string,
  payload: Partial<Employee>
): Promise<Employee> {
  const response = await httpClient.put<Employee>(`/employees/${id}`, payload);
  return response.data;
}

export async function deleteEmployee(id: string): Promise<void> {
  await httpClient.delete(`/employees/${id}`);
}
