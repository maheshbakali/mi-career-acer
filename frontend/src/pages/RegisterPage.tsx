import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { backendApi } from "@/api/backendApi";
import { tokenStorage } from "@/lib/storage";
import type { ApiEnvelope } from "@/types/api";

type AuthData = { token: string; userId: string; email: string };

export function RegisterPage() {
  const nav = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    try {
      const { data } = await backendApi.post<ApiEnvelope<AuthData>>("/api/auth/register", { email, password });
      if (!data.success || !data.data?.token) {
        setErr(data.error ?? "Register failed");
        return;
      }
      tokenStorage.set(data.data.token);
      nav("/", { replace: true });
    } catch (ex: unknown) {
      const msg =
        typeof ex === "object" && ex !== null && "response" in ex
          ? String((ex as { response?: { data?: { error?: string } } }).response?.data?.error)
          : "Register failed";
      setErr(msg || "Register failed");
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-md flex-col justify-center px-4">
      <h1 className="mb-2 text-2xl font-semibold tracking-tight">Create account</h1>
      <p className="mb-8 text-sm text-slate-400">Local portfolio instance — use a strong password.</p>
      <form onSubmit={onSubmit} className="space-y-4 rounded-xl border border-slate-800 bg-slate-900/60 p-6">
        {err && <p className="text-sm text-red-400">{err}</p>}
        <div>
          <label className="mb-1 block text-xs uppercase text-slate-500">Email</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm outline-none ring-brand-500 focus:ring-1"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            type="email"
            autoComplete="email"
            required
          />
        </div>
        <div>
          <label className="mb-1 block text-xs uppercase text-slate-500">Password (min 8)</label>
          <input
            className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm outline-none ring-brand-500 focus:ring-1"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            autoComplete="new-password"
            minLength={8}
            required
          />
        </div>
        <button
          type="submit"
          className="w-full rounded-lg bg-brand-500 py-2 text-sm font-medium text-white hover:bg-brand-400"
        >
          Register
        </button>
        <p className="text-center text-xs text-slate-500">
          Have an account?{" "}
          <Link className="text-brand-400 hover:underline" to="/login">
            Login
          </Link>
        </p>
      </form>
    </div>
  );
}
