import httpClient from "./httpClient";

export async function getAttendance(page = 1, pageSize = 25) {
  const response = await httpClient.get("/attendance", { params: { page, pageSize } });
  return response.data;
}
