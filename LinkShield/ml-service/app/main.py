from fastapi import FastAPI

from app.api.predict import router as predict_router
from app.core.config import get_settings
from app.services.model_service import get_model_service

settings = get_settings()
app = FastAPI(title=settings.service_name, version="1.0.0")

app.include_router(predict_router, prefix=settings.api_prefix, tags=["predict"])


@app.get("/health")
def health() -> dict:
    return {
        "status": "Healthy",
        "service": settings.service_name,
        "model_ready": get_model_service().is_ready(),
    }
