import json
import re

from anthropic import AsyncAnthropic

from config import settings

SYSTEM = """You help candidates prepare for interviews. Given resume and job description, produce JSON ONLY (no markdown)
with three keys: behavioral, technical, situational.
Each value must be an object with:
  "title": string,
  "prep_points": string[] (actionable bullets: themes to study, stories to prepare, likely follow-ups)
Do not include any text outside the JSON object."""


async def interview_prep_json(resume_text: str, job_description: str) -> dict:
    client = AsyncAnthropic(api_key=settings.anthropic_api_key)
    rt = resume_text[: settings.max_context_chars]
    jd = job_description[: settings.max_context_chars]
    user_msg = f"Job description:\n{jd}\n\nResume:\n{rt}"
    msg = await client.messages.create(
        model=settings.anthropic_model_interview,
        max_tokens=settings.max_output_tokens_interview,
        system=SYSTEM,
        messages=[{"role": "user", "content": user_msg}],
    )
    text = ""
    for block in msg.content:
        if block.type == "text":
            text += block.text
    text = text.strip()
    m = re.search(r"\{.*\}", text, re.DOTALL)
    if m:
        text = m.group(0)
    return json.loads(text)

