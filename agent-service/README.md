# mi-career-acer — agent-service

FastAPI service exposing Anthropic-powered agents and **PDF/DOCX text extraction**.

## Setup

```bash
python -m venv .venv
source .venv/bin/activate   # Windows: .venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env
```

Edit `.env`:

- `ANTHROPIC_API_KEY` — required for agents
- `JWT_SECRET` — must match `Jwt:SecretKey` in the .NET `appsettings`
- Model IDs — defaults target Claude 3.5 Haiku/Sonnet; update if Anthropic renames models

## Run

```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

## Routes

- `GET /health` — no auth
- `POST /resume/extract-text` — multipart `file`, JWT required
- `POST /agents/resume/stream` — SSE (`text/event-stream`), JWT required
- `POST /agents/interview/questions` — JSON prep structure, JWT required
- `POST /agents/assess` — JSON scores, JWT required
- `POST /agents/cover-letter/stream` — optional; **not** used by MVP Process

Responses use `{ success, data, error }` except raw SSE streams.
