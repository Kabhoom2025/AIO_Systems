import pytest

from app.schemas.predict import UrlFeatures
from app.services.model_service import ModelNotTrainedError, ModelService


def make_features(**overrides) -> UrlFeatures:
    defaults = dict(
        url_length=42, domain_length=12, path_length=10, query_length=0,
        subdomain_count=1, dot_count=2, hyphen_count=0, digit_count=0, special_char_count=0,
        is_ip_address_host=False, is_https=True, has_url_encoding=False, has_punycode=False,
        is_shortened_url=False, has_suspicious_tld=False, has_redirect_parameter=False,
        domain_age_days=400, redirect_count=0, has_mail_configuration=True,
        ssl_is_valid=True, ssl_domain_matches=True,
    )
    defaults.update(overrides)
    return UrlFeatures(**defaults)


def test_predict_raises_when_model_not_trained(tmp_path):
    service = ModelService()
    service._settings.model_dir = str(tmp_path)
    with pytest.raises(ModelNotTrainedError):
        service.predict(make_features())


def test_feature_vector_orders_and_fills_missing_domain_age():
    features = make_features(domain_age_days=None)
    vector = ModelService.to_feature_vector(features)
    assert vector.shape == (1, 21)
    assert vector[0][16] == -1.0
