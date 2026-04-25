from typing import Annotated

import jwt
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from config import settings

security = HTTPBearer(auto_error=False)


def _truncate_secret(secret: str) -> bytes:
    raw = secret.encode("utf-8")
    if len(raw) < 32:
        raise RuntimeError("JWT_SECRET must be at least 32 bytes for HS256")
    return raw


def verify_jwt(
    creds: Annotated[HTTPAuthorizationCredentials | None, Depends(security)],
) -> dict:
    if creds is None or not creds.credentials:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Missing bearer token")
    token = creds.credentials
    try:
        payload = jwt.decode(
            token,
            _truncate_secret(settings.jwt_secret),
            algorithms=["HS256"],
            audience=settings.jwt_audience,
            issuer=settings.jwt_issuer,
        )
        return payload
    except jwt.PyJWTError as e:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token") from e
