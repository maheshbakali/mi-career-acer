# mi-career-acer — backend-api

ASP.NET Core Web API with JWT auth, EF Core + PostgreSQL, local file uploads, and **Process** orchestration (assessment → tailored resume stream → interview prep).

## Setup

1. Copy [appsettings.example.json](appsettings.example.json) values into `appsettings.Development.json` or user secrets; **do not commit secrets**.
2. Ensure `Jwt:SecretKey` matches `JWT_SECRET` in the Python agent service (same HS256 key, ≥ 32 chars).
3. `AgentService:BaseUrl` should point at the FastAPI service (default `http://localhost:8000`).

## Database

```bash
export PATH="$PATH:$HOME/.dotnet/tools"   # if dotnet-ef was installed globally
dotnet ef database update
```

Or run the API once with PostgreSQL up; migrations run on startup.

## Run

```bash
dotnet run
```

Swagger: `http://localhost:5000/swagger` (Development).

## Main endpoints

- `POST /api/auth/register`, `POST /api/auth/login` — JWT
- `GET|POST|PUT|DELETE /api/jobs` — job CRUD; `Description` stores **HTML**
- `POST /api/jobs/{id}/process` — runs three agents; returns `{ success, data: { compatibility, tailoredResume, interviewPrep, jobDescriptionTruncated }, error }`
- `POST /api/resume/upload` — PDF/DOCX, extraction via agent service
- `GET /api/sessions`, `GET /api/sessions/runs`, `GET /api/sessions/{id}`

All JSON responses use the `{ success, data, error }` envelope.
