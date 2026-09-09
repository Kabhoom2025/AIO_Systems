# LinkShield AI — ML Service

`ml-service/` is a standalone FastAPI app (Python 3.12+), separate from the .NET solution,
called by `LinkShield.Infrastructure` over HTTP (`MlService:BaseUrl` in `appsettings.json`,
default `http://localhost:8001`).

## Layout

```
ml-service/
  app/
    main.py                  FastAPI app, /health, mounts /api/v1/predict
    api/predict.py           POST /api/v1/predict
    core/config.py           pydantic-settings config (env prefix LINKSHIELD_ML_)
    schemas/predict.py       UrlFeatures / PredictionRequest / PredictionResponse
    services/model_service.py  Lazy-loads the joblib model; predict() raises
                                ModelNotTrainedError (-> HTTP 503) if no model exists yet
  models/                     Trained artifact + metadata JSON land here (gitignored contents)
  training/
    dataset_loader.py        Loads + validates a labeled CSV feature dataset
    train.py                 RandomForestClassifier training + evaluation + versioned save
  tests/
    test_model_service.py
```

## Why prediction fails loudly instead of guessing

`ModelService.predict()` has no fallback path. Until `training/train.py` has been run against a
real labeled dataset and produced `models/phishing_classifier.joblib`, `/api/v1/predict` returns
`503 Model not trained yet` rather than a fabricated probability — the platform's accuracy
requirement (spec section 33) forbids claiming a prediction that was never actually learned.

## Training a model

```bash
cd ml-service
pip install -r requirements.txt
python training/train.py --dataset path/to/labeled_scans.csv --model-version 2026.08.1
```

`train.py` prints `classification_report` plus precision/recall/F1/ROC-AUC/confusion-matrix
JSON, and only writes the model artifact after evaluating on a held-out test split — it never
reports training-set accuracy as if it were test accuracy.

## Feature contract

`app/schemas/predict.py::UrlFeatures` is the single source of truth for the feature vector shape
(`FEATURE_ORDER` in `model_service.py` and `train.py` must stay in lock-step). The .NET side
populates this from `UrlAnalysis`/`DomainAnalysis`/`DnsAnalysis`/`SslAnalysis`/
`RedirectAnalysis` results for a scan before calling `/api/v1/predict` — see `docs/database.md`
for where each field comes from.

## Status

Scaffolded and testable (`pytest ml-service/tests`), but no model has been trained yet — there
is no dataset in this repo. Training a real model is a separate, explicit step once labeled scan
data exists (spec section 14: "Do not claim accuracy without proper test-set evaluation").
