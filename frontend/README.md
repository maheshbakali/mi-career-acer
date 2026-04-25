# mi-career-acer — frontend

React + TypeScript (Vite) SPA: login/register, **Process** workspace (TipTap job description, resume upload, results panels), and session history.

## Requirements

- **Node.js 18+** and npm

## Setup

```bash
cp .env.example .env
# VITE_BACKEND_API_URL=http://localhost:5000
# VITE_AGENT_SERVICE_URL=http://localhost:8000
npm install
npm run dev
```

Open `http://localhost:3000`.

## Scripts

- `npm run dev` — Vite dev server (port 3000)
- `npm run build` — production build
- `npm run preview` — preview production build

## Notes

- JWT is stored in `localStorage` and sent to the .NET API. The same token is forwarded by the backend when it calls the agent service for extraction and Process orchestration.
- `src/hooks/useAgentStream.ts` is included for direct SSE calls to Python if you extend the UI later.
