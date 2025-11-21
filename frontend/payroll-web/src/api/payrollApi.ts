import httpClient from "./httpClient";

export async function getPayRuns(page = 1, pageSize = 25) {
  const response = await httpClient.get("/payroll", { params: { page, pageSize } });
  return response.data;
}
