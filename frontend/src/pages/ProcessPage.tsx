import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { backendApi } from "@/api/backendApi";
import { JobDescriptionEditor } from "@/components/JobDescriptionEditor";
import type { ApiEnvelope } from "@/types/api";

type Job = {
  id: string;
  title: string;
  company: string;
  location: string;
  url: string;
  description: string;
  createdAt: string;
};

type ResumeCurrent = { id: string; fileName: string; uploadedAt: string; hasExtractedText: boolean } | null;

type ProcessData = {
  compatibility: Record<string, unknown>;
  tailoredResume: string;
  interviewPrep: Record<string, unknown>;
  jobDescriptionTruncated: boolean;
};

function stripHtmlToPreview(html: string) {
  return html.replace(/<[^>]+>/g, " ").replace(/\s+/g, " ").trim();
}

function PrepSection({ title, data }: { title: string; data?: { title?: string; prep_points?: string[] } }) {
  if (!data) return null;
  const pts = data.prep_points ?? [];
  return (
    <div className="mb-4">
      <h4 className="mb-2 text-sm font-semibold text-brand-400">{data.title ?? title}</h4>
      <ul className="list-disc space-y-1 pl-5 text-sm text-slate-300">
        {pts.map((p) => (
          <li key={p}>{p}</li>
        ))}
      </ul>
    </div>
  );
}

export function ProcessPage() {
  const qc = useQueryClient();
  const [title, setTitle] = useState("");
  const [company, setCompany] = useState("");
  const [jdHtml, setJdHtml] = useState("<p></p>");
  const [jobId, setJobId] = useState<string | null>(null);
  const [result, setResult] = useState<ProcessData | null>(null);
  const [procErr, setProcErr] = useState<string | null>(null);

  const jdPlainLen = useMemo(() => stripHtmlToPreview(jdHtml).length, [jdHtml]);

  const resumeQ = useQuery({
    queryKey: ["resume-current"],
    queryFn: async () => {
      const { data } = await backendApi.get<ApiEnvelope<ResumeCurrent>>("/api/resume/current");
      if (!data.success) throw new Error(data.error ?? "failed");
      return data.data;
    },
  });

  const saveJob = useMutation({
    mutationFn: async () => {
      const { data } = await backendApi.post<ApiEnvelope<Job>>("/api/jobs", {
        title,
        company,
        location: "",
        url: "",
        description: jdHtml,
      });
      if (!data.success || !data.data) throw new Error(data.error ?? "Save failed");
      return data.data;
    },
    onSuccess: (job) => {
      setJobId(job.id);
      qc.invalidateQueries({ queryKey: ["jobs"] });
    },
  });

  const updateJob = useMutation({
    mutationFn: async () => {
      if (!jobId) throw new Error("No job");
      const { data } = await backendApi.put<ApiEnvelope<Job>>(`/api/jobs/${jobId}`, {
        title,
        company,
        location: "",
        url: "",
        description: jdHtml,
      });
      if (!data.success || !data.data) throw new Error(data.error ?? "Update failed");
      return data.data;
    },
  });

  const upload = useMutation({
    mutationFn: async (file: File) => {
      const fd = new FormData();
      fd.append("file", file);
      const { data } = await backendApi.post<ApiEnvelope<NonNullable<ResumeCurrent>>>("/api/resume/upload", fd);
      if (!data.success || !data.data) throw new Error(data.error ?? "Upload failed");
      return data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["resume-current"] }),
  });

  const process = useMutation({
    mutationFn: async () => {
      if (!jobId) throw new Error("Save the job first");
      const { data } = await backendApi.post<ApiEnvelope<ProcessData>>(`/api/jobs/${jobId}/process`);
      if (!data.success || !data.data) throw new Error(data.error ?? "Process failed");
      return data.data;
    },
    onSuccess: (d) => {
      setResult(d);
      setProcErr(null);
      qc.invalidateQueries({ queryKey: ["sessions-runs"] });
    },
    onError: (e: Error) => {
      setProcErr(e.message);
    },
  });

  const canProcess =
    !!jobId &&
    jdPlainLen > 0 &&
    !!resumeQ.data?.hasExtractedText &&
    !process.isPending &&
    !saveJob.isPending &&
    !updateJob.isPending;

  return (
    <div className="space-y-10">
      <div>
        <h2 className="text-xl font-semibold">Job &amp; resume</h2>
        <p className="mt-1 text-sm text-slate-400">
          Upload PDF/DOCX, paste the JD in the rich editor, save the job, then run <strong>Process</strong>.
        </p>
      </div>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900/40 p-5">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Job</h3>
          <div className="grid gap-3 sm:grid-cols-2">
            <div>
              <label className="text-xs text-slate-500">Title</label>
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </div>
            <div>
              <label className="text-xs text-slate-500">Company</label>
              <input
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                value={company}
                onChange={(e) => setCompany(e.target.value)}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-slate-500">Job description</label>
            <div className="mt-2">
              <JobDescriptionEditor key={jobId ?? "new"} value={jdHtml} onChange={setJdHtml} />
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={saveJob.isPending || jdPlainLen === 0}
              onClick={() => saveJob.mutate()}
              className="rounded-lg bg-slate-100 px-4 py-2 text-sm font-medium text-slate-900 hover:bg-white disabled:opacity-40"
            >
              {jobId ? "Save as new job" : "Save job"}
            </button>
            {jobId && (
              <button
                type="button"
                disabled={updateJob.isPending || jdPlainLen === 0}
                onClick={() => updateJob.mutate()}
                className="rounded-lg border border-slate-600 px-4 py-2 text-sm hover:bg-slate-900 disabled:opacity-40"
              >
                Update job
              </button>
            )}
          </div>
          {jobId && <p className="text-xs text-slate-500">Active job id: {jobId}</p>}
          {(saveJob.error || updateJob.error) && (
            <p className="text-sm text-red-400">{(saveJob.error ?? updateJob.error)?.message}</p>
          )}
        </div>

        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-900/40 p-5">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Resume</h3>
          <label className="flex cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed border-slate-600 bg-slate-950/60 px-4 py-10 text-center text-sm text-slate-400 hover:border-brand-500 hover:text-slate-200">
            <input
              type="file"
              accept=".pdf,.docx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
              className="hidden"
              onChange={(e) => {
                const f = e.target.files?.[0];
                if (f) upload.mutate(f);
                e.target.value = "";
              }}
            />
            Drop PDF or DOCX here, or click to browse
          </label>
          {upload.isPending && <p className="text-sm text-slate-400">Uploading…</p>}
          {upload.error && <p className="text-sm text-red-400">{(upload.error as Error).message}</p>}
          {resumeQ.data && (
            <p className="text-sm text-slate-300">
              Current file: <span className="font-medium text-white">{resumeQ.data.fileName}</span>{" "}
              {resumeQ.data.hasExtractedText ? (
                <span className="text-emerald-400">(text ready)</span>
              ) : (
                <span className="text-amber-400">(extracting…)</span>
              )}
            </p>
          )}
        </div>
      </section>

      <div className="flex items-center gap-4">
        <button
          type="button"
          disabled={!canProcess}
          onClick={() => process.mutate()}
          className="rounded-lg bg-brand-500 px-6 py-2.5 text-sm font-semibold text-white shadow-lg shadow-brand-500/20 hover:bg-brand-400 disabled:cursor-not-allowed disabled:opacity-40"
        >
          {process.isPending ? "Processing…" : "Process"}
        </button>
        {!jobId && <span className="text-xs text-slate-500">Save a job first.</span>}
        {jobId && jdPlainLen === 0 && <span className="text-xs text-slate-500">Add job description text.</span>}
        {jobId && jdPlainLen > 0 && !resumeQ.data?.hasExtractedText && (
          <span className="text-xs text-amber-400">Upload a resume before processing.</span>
        )}
      </div>
      {procErr && <p className="text-sm text-red-400">{procErr}</p>}

      {result && (
        <div className="grid gap-6 lg:grid-cols-3">
          <section className="rounded-xl border border-slate-800 bg-slate-900/50 p-5 lg:col-span-1">
            <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-400">Compatibility</h3>
            {result.jobDescriptionTruncated && (
              <p className="mb-2 text-xs text-amber-400">Job description was truncated for token limits.</p>
            )}
            <div className="text-4xl font-bold text-brand-400">
              {String((result.compatibility as { overall_score?: number }).overall_score ?? "—")}
            </div>
            <p className="mt-1 text-xs text-slate-500">Overall score (0–100)</p>
            <div className="mt-4 space-y-2 text-sm">
              {Array.isArray((result.compatibility as { per_skill_scores?: unknown }).per_skill_scores) &&
                ((result.compatibility as { per_skill_scores: { skill: string; score: number; notes?: string }[] }).per_skill_scores).map(
                  (s) => (
                    <div key={s.skill} className="flex justify-between gap-2 border-b border-slate-800 py-1">
                      <span className="text-slate-300">{s.skill}</span>
                      <span className="font-mono text-brand-400">{s.score}</span>
                    </div>
                  )
                )}
            </div>
            <div className="mt-4">
              <h4 className="text-xs font-semibold uppercase text-slate-500">Gaps</h4>
              <ul className="mt-1 list-disc space-y-1 pl-5 text-sm text-slate-300">
                {(
                  (result.compatibility as { gaps?: string[] }).gaps ?? []
                ).map((g) => (
                  <li key={g}>{g}</li>
                ))}
              </ul>
            </div>
            <div className="mt-4">
              <h4 className="text-xs font-semibold uppercase text-slate-500">Recommendations</h4>
              <ul className="mt-1 list-disc space-y-1 pl-5 text-sm text-slate-300">
                {(
                  (result.compatibility as { recommendations?: string[] }).recommendations ?? []
                ).map((g) => (
                  <li key={g}>{g}</li>
                ))}
              </ul>
            </div>
          </section>

          <section className="rounded-xl border border-slate-800 bg-slate-900/50 p-5 lg:col-span-1">
            <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-400">Tailored resume</h3>
            <pre className="max-h-[480px] overflow-auto whitespace-pre-wrap rounded-lg bg-slate-950 p-3 text-xs leading-relaxed text-slate-200">
              {result.tailoredResume}
            </pre>
          </section>

          <section className="rounded-xl border border-slate-800 bg-slate-900/50 p-5 lg:col-span-1">
            <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-400">Interview preparation</h3>
            <PrepSection title="Behavioral" data={(result.interviewPrep as { behavioral?: { title?: string; prep_points?: string[] } }).behavioral} />
            <PrepSection title="Technical" data={(result.interviewPrep as { technical?: { title?: string; prep_points?: string[] } }).technical} />
            <PrepSection title="Situational" data={(result.interviewPrep as { situational?: { title?: string; prep_points?: string[] } }).situational} />
          </section>
        </div>
      )}
    </div>
  );
}
