import json
import re

from anthropic import AsyncAnthropic

from config import settings

SYSTEM = """You assess candidate fit for a role. Output JSON ONLY (no markdown) with this shape:
{
  "overall_score": number from 0 to 100,
  "per_skill_scores": [ { "skill": string, "score": number 0-100, "notes": string } ],
  "gaps": string[] (missing or weak areas vs JD),
  "recommendations": string[] (concrete next steps)
}
Base scores strictly on overlap between resume_text and job_description. Do not invent experience.
No text outside JSON."""


async def assess_json(resume_text: str, job_description: str) -> dict:
    client = AsyncAnthropic(api_key=settings.anthropic_api_key)
    rt = resume_text[: settings.max_context_chars]
    jd = job_description[: settings.max_context_chars]
    user_msg = f"Job description:\n{jd}\n\nResume:\n{rt}"
    msg = await client.messages.create(
        model=settings.anthropic_model_assess,
        max_tokens=settings.max_output_tokens_assess,
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

