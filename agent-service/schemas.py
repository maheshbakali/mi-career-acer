from pydantic import BaseModel, Field


class AgentInput(BaseModel):
    resume_text: str = Field(..., min_length=1)
    job_description: str = Field(..., min_length=1)


class ExtractTextData(BaseModel):
    text: str


class Envelope(BaseModel):
    success: bool
    data: dict | list | str | None = None
    error: str | None = None
