import httpClient from "./httpClient";
import { Employee } from "../types/Employee";

export async function getEmployees(page = 1, pageSize = 25) {
  const response = await httpClient.get("/employees", { params: { page, pageSize } });
  return response.data;
}

export async function getEmployee(id: string) {
  const response = await httpClient.get(`/employees/${id}`);
  return response.data as Employee;
}

export async function createEmployee(payload: Partial<Employee>) {
  const response = await httpClient.post("/employees", payload);
  return response.data;
}
