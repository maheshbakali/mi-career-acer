import json
from collections.abc import AsyncIterator

from anthropic import AsyncAnthropic

from config import settings

SYSTEM = """You are an expert resume writer. Rewrite the candidate's resume to align with the job description.
Preserve factual accuracy: do not invent employers, dates, degrees, or credentials.
Output only the resume body in plain text with clear section headings (e.g. SUMMARY, EXPERIENCE, SKILLS, EDUCATION).
Use strong action verbs and quantify impact where the source resume implies it; do not fabricate metrics."""


async def stream_tailored_resume(resume_text: str, job_description: str) -> AsyncIterator[bytes]:
    client = AsyncAnthropic(api_key=settings.anthropic_api_key)
    rt = resume_text[: settings.max_context_chars]
    jd = job_description[: settings.max_context_chars]
    user_msg = f"Job description:\n{jd}\n\nCurrent resume:\n{rt}"
    async with client.messages.stream(
        model=settings.anthropic_model_resume,
        max_tokens=settings.max_output_tokens_resume,
        system=SYSTEM,
        messages=[{"role": "user", "content": user_msg}],
    ) as stream:
        async for text in stream.text_stream:
            if text:
                yield f"data: {json.dumps({'content': text})}\n\n".encode("utf-8")
