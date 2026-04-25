import { Link, Outlet, useNavigate } from "react-router-dom";
import { tokenStorage } from "@/lib/storage";

export function Layout() {
  const nav = useNavigate();
  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-800 bg-slate-950/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-semibold tracking-tight text-white">
            mi-career-acer
          </Link>
          <nav className="flex items-center gap-4 text-sm text-slate-300">
            <Link className="hover:text-white" to="/">
              Process
            </Link>
            <Link className="hover:text-white" to="/history">
              History
            </Link>
            <button
              type="button"
              className="rounded-lg border border-slate-700 px-3 py-1 text-xs hover:bg-slate-900"
              onClick={() => {
                tokenStorage.clear();
                nav("/login");
              }}
            >
              Logout
            </button>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
