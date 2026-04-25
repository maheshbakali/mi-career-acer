import { Navigate, Outlet } from "react-router-dom";
import { tokenStorage } from "@/lib/storage";

export function ProtectedRoute() {
  if (!tokenStorage.get()) return <Navigate to="/login" replace />;
  return <Outlet />;
}
