import httpClient from "./httpClient";

export async function getLeaveRequests(page = 1, pageSize = 25) {
  const response = await httpClient.get("/leave", { params: { page, pageSize } });
  return response.data;
}
