import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "@/auth/ProtectedRoute";
import { Layout } from "@/components/Layout";
import { HistoryPage } from "@/pages/HistoryPage";
import { LoginPage } from "@/pages/LoginPage";
import { ProcessPage } from "@/pages/ProcessPage";
import { RegisterPage } from "@/pages/RegisterPage";

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/" element={<ProcessPage />} />
          <Route path="/history" element={<HistoryPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
