import { useCallback, useState } from "react";
import { tokenStorage } from "@/lib/storage";

type StreamState = { text: string; loading: boolean; error: string | null };

/**
 * POST SSE consumer for the Python agent service (portfolio / future UI streaming).
 */
export function useAgentStream() {
  const [state, setState] = useState<StreamState>({ text: "", loading: false, error: null });

  const run = useCallback(async (url: string, body: unknown) => {
    setState({ text: "", loading: true, error: null });
    const token = tokenStorage.get();
    try {
      const res = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify(body),
      });
      if (!res.ok || !res.body) {
        const t = await res.text();
        throw new Error(t || res.statusText);
      }
      const reader = res.body.getReader();
      const dec = new TextDecoder();
      let buf = "";
      let acc = "";
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        buf += dec.decode(value, { stream: true });
        const parts = buf.split("\n\n");
        buf = parts.pop() ?? "";
        for (const block of parts) {
          for (const line of block.split("\n")) {
            if (!line.startsWith("data:")) continue;
            const payload = line.slice(5).trim();
            if (!payload || payload === "[DONE]") continue;
            try {
              const j = JSON.parse(payload) as { content?: string };
              if (j.content) acc += j.content;
            } catch {
              /* ignore */
            }
          }
        }
      }
      setState({ text: acc, loading: false, error: null });
    } catch (e) {
      setState({ text: "", loading: false, error: e instanceof Error ? e.message : "Stream failed" });
    }
  }, []);

  return { ...state, run };
}
