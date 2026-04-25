# mi-career-acer

Monorepo for a **multi-agent career assistant** MVP: upload a resume (PDF/DOCX), edit a job description with rich text, click **Process**, and get **compatibility scoring**, a **tailored resume**, and **interview preparation** points.

## Projects

| Folder | Stack | Port (default) |
|--------|--------|------------------|
| [backend-api](backend-api/) | ASP.NET Core **8** Web API, EF Core, PostgreSQL, JWT | `http://localhost:5000` |
| [agent-service](agent-service/) | Python **3.11+**, FastAPI, Anthropic | `http://localhost:8000` |
| [frontend](frontend/) | React 18, TypeScript, Vite, Tailwind, TipTap | `http://localhost:3000` |

> **Note:** The API targets **.NET 8** (`net8.0`). Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) if you do not have it.

## Quick start

1. **PostgreSQL** — from repo root:

   ```bash
   docker compose up -d
   ```

2. **Shared JWT secret** — use the **same** value in:

   - `backend-api/appsettings.json` → `Jwt:SecretKey` (≥ 32 characters)
   - `agent-service/.env` → `JWT_SECRET`

3. **Agent service** — copy `agent-service/.env.example` to `agent-service/.env`, set `ANTHROPIC_API_KEY` and `JWT_SECRET`, then:

   ```bash
   cd agent-service
   python -m venv .venv && source .venv/bin/activate
   pip install -r requirements.txt
   uvicorn main:app --reload --port 8000
   ```

4. **Backend** — update `ConnectionStrings:DefaultConnection` if needed, then:

   ```bash
   cd backend-api
   dotnet run
   ```

   On startup the API applies EF migrations (if the database is reachable).

5. **Frontend** — **Node 18+** required. Copy `frontend/.env.example` to `frontend/.env`, then:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

6. Open `http://localhost:3000` → register → save a job → upload resume → **Process**.

## Cost tips

- Default models in `agent-service/.env.example` use **Haiku 4.5** (`claude-haiku-4-5`) for resume + interview + cover letter and **Sonnet 4.6** (`claude-sonnet-4-6`) for assessment (Anthropic retired Claude 3.5 snapshot IDs).
- Reuse **History** instead of re-running Process when possible.
- Keep job descriptions reasonably sized; the API truncates very long plain-text JDs to save tokens.

## Architecture

Browser → **backend-api** (auth, jobs, resume storage, orchestration) and **agent-service** (Anthropic, extraction). The MVP **Process** flow is fully orchestrated in **.NET**, which calls the Python service with the user’s JWT.

## License

MIT — see [LICENSE](LICENSE)
