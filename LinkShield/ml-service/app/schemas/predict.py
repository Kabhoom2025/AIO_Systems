from pydantic import BaseModel, Field


class UrlFeatures(BaseModel):
    """Feature vector expected by the classifier. Populated by the .NET Risk Engine
    from UrlAnalysis/DomainAnalysis/DnsAnalysis/SslAnalysis results for a single scan."""

    url_length: int
    domain_length: int
    path_length: int
    query_length: int
    subdomain_count: int
    dot_count: int
    hyphen_count: int
    digit_count: int
    special_char_count: int
    is_ip_address_host: bool
    is_https: bool
    has_url_encoding: bool
    has_punycode: bool
    is_shortened_url: bool
    has_suspicious_tld: bool
    has_redirect_parameter: bool
    domain_age_days: int | None = Field(default=None, description="None when RDAP lookup failed or domain unresolvable")
    redirect_count: int = 0
    has_mail_configuration: bool = False
    ssl_is_valid: bool = False
    ssl_domain_matches: bool = False


class PredictionRequest(BaseModel):
    scan_id: str
    features: UrlFeatures


class PredictionResponse(BaseModel):
    prediction: str = Field(description="'phishing' or 'benign'")
    probability: float = Field(ge=0.0, le=1.0, description="Probability of the predicted class")
    model_version: str
