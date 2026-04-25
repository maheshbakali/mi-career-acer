# mi-career-acer

Monorepo for a **multi-agent career assistant** MVP: upload a resume (PDF/DOCX), edit a job description with rich text, click **Process**, and get **compatibility scoring**, a **tailored resume**, and **interview preparation** points.

## Projects

| Folder | Stack | Port (default) |
|--------|--------|------------------|
| [backend-api](backend-api/) | ASP.NET Core **7** Web API, EF Core, PostgreSQL, JWT | `http://localhost:5000` |
| [agent-service](agent-service/) | Python **3.11+**, FastAPI, Anthropic | `http://localhost:8000` |
| [frontend](frontend/) | React 18, TypeScript, Vite, Tailwind, TipTap | `http://localhost:3000` |

> **Note:** The template targets **.NET 7** because the build environment used SDK 7. To align with **.NET 8**, install the .NET 8 SDK and change `TargetFramework` in `backend-api/backend-api.csproj` to `net8.0`.

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

- Default models in `agent-service/.env.example` use **Haiku** for resume + interview and **Sonnet** for assessment.
- Reuse **History** instead of re-running Process when possible.
- Keep job descriptions reasonably sized; the API truncates very long plain-text JDs to save tokens.

## Architecture

Browser → **backend-api** (auth, jobs, resume storage, orchestration) and **agent-service** (Anthropic, extraction). The MVP **Process** flow is fully orchestrated in **.NET**, which calls the Python service with the user’s JWT.

## License

MIT (adjust as needed for your portfolio).
