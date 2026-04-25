from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    anthropic_api_key: str = ""
    jwt_secret: str = ""
    jwt_issuer: str = "mi-career-acer"
    jwt_audience: str = "mi-career-acer-clients"

    anthropic_model_resume: str = "claude-3-5-haiku-20241022"
    anthropic_model_interview: str = "claude-3-5-haiku-20241022"
    anthropic_model_assess: str = "claude-3-5-sonnet-20241022"
    anthropic_model_cover_letter: str = "claude-3-5-haiku-20241022"

    max_context_chars: int = 12000
    max_output_tokens_resume: int = 4096
    max_output_tokens_interview: int = 2048
    max_output_tokens_assess: int = 2048
    max_output_tokens_cover: int = 1024


settings = Settings()
