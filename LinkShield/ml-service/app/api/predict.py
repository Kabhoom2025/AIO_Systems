from fastapi import APIRouter, Depends, HTTPException

from app.schemas.predict import PredictionRequest, PredictionResponse
from app.services.model_service import ModelNotTrainedError, ModelService, get_model_service

router = APIRouter()


@router.post("/predict", response_model=PredictionResponse)
def predict(request: PredictionRequest, model_service: ModelService = Depends(get_model_service)) -> PredictionResponse:
    try:
        prediction, probability, model_version = model_service.predict(request.features)
    except ModelNotTrainedError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc

    return PredictionResponse(prediction=prediction, probability=probability, model_version=model_version)
