import axios from "axios";
import { tokenStorage } from "@/lib/storage";

const baseURL = import.meta.env.VITE_AGENT_SERVICE_URL ?? "http://localhost:8000";

export const agentApi = axios.create({ baseURL });

agentApi.interceptors.request.use((config) => {
  const t = tokenStorage.get();
  if (t) config.headers.Authorization = `Bearer ${t}`;
  return config;
});
