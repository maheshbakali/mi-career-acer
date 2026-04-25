import json
import logging
from typing import Annotated, Any

from fastapi import Depends, FastAPI, File, HTTPException, Request, UploadFile
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse, StreamingResponse

from agents.assessment_agent import assess_json
from agents.cover_letter_agent import stream_cover_letter
from agents.interview_agent import interview_prep_json
from agents.resume_agent import stream_tailored_resume
from auth import verify_jwt
from config import settings
from extract import extract_text_from_docx, extract_text_from_pdf
from schemas import AgentInput, Envelope

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("mi-career-acer")

app = FastAPI(title="mi-career-acer agent service", version="0.1.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:5000", "http://127.0.0.1:3000", "http://127.0.0.1:5000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


def envelope_ok(data: Any) -> dict:
    return Envelope(success=True, data=data, error=None).model_dump()


def envelope_err(status_code: int, message: str) -> JSONResponse:
    return JSONResponse(
        status_code=status_code,
        content=Envelope(success=False, data=None, error=message).model_dump(),
    )


@app.exception_handler(HTTPException)
async def http_exc_handler(_: Request, exc: HTTPException):
    return JSONResponse(
        status_code=exc.status_code,
        content=Envelope(success=False, data=None, error=str(exc.detail)).model_dump(),
    )


@app.exception_handler(RequestValidationError)
async def validation_handler(_: Request, exc: RequestValidationError):
    return JSONResponse(
        status_code=422,
        content=Envelope(success=False, data=None, error=json.dumps(exc.errors())).model_dump(),
    )


@app.exception_handler(Exception)
async def generic_handler(_: Request, exc: Exception):
    logger.exception("Unhandled error")
    return JSONResponse(
        status_code=500,
        content=Envelope(success=False, data=None, error="Internal server error").model_dump(),
    )


@app.get("/health")
async def health():
    return envelope_ok({"status": "ok"})


@app.post("/resume/extract-text")
async def extract_text(
    _: Annotated[dict, Depends(verify_jwt)],
    file: UploadFile = File(...),
):
    name = (file.filename or "").lower()
    raw = await file.read()
    if not raw:
        return envelope_err(400, "Empty file")
    try:
        if name.endswith(".pdf"):
            text = extract_text_from_pdf(raw)
        elif name.endswith(".docx"):
            text = extract_text_from_docx(raw)
        else:
            return envelope_err(400, "Only PDF and DOCX are supported")
    except Exception as e:
        logger.warning("extract failed: %s", e)
        return envelope_err(400, f"Extraction failed: {e}")
    if not text.strip():
        return envelope_err(400, "No text could be extracted")
    return envelope_ok({"text": text})


@app.post("/agents/resume/stream")
async def resume_stream(
    _: Annotated[dict, Depends(verify_jwt)],
    body: AgentInput,
):
    if not settings.anthropic_api_key:
        return envelope_err(500, "ANTHROPIC_API_KEY is not configured")

    async def gen():
        async for chunk in stream_tailored_resume(body.resume_text, body.job_description):
            yield chunk

    return StreamingResponse(gen(), media_type="text/event-stream")


@app.post("/agents/interview/questions")
async def interview_questions(
    _: Annotated[dict, Depends(verify_jwt)],
    body: AgentInput,
):
    if not settings.anthropic_api_key:
        return envelope_err(500, "ANTHROPIC_API_KEY is not configured")
    try:
        data = await interview_prep_json(body.resume_text, body.job_description)
        return envelope_ok(data)
    except Exception as e:
        logger.warning("interview agent failed: %s", e)
        return envelope_err(500, f"Interview agent failed: {e}")


@app.post("/agents/assess")
async def assess(
    _: Annotated[dict, Depends(verify_jwt)],
    body: AgentInput,
):
    if not settings.anthropic_api_key:
        return envelope_err(500, "ANTHROPIC_API_KEY is not configured")
    try:
        data = await assess_json(body.resume_text, body.job_description)
        return envelope_ok(data)
    except Exception as e:
        logger.warning("assessment failed: %s", e)
        return envelope_err(500, f"Assessment failed: {e}")


@app.post("/agents/cover-letter/stream")
async def cover_stream(
    _: Annotated[dict, Depends(verify_jwt)],
    body: AgentInput,
):
    if not settings.anthropic_api_key:
        return envelope_err(500, "ANTHROPIC_API_KEY is not configured")

    async def gen():
        async for chunk in stream_cover_letter(body.resume_text, body.job_description):
            yield chunk

    return StreamingResponse(gen(), media_type="text/event-stream")
