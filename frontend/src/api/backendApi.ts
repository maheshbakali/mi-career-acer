import axios from "axios";
import { tokenStorage } from "@/lib/storage";

const baseURL = import.meta.env.VITE_BACKEND_API_URL ?? "http://localhost:5000";

export const backendApi = axios.create({ baseURL });

backendApi.interceptors.request.use((config) => {
  const t = tokenStorage.get();
  if (t) config.headers.Authorization = `Bearer ${t}`;
  return config;
});

backendApi.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err.response?.status === 401) tokenStorage.clear();
    return Promise.reject(err);
  }
);
