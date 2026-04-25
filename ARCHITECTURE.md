# mi-career-acer — Architecture Diagram

## System Overview

```mermaid
graph TB
    subgraph Browser["Browser — localhost:3000"]
        direction TB
        FE_PAGES["Pages\nLogin · Register · Process · History"]
        FE_HOOKS["useAgentStream\nSSE Hook"]
        FE_API["Axios Clients\nbackendApi · agentApi"]
        FE_PAGES --> FE_HOOKS
        FE_PAGES --> FE_API
    end

    subgraph Backend["Backend API — localhost:5000 (ASP.NET Core 8 / .NET 8)"]
        direction TB
        subgraph Controllers["Controllers"]
            AUTH_C["AuthController\nPOST /register · /login"]
            JOBS_C["JobsController\nCRUD + POST /process"]
            RES_C["ResumeController\nPOST /upload · GET /current"]
            SESS_C["SessionsController\nGET /sessions · /runs"]
        end
        subgraph BServices["Services"]
            JWT_SVC["JwtTokenService"]
            AUTH_SVC["AuthService\n(BCrypt hashing)"]
            ORCH_SVC["AgentOrchestrationService\n(sequential: assess → resume → interview)"]
            FILE_SVC["FileStorageService\n(disk: uploads/)"]
            HTML_SVC["HtmlToPlainTextService"]
            AGENT_CLIENT["AgentHttpClient\n(calls Agent Service)"]
        end
        EF["EF Core\n(auto-migrations on startup)"]
        JOBS_C --> ORCH_SVC
        RES_C --> FILE_SVC
        ORCH_SVC --> AGENT_CLIENT
        ORCH_SVC --> HTML_SVC
        AUTH_C --> AUTH_SVC
        AUTH_C --> JWT_SVC
        Controllers --> EF
    end

    subgraph AgentSvc["Agent Service — localhost:8000 (Python FastAPI)"]
        direction TB
        AGENT_AUTH["JWT Middleware\n(shared secret with backend)"]
        subgraph Agents["AI Agents"]
            ASSESS["AssessmentAgent\nclaude-sonnet-4-6\nJSON: score · gaps · recommendations"]
            RESUME["ResumeAgent\nclaude-haiku-4-5\nSSE stream: tailored resume"]
            INTERVIEW["InterviewAgent\nclaude-haiku-4-5\nJSON: behavioral · technical · situational"]
            COVER["CoverLetterAgent\nclaude-haiku-4-5\nSSE stream: cover letter"]
        end
        EXTRACT["extract.py\nPDF (pypdf) · DOCX (python-docx)"]
        AGENT_AUTH --> Agents
        AGENT_AUTH --> EXTRACT
    end

    subgraph Data["Data Layer"]
        PG[("PostgreSQL 16\n(Docker)\n\nusers\njobs\nresume_files\nagent_sessions")]
        FILES["uploads/\n(local disk)"]
    end

    ANTHROPIC["☁ Anthropic API\nclaude-sonnet-4-6\nclaude-haiku-4-5"]

    %% Frontend → Backend (REST + JWT)
    FE_API -- "REST + JWT\nAuth · Jobs · Resume · Sessions" --> Backend

    %% Frontend → Agent Service (SSE streaming direct)
    FE_HOOKS -- "SSE + JWT\n/agents/resume/stream\n/agents/cover-letter/stream" --> AgentSvc

    %% Backend → Agent Service (orchestration)
    AGENT_CLIENT -- "REST + JWT\n/agents/assess\n/agents/resume/stream\n/agents/interview/questions" --> AgentSvc

    %% Backend → Data
    EF -- "npgsql" --> PG
    FILE_SVC -- "read/write" --> FILES

    %% Agent → Anthropic
    Agents -- "HTTPS\nAnthropic SDK" --> ANTHROPIC

    style Browser fill:#dbeafe,stroke:#3b82f6
    style Backend fill:#dcfce7,stroke:#22c55e
    style AgentSvc fill:#fef9c3,stroke:#eab308
    style Data fill:#f3e8ff,stroke:#a855f7
    style ANTHROPIC fill:#ffe4e6,stroke:#f43f5e
```

## Data Flow: "Process Job" (Happy Path)

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant BE as Backend API
    participant AS as Agent Service
    participant DB as PostgreSQL
    participant AI as Anthropic API

    FE->>BE: POST /api/jobs/{id}/process (JWT)
    BE->>DB: Fetch job + latest resume
    BE->>BE: HTML → plain text (max 12,000 chars)

    BE->>AS: POST /agents/assess (JWT)
    AS->>AI: claude-sonnet-4-6
    AI-->>AS: JSON {score, gaps, recommendations}
    AS-->>BE: assessment JSON
    BE->>DB: INSERT agent_session (Assessment)

    BE->>AS: POST /agents/resume/stream (JWT)
    AS->>AI: claude-haiku-4-5 (streaming)
    AI-->>AS: SSE chunks
    AS-->>BE: collected full text
    BE->>DB: INSERT agent_session (Resume)

    BE->>AS: POST /agents/interview/questions (JWT)
    AS->>AI: claude-haiku-4-5
    AI-->>AS: JSON {behavioral, technical, situational}
    AS-->>BE: interview JSON
    BE->>DB: INSERT agent_session (Interview)

    BE-->>FE: {score, tailoredResume, interviewQuestions}
    FE->>FE: Render 3-column results layout
```

## Technology Stack Summary

| Layer | Technology | Port |
|---|---|---|
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS + TipTap | 3000 |
| Backend API | ASP.NET Core 8, EF Core, JWT, AutoMapper, BCrypt | 5000 |
| Agent Service | Python 3.11, FastAPI, Uvicorn, Anthropic SDK | 8000 |
| Database | PostgreSQL 16 (Docker) | 5432 |
| AI Models | claude-sonnet-4-6 (assess), claude-haiku-4-5 (resume/interview/cover) | — |

## Key Design Decisions

- **Shared JWT secret** — backend and agent service share the same secret so the frontend's token works with both services
- **Sequential orchestration** — assess → resume → interview run one at a time (not parallel) to ensure each agent can use prior context
- **Context capping** — job descriptions and resume text are truncated to 12,000 chars before sending to Anthropic (cost control)
- **SSE for long outputs** — resume and cover letter use Server-Sent Events so the user sees text appear progressively
- **Regex JSON extraction** — agents instruct the model to output JSON only; responses are parsed with regex to strip any prose wrapper
