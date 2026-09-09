"""Trains the phishing-URL classifier from a labeled feature dataset and writes a
versioned model artifact to ml-service/models/. Run manually / via CI, never at
request time — inference (app/services/model_service.py) only ever reads the
artifact this script produces.

Usage:
    python training/train.py --dataset data/scans.csv --model-version 2026.08.1
"""

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path

import joblib
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import (
    classification_report,
    confusion_matrix,
    f1_score,
    precision_score,
    recall_score,
    roc_auc_score,
)
from sklearn.model_selection import train_test_split

from dataset_loader import load_dataset

FEATURE_ORDER = [
    "url_length", "domain_length", "path_length", "query_length",
    "subdomain_count", "dot_count", "hyphen_count", "digit_count", "special_char_count",
    "is_ip_address_host", "is_https", "has_url_encoding", "has_punycode",
    "is_shortened_url", "has_suspicious_tld", "has_redirect_parameter",
    "domain_age_days", "redirect_count", "has_mail_configuration",
    "ssl_is_valid", "ssl_domain_matches",
]


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", required=True, help="Path to labeled CSV dataset")
    parser.add_argument("--model-version", required=True)
    parser.add_argument("--output-dir", default="../models")
    parser.add_argument("--test-size", type=float, default=0.2)
    parser.add_argument("--random-state", type=int, default=42)
    args = parser.parse_args()

    df = load_dataset(args.dataset)
    X = df[FEATURE_ORDER].astype(float)
    y = df["label"].astype(int)

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=args.test_size, random_state=args.random_state, stratify=y
    )

    model = RandomForestClassifier(n_estimators=200, max_depth=12, random_state=args.random_state)
    model.fit(X_train, y_train)

    y_pred = model.predict(X_test)
    y_proba = model.predict_proba(X_test)[:, 1]

    metrics = {
        "precision": precision_score(y_test, y_pred),
        "recall": recall_score(y_test, y_pred),
        "f1": f1_score(y_test, y_pred),
        "roc_auc": roc_auc_score(y_test, y_proba),
        "confusion_matrix": confusion_matrix(y_test, y_pred).tolist(),
    }
    print(classification_report(y_test, y_pred))
    print(json.dumps(metrics, indent=2))

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    joblib.dump(model, output_dir / "phishing_classifier.joblib")
    (output_dir / "phishing_classifier.metadata.json").write_text(json.dumps({
        "model_version": args.model_version,
        "trained_at_utc": datetime.now(timezone.utc).isoformat(),
        "training_rows": len(df),
        "feature_order": FEATURE_ORDER,
        "metrics": metrics,
    }, indent=2))


if __name__ == "__main__":
    main()
