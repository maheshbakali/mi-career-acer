import { useQuery } from "@tanstack/react-query";
import { backendApi } from "@/api/backendApi";
import type { ApiEnvelope } from "@/types/api";

type AgentSessionResponse = {
  id: string;
  jobId: string;
  agentType: string;
  outputPayload: string;
  compatibilityScore: number | null;
  createdAt: string;
};

type SessionGroupDto = {
  jobId: string;
  jobTitle?: string | null;
  runAt: string;
  sessions: AgentSessionResponse[];
};

export function HistoryPage() {
  const q = useQuery({
    queryKey: ["sessions-runs"],
    queryFn: async () => {
      const { data } = await backendApi.get<ApiEnvelope<SessionGroupDto[]>>("/api/sessions/runs");
      if (!data.success) throw new Error(data.error ?? "failed");
      return data.data ?? [];
    },
  });

  if (q.isLoading) return <p className="text-sm text-slate-400">Loading history…</p>;
  if (q.error) return <p className="text-sm text-red-400">{(q.error as Error).message}</p>;
  if (!q.data?.length) return <p className="text-sm text-slate-400">No runs yet. Process a job to see history.</p>;

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold">Session history</h2>
      <div className="space-y-4">
        {q.data.map((g) => (
          <div key={`${g.jobId}-${g.runAt}`} className="rounded-xl border border-slate-800 bg-slate-900/40 p-4">
            <div className="mb-2 flex flex-wrap items-baseline justify-between gap-2">
              <p className="font-medium text-white">{g.jobTitle || "Untitled job"}</p>
              <p className="text-xs text-slate-500">{new Date(g.runAt).toLocaleString()}</p>
            </div>
            <ul className="space-y-2 text-sm text-slate-300">
              {g.sessions.map((s) => (
                <li key={s.id} className="flex flex-wrap gap-2 border-t border-slate-800 pt-2 first:border-0 first:pt-0">
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs font-medium text-brand-400">{s.agentType}</span>
                  {s.compatibilityScore != null && (
                    <span className="text-xs text-slate-500">score {s.compatibilityScore}</span>
                  )}
                  <span className="line-clamp-2 text-xs text-slate-500">{s.outputPayload.slice(0, 160)}…</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </div>
  );
}
