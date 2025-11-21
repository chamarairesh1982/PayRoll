import httpClient from "./httpClient";

export async function downloadStatutoryReport(period: string) {
  const response = await httpClient.get("/reports/statutory", {
    params: { period },
    responseType: "blob"
  });
  return response.data;
}
