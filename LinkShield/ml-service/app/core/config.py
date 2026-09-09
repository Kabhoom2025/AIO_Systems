from functools import lru_cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="LINKSHIELD_ML_", env_file=".env", extra="ignore")

    service_name: str = "LinkShield ML Service"
    api_prefix: str = "/api/v1"
    model_dir: str = "models"
    active_model_filename: str = "phishing_classifier.joblib"
    model_metadata_filename: str = "phishing_classifier.metadata.json"


@lru_cache
def get_settings() -> Settings:
    return Settings()
