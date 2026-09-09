from pathlib import Path

import pandas as pd

REQUIRED_COLUMNS = {
    "url_length", "domain_length", "path_length", "query_length",
    "subdomain_count", "dot_count", "hyphen_count", "digit_count", "special_char_count",
    "is_ip_address_host", "is_https", "has_url_encoding", "has_punycode",
    "is_shortened_url", "has_suspicious_tld", "has_redirect_parameter",
    "domain_age_days", "redirect_count", "has_mail_configuration",
    "ssl_is_valid", "ssl_domain_matches", "label",
}


def load_dataset(csv_path: str | Path) -> pd.DataFrame:
    """Loads a labeled feature dataset (one row per historical scan). `label` is 1 for
    confirmed phishing/malicious, 0 for confirmed benign. Raises if required columns
    are missing rather than silently training on a malformed dataset."""
    df = pd.read_csv(csv_path)
    missing = REQUIRED_COLUMNS - set(df.columns)
    if missing:
        raise ValueError(f"Dataset {csv_path} is missing required columns: {sorted(missing)}")
    df["domain_age_days"] = df["domain_age_days"].fillna(-1)
    return df
