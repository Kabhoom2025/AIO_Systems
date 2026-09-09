import json
from pathlib import Path

import joblib
import numpy as np

from app.core.config import get_settings
from app.schemas.predict import UrlFeatures

FEATURE_ORDER = [
    "url_length", "domain_length", "path_length", "query_length",
    "subdomain_count", "dot_count", "hyphen_count", "digit_count", "special_char_count",
    "is_ip_address_host", "is_https", "has_url_encoding", "has_punycode",
    "is_shortened_url", "has_suspicious_tld", "has_redirect_parameter",
    "domain_age_days", "redirect_count", "has_mail_configuration",
    "ssl_is_valid", "ssl_domain_matches",
]


class ModelNotTrainedError(RuntimeError):
    """Raised when a prediction is requested but no model artifact has been trained yet."""


class ModelService:
    """Loads the trained classifier lazily. There is no fallback/mock prediction path —
    if no model has been trained (see training/train.py), predict() raises rather than
    returning a fabricated result."""

    def __init__(self) -> None:
        self._settings = get_settings()
        self._model = None
        self._model_version: str | None = None

    def _model_path(self) -> Path:
        return Path(self._settings.model_dir) / self._settings.active_model_filename

    def _metadata_path(self) -> Path:
        return Path(self._settings.model_dir) / self._settings.model_metadata_filename

    def is_ready(self) -> bool:
        return self._model_path().exists()

    def _ensure_loaded(self) -> None:
        if self._model is not None:
            return
        if not self.is_ready():
            raise ModelNotTrainedError(
                f"No trained model found at {self._model_path()}. Run training/train.py first."
            )
        self._model = joblib.load(self._model_path())
        metadata_path = self._metadata_path()
        self._model_version = (
            json.loads(metadata_path.read_text())["model_version"] if metadata_path.exists() else "unknown"
        )

    @staticmethod
    def to_feature_vector(features: UrlFeatures) -> np.ndarray:
        raw = features.model_dump()
        raw["domain_age_days"] = raw["domain_age_days"] if raw["domain_age_days"] is not None else -1
        return np.array([[float(raw[name]) for name in FEATURE_ORDER]])

    def predict(self, features: UrlFeatures) -> tuple[str, float, str]:
        self._ensure_loaded()
        vector = self.to_feature_vector(features)
        probability = float(self._model.predict_proba(vector)[0][1])
        prediction = "phishing" if probability >= 0.5 else "benign"
        return prediction, probability, self._model_version or "unknown"


_model_service = ModelService()


def get_model_service() -> ModelService:
    return _model_service
