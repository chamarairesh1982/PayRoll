import axios from "axios";

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "https://localhost:5001/api",
  withCredentials: true
});

// TODO: attach auth token via interceptors

export default httpClient;
