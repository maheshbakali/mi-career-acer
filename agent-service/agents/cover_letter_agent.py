import json
from collections.abc import AsyncIterator

from anthropic import AsyncAnthropic

from config import settings

SYSTEM = """You write concise, tailored cover letters in plain text. Match tone to the job description.
Do not invent credentials. One page maximum."""


async def stream_cover_letter(resume_text: str, job_description: str) -> AsyncIterator[bytes]:
    client = AsyncAnthropic(api_key=settings.anthropic_api_key)
    rt = resume_text[: settings.max_context_chars]
    jd = job_description[: settings.max_context_chars]
    user_msg = f"Job description:\n{jd}\n\nResume:\n{rt}"
    async with client.messages.stream(
        model=settings.anthropic_model_cover_letter,
        max_tokens=settings.max_output_tokens_cover,
        system=SYSTEM,
        messages=[{"role": "user", "content": user_msg}],
    ) as stream:
        async for text in stream.text_stream:
            if text:
                yield f"data: {json.dumps({'content': text})}\n\n".encode("utf-8")
